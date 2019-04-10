using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PromoCodesWebApp.Models;

namespace PromoCodesWebApp.Services
{
    public class AntiFraudService
    {
        private const string CachePrefix = "fraud_";
        private static readonly (int MaxTries, TimeSpan BlockPeriod, TimeSpan RemitPeriod)[] _levelMap = new (int, TimeSpan, TimeSpan)[]
        {
            (1, TimeSpan.FromSeconds(0), TimeSpan.FromHours(1)),
            (5, TimeSpan.FromMinutes(1), TimeSpan.FromHours(5)),
            (10, TimeSpan.FromMinutes(30), TimeSpan.FromHours(12)),
            (15, TimeSpan.FromHours(24), TimeSpan.FromDays(3))
        };

        private readonly IDistributedCache _distributedCache;
        
        public AntiFraudService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        /// Returns false if user was blocked
        public async Task<bool> Accept(string userId)
        {
            var key = CachePrefix + userId;
            var cacheStr = await _distributedCache.GetStringAsync(key);
            if (cacheStr == null)
            {
                return true;
            }

            var suspect = (FraudSuspect) JsonConvert.DeserializeObject(cacheStr, typeof(FraudSuspect));
            var levelInfo = _levelMap[suspect.Level];
            if (suspect.IsBlocked && suspect.StartTime.Add(levelInfo.BlockPeriod) > DateTime.Now)
                return false;
            return true;
        }

        /// Returns false if user was blocked
        public async Task<bool> HandleMiss(string userId)
        {
            var key = CachePrefix + userId;
            var cacheStr = await _distributedCache.GetStringAsync(key);
            if (cacheStr == null)
            {
                await _distributedCache.SetStringAsync(key,
                    JsonConvert.SerializeObject(new FraudSuspect() { UserId = userId, StartTime = DateTime.Now, TriesCount = 1}));
                return true;
            }
            else
            {
                var suspect = (FraudSuspect)JsonConvert.DeserializeObject(cacheStr, typeof(FraudSuspect));
                if (await IncreaseSuspect(suspect))
                    return false;
                return true;
            }
        }
        
        /// Returns false if user was blocked
        private async Task<bool> IncreaseSuspect(FraudSuspect suspect)
        {
            var now = DateTime.Now;
            var key = CachePrefix + suspect.UserId;
            var levelInfo = _levelMap[suspect.Level];

            if (suspect.IsBlocked && suspect.StartTime.Add(levelInfo.BlockPeriod) <= now)
            {
                // client was blocked, but block period expired
                suspect.IsBlocked = false;
            }

            // In theory this case impossible, because we check blocked clients at the beggining
            if (suspect.IsBlocked)
                return true;

            var startTime = suspect.StartTime;
            while (startTime.Add(levelInfo.RemitPeriod) <= now)
            {
                // client was reached level long time ago - decrease his level
                if (suspect.Level == 0)
                {
                    suspect.TriesCount = 0;
                    suspect.StartTime = now;
                    break;
                }
                suspect.Level--;
                startTime += levelInfo.RemitPeriod;
                levelInfo = _levelMap[suspect.Level];
                suspect.TriesCount = _levelMap[suspect.Level].MaxTries;
            }

            suspect.TriesCount++;
            if (suspect.Level + 1 >= _levelMap.Length || suspect.TriesCount >= _levelMap[suspect.Level + 1].MaxTries)
            {
                suspect.StartTime = now;
                suspect.IsBlocked = true;
                if (suspect.Level + 1 < _levelMap.Length)
                {
                    suspect.Level++;
                }
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(suspect));
                return true;
            }
            else
            {
                await _distributedCache.SetStringAsync(key, JsonConvert.SerializeObject(suspect));
                return false;
            }
        }
    }
}
