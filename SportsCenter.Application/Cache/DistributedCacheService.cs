using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SportsCenter.Application.Cache;

public sealed class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;
    private readonly JsonSerializerOptions _json;

    public DistributedCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<(bool Found, T? Value)> TryGetAsync<T>(string key)
    {
        var bytes = await _cache.GetAsync(key);
        if (bytes == null || bytes.Length == 0)
            return (false, default);

        return (true, JsonSerializer.Deserialize<T>(bytes, _json));
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var options = ttl.HasValue
            ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl }
            : _defaultOptions;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
        await _cache.SetAsync(key, bytes, options);
    }

    public Task RemoveAsync(string key) => _cache.RemoveAsync(key);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> valueFactory,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        var (found, cached) = await TryGetAsync<T>(key);
        if (found && cached is not null)
            return cached;

        var value = await valueFactory(ct);
        await SetAsync(key, value, ttl);
        return value;
    }
}