using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using GiapTech.Ipages.Api;
using GiapTech.Ipages.Application;
using GiapTech.Ipages.Infrastructure;
using GiapTech.Ipages.Infrastructure.BackgroundJobs;
using GiapTech.Ipages.Infrastructure.MultiTenant;
using GiapTech.Ipages.Infrastructure.Persistence;
using GiapTech.Ipages.Infrastructure.Persistence.Seed;
using Hangfire;
using Hangfire.Dashboard;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day));

    // Application & Infrastructure
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "GiapTech.Ipages API",
            Version = "v1",
            Description = "Multi-Tenant SaaS Platform API"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT token"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // JWT Authentication
    var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
    var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql")
        .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

    // Per-tenant rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("per-tenant", ctx =>
        {
            var host = ctx.Request.Host.Host;
            var dotIdx = host.IndexOf('.');
            var tenantKey = dotIdx > 0 ? host[..dotIdx] : ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(tenantKey, _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 300,
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            });
        });

        options.RejectionStatusCode = 429;
    });

    var app = builder.Build();

    // Migrate & Seed on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Ensuring database schema...");
            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("Database schema ready.");

            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();
            logger.LogInformation("Data seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during migration/seeding.");
            throw;
        }
    }

    // Register recurring Hangfire jobs
    var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<CleanupExpiredTokensJob>(
        "cleanup-expired-tokens",
        job => job.ExecuteAsync(),
        Cron.Hourly);

    // Middleware pipeline
    app.UseSerilogRequestLogging();
    app.UseHttpMetrics();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GiapTech.Ipages API v1"));

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<TenantMiddleware>();

    app.UseCors();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers().RequireRateLimiting("per-tenant");

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapMetrics("/metrics");

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireDashboardAuthFilter()],
        AppPath = null,
    });

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            || (httpContext.Connection.RemoteIpAddress != null && IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress));
    }
}
