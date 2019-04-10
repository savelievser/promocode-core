using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PromoCodesWebApp.Models
{
    public class PromoCode
    {
        [Required]
        [BsonId]
        public string Key { get; set; }

        [BsonElement]
        public decimal? AbsoluteValue { get; set; }

        [BsonElement]
        public decimal? PercentValue { get; set; }

        [Required]
        [BsonElement]
        public DateTime? ExpireDate { get; set; }

        [BsonElement]
        public DateTime? ActivationDate { get; set; }
    }
}
