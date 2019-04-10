using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodesWebApp.Services
{
    public class PromoCodeValidationException : Exception
    {
        public PromoCodeValidationException(string message) : base(message)
        {
        }
    }
}
