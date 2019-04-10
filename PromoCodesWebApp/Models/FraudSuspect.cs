using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PromoCodesWebApp.Models
{
    public class FraudSuspect
    {
        public string UserId { get; set; }
        public bool IsBlocked { get; set; }
        /// Time of first attempt to retrieve non-existing record on current level.
        public DateTime StartTime { get; set; }
        public int Level { get; set; }
        public int TriesCount { get; set; }
    }
}
