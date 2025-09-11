using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchzone.Interfaces;
using Watchzone.Models;

namespace Watchzone.ViewModels
{
    public class OrderConfirmedViewModel : BaseViewModel
    {
        private readonly IWoocommerceServices _wooService;

        public int OrderId { get; set; }
        public ObservableCollection<CartItem> CartItems { get; set; } = new();
        public Address ShippingAddress { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }

        public string PaymentStatus => PaymentMethod == "cod" ? "To be paid on delivery" : "Paid";
        public DateTime EstimatedDeliveryDate { get; set; }

        public OrderConfirmedViewModel(IWoocommerceServices wooService, Order order)
        {
            _wooService = wooService; 


            OrderId = order.Id;
            ShippingAddress = order.Shipping;
            Total = order.Total;
            PaymentMethod = order.PaymentMethod;
            ShippingCost = order.ShippingTotal;
            TaxAmount = order.TotalTax;
            DiscountAmount = order.TotalDiscount;
            EstimatedDeliveryDate = order.DateCreated.AddDays(5);

            // Line items se product details load karo
            Task.Run(async () => await LoadCartItems(order));
        }

        private async Task LoadCartItems(Order order)
        {
            foreach (var line in order.LineItems)
            {
                var product = await _wooService.GetProductAsync(line.ProductId);

                CartItems.Add(new CartItem
                {
                    Product = product,
                    Quantity = line.Quantity
                });
            }

            Subtotal = CartItems.Sum(x => x.TotalPrice);
           
            OnPropertyChanged(nameof(CartItems));
            OnPropertyChanged(nameof(Subtotal));    
            OnPropertyChanged(nameof(DiscountAmount));    
            OnPropertyChanged(nameof(TaxAmount));    
            OnPropertyChanged(nameof(ShippingCost));    
        }
    }


}
