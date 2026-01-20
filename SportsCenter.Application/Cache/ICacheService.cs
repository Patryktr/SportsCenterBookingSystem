using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsCenter.Application.Cache
{
    public interface ICacheService
    {
        Task<(bool Found, T? Value)> TryGetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
        Task RemoveAsync(string key);

        Task<T> GetOrCreateAsync<T>(
            string key,
            Func<CancellationToken, Task<T>> valueFactory,
            TimeSpan? ttl = null,
            CancellationToken ct = default);
    }
}
