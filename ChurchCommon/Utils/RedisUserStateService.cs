using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ChurchCommon.Utils
{
    public class RedisUserStateService : IUserStateService
    {
        private readonly IDistributedCache _redis;

        public RedisUserStateService(IDistributedCache redis)
        {
            _redis = redis;
        }

        public async Task SetStateAsync(string userMobile, string state, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
            };
            await _redis.SetStringAsync(userMobile, state, options);
        }

        public async Task<string?> GetStateAsync(string userMobile)
        {
            return await _redis.GetStringAsync(userMobile);
        }

        public Task ClearStateAsync(string userMobile)
        {
            return _redis.RemoveAsync(userMobile);
        }
    }

}
