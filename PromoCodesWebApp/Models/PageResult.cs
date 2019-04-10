using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodesWebApp.Models
{
    public class PageResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public long Total { get; set; }
    }
}
