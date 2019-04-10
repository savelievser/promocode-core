using PromoCodesWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PromoCodesWebApp.Services
{
    public class PromoCodeService
    {
        private const int KeyLength = 8;
        private readonly IPromoCodeRepository _codeRepository;
        private static readonly Regex _keyRegex = new Regex("^([0-9]|[a-z]){8}$");
        private static readonly string _keyPossibleChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly Random _random = new Random();

        public PromoCodeService(IPromoCodeRepository codeRepository)
        {
            _codeRepository = codeRepository;
        }

        public Task<PageResult<PromoCode>> GetAsync(int skip, int top) => _codeRepository.GetAsync(skip, top);

        public Task<PromoCode> GetAsync(string key) => _codeRepository.GetAsync(key);

        public Task UpdateAsync(string key, PromoCode code)
        {
            code.Key = key;
            if (!Validate(code, out var msg)) throw new PromoCodeValidationException(msg);
            return _codeRepository.UpdateAsync(key, code);
        }

        public Task DeleteAsync(string key) => _codeRepository.DeleteAsync(key);

        public async Task<IEnumerable<string>> GenerateAsync(int count)
        {
            var created = new string[count];
            var i = 0;
            while (i < count)
            {
                var key = GenerateKey();
                if ((await _codeRepository.GetAsync(key)) == null)
                {
                    await _codeRepository.CreateAsync(new PromoCode() {Key = key});
                    created[i++] = key;
                }
            }
            return created;
        }

        private bool Validate(PromoCode code, out string message)
        {
            message = null;
            if (String.IsNullOrEmpty(code.Key))
            {
                message = "Key is empty";
                return false;
            }
            
            if (!_keyRegex.IsMatch(code.Key))
            {
                message = "Key doesn't match pattern of 8 latin letters or digits";
                return false;
            }

            if (code.AbsoluteValue < 0)
            {
                message = "AbsoluteValue must be positive";
                return false;
            }

            if (code.PercentValue < 0 || code.PercentValue > 100)
            {
                message = "PercentValue must be in range of [0,100]";
                return false;
            }
            return true;
        }
        
        private string GenerateKey()
        {
            var keyChars = new char[KeyLength];
            for (int i = 0; i < KeyLength; i++)
            {
                keyChars[i] = _keyPossibleChars[_random.Next(_keyPossibleChars.Length)];
            }
            return new String(keyChars);
        }
    }
}
