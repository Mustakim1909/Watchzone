using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Models
{
    public class Coupon
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("discount_type")]
        public string DiscountType { get; set; } // percent, fixed_cart, fixed_product

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("date_expires")]
        public DateTime? DateExpires { get; set; }

        [JsonProperty("minimum_amount")]
        public string MinimumAmount { get; set; }

        [JsonProperty("maximum_amount")]
        public string MaximumAmount { get; set; }
    }
    public class CouponLine
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("discount")]
        public decimal Discount { get; set; }
    }
}
