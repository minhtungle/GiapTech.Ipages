using GiapTech.Ipages.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiapTech.Ipages.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;
    private readonly ICurrentTenantService _tenant;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cache, ICurrentTenantService tenant, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _tenant = tenant;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not ICacheable cacheable)
            return await next();

        var tenantPrefix = _tenant.TenantId?.ToString("N") ?? "host";
        var key = $"{tenantPrefix}:{cacheable.CacheKey}";

        var cached = await _cache.GetAsync<TResponse>(key);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit: {Key}", key);
            return cached;
        }

        var result = await next();
        if (result is not null)
            await _cache.SetAsync(key, result, cacheable.CacheDuration);
        return result;
    }
}
