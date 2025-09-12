using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Models
{
    public class Order
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; } = new List<LineItem>();

        [JsonProperty("shipping")]
        public Address Shipping { get; set; } = new Address();

        [JsonProperty("billing")]
        public Address Billing { get; set; } = new Address();

        [JsonProperty("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("coupon_lines")]
        public List<CouponLine> CouponLines { get; set; } = new List<CouponLine>();
        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }
        [JsonProperty("shipping_total")]
        public decimal ShippingTotal { get; set; }

        [JsonProperty("total_tax")]
        public decimal TotalTax { get; set; }
        [JsonProperty("discount_total")]
        public decimal TotalDiscount { get; set; }
        public Color StatusColor
        {
            get
            {
                return Status switch
                {
                    "Delivered" => Color.FromArgb("#00A650"),
                    "Shipped" => Color.FromArgb("#007185"),
                    "Processing" => Color.FromArgb("#FF9900"),
                    "Cancelled" => Color.FromArgb("#FF0000"),
                    _ => Color.FromArgb("#666666")
                };
            }
        }

    }

    public class LineItem
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subtotal")]
        public string Subtotal { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }
        [JsonProperty("image")]
        public Image Images { get; set; }
    }

}
