using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using PromoCodesWebApp.Config;
using PromoCodesWebApp.Models;

namespace PromoCodesWebApp.Services
{
    public class PromoCodeMongoDbRepository : IPromoCodeRepository
    {
        private readonly IMongoCollection<PromoCode> _codes;

        public PromoCodeMongoDbRepository(AppConfig config)
        {
            var client = new MongoClient(config.MongoDB.ConnectionString);
            var database = client.GetDatabase(config.MongoDB.Database);
            _codes = database.GetCollection<PromoCode>("PromoCodes");
        }

        public async Task<PageResult<PromoCode>> GetAsync(int skip, int top)
        {
            var query = _codes.Find(_ => true);
            var totalTask = query.CountDocumentsAsync();
            var itemsTask = query.Skip(skip).Limit(top).ToListAsync();
            await Task.WhenAll(totalTask, itemsTask);
            return new PageResult<PromoCode> { Total = totalTask.Result, Items = itemsTask.Result };
        }

        public Task<PromoCode> GetAsync(string key)
        {
            var normKey = NormalizeKey(key);
            return _codes.Find(c => c.Key == normKey).FirstOrDefaultAsync();
        }

        public Task CreateAsync(PromoCode code)
        {
            code.Key = NormalizeKey(code.Key);
            return _codes.InsertOneAsync(code);
        }

        public Task UpdateAsync(string key, PromoCode code)
        {
            var normKey = NormalizeKey(key);
            code.Key = normKey;
            return _codes.ReplaceOneAsync(c => c.Key == normKey, code);
        }

        public Task DeleteAsync(string key)
        {
            var normKey = NormalizeKey(key);
            return _codes.DeleteOneAsync(c => c.Key == normKey);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string NormalizeKey(string key) => key.ToLowerInvariant();
    }
}
