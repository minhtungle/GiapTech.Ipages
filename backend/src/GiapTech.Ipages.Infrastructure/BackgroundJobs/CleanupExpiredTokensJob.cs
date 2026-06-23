using GiapTech.Ipages.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GiapTech.Ipages.Infrastructure.BackgroundJobs;

public class CleanupExpiredTokensJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupExpiredTokensJob> _logger;

    public CleanupExpiredTokensJob(IServiceScopeFactory scopeFactory, ILogger<CleanupExpiredTokensJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoff = DateTime.UtcNow;
        var count = await db.Users
            .Where(u => u.RefreshTokenExpiresAt != null && u.RefreshTokenExpiresAt < cutoff)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.RefreshToken, (string?)null)
                .SetProperty(u => u.RefreshTokenExpiresAt, (DateTime?)null));

        _logger.LogInformation("Cleared {Count} expired refresh tokens", count);
    }
}
