using GiapTech.Ipages.Application.Common.Interfaces;
using GiapTech.Ipages.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GiapTech.Ipages.Infrastructure.MultiTenant;

public class TenantMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantService = context.RequestServices.GetRequiredService<ICurrentTenantService>();
        var baseDomain = configuration["BaseDomain"] ?? "localhost";
        var host = context.Request.Host.Host;

        if (host.EndsWith($".{baseDomain}", StringComparison.OrdinalIgnoreCase))
        {
            var subdomain = host[..^(baseDomain.Length + 1)];

            if (subdomain.Equals("host", StringComparison.OrdinalIgnoreCase))
            {
                tenantService.SetHostAdmin();
            }
            else
            {
                var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var tenant = await db.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Slug == subdomain);

                if (tenant != null)
                    tenantService.SetTenant(tenant.Id, tenant.Slug);
                else
                    tenantService.SetHostAdmin();
            }
        }
        else if (host.Equals(baseDomain, StringComparison.OrdinalIgnoreCase) ||
                 host.Equals($"host.{baseDomain}", StringComparison.OrdinalIgnoreCase))
        {
            tenantService.SetHostAdmin();
        }

        await next(context);
    }
}
