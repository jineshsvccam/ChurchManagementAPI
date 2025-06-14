using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ChurchCommon.Utils
{
    public class InMemoryUserStateService : IUserStateService
    {
        private readonly IMemoryCache _cache;

        public InMemoryUserStateService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task SetStateAsync(string userMobile, string state, TimeSpan? expiry = null)
        {
            _cache.Set(userMobile, state, expiry ?? TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        public Task<string?> GetStateAsync(string userMobile)
        {
            _cache.TryGetValue(userMobile, out string? state);
            return Task.FromResult(state);
        }

        public Task ClearStateAsync(string userMobile)
        {
            _cache.Remove(userMobile);
            return Task.CompletedTask;
        }
    }

}
