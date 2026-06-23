using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Infrastructure.Persistence;
using GiapTech.Ipages.Infrastructure.Persistence.Seed;
using GiapTech.Ipages.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using StackExchange.Redis;

namespace GiapTech.Ipages.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Current tenant & user (scoped — one per request)
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis")!;
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<ICacheService, RedisCacheService>();

        // MinIO
        services.AddMinio(client =>
        {
            var host = configuration["MinioSettings:Host"]!;
            var port = int.Parse(configuration["MinioSettings:Port"] ?? "9000");
            var accessKey = configuration["MinioSettings:AccessKey"]!;
            var secretKey = configuration["MinioSettings:SecretKey"]!;
            var useSSL = bool.Parse(configuration["MinioSettings:UseSSL"] ?? "false");

            client.WithEndpoint(host, port)
                  .WithCredentials(accessKey, secretKey)
                  .WithSSL(useSSL)
                  .Build();
        });
        services.AddScoped<IStorageService, MinioStorageService>();

        // Other services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<DataSeeder>();

        return services;
    }
}
