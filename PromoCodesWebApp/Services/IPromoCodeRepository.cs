using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PromoCodesWebApp.Models;


namespace PromoCodesWebApp.Services
{
    public interface IPromoCodeRepository
    {
        Task<PageResult<PromoCode>> GetAsync(int skip, int top);
        Task<PromoCode> GetAsync(string key);
        Task CreateAsync(PromoCode code);
        Task UpdateAsync(string key, PromoCode code);
        Task DeleteAsync(string key);
    }
}
