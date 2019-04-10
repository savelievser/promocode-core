using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using PromoCodesWebApp.Models;
using Newtonsoft.Json;

namespace PromoCodesWebApp.Services
{
    public class PromoCodeRepositoryCacheDecorator : IPromoCodeRepository
    {
        private readonly IPromoCodeRepository _innerRepository;
        private readonly IDistributedCache _distributedCache;
        private const string MissedObjectString = "null";

        public PromoCodeRepositoryCacheDecorator(IPromoCodeRepository innerRepository, IDistributedCache distributedCache)
        {
            _innerRepository = innerRepository;
            _distributedCache = distributedCache;
        }

        public Task<PageResult<PromoCode>> GetAsync(int skip, int top)
        {
            return _innerRepository.GetAsync(skip, top);
        }

        // Every call to _distributedCache wrapped with try\catch, because any problems with cache should lead to crash

        public async Task<PromoCode> GetAsync(string key)
        {
            var cachedValue = await _distributedCache.GetStringAsync(key);
            PromoCode code;
            if (cachedValue != null)
            {
                if (cachedValue == MissedObjectString)
                    return null;
                code = (PromoCode) JsonConvert.DeserializeObject(cachedValue, typeof(PromoCode));
            }
            else
            {
                code = await _innerRepository.GetAsync(key);
                if (code != null)
                    await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(code));
                else
                    await _distributedCache.SetStringAsync(key, MissedObjectString);
            }
            return code;
        }

        public async Task CreateAsync(PromoCode code)
        {
            await _innerRepository.CreateAsync(code);
            await _distributedCache.SetStringAsync(code.Key, JsonConvert.SerializeObject(code));
        }

        public async Task UpdateAsync(string key, PromoCode code)
        {
            await _innerRepository.UpdateAsync(key, code);
            await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(code));
        }

        public async Task DeleteAsync(string key)
        {
            await _innerRepository.DeleteAsync(key);
            await _distributedCache.RemoveAsync(key);
        }
    }
}
