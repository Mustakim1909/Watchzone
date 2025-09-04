using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Total { get; set; }
        public DateTime DateCreated { get; set; }
        public List<OrderItem> LineItems { get; set; }
    }
    public class OrderItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
    }
}
