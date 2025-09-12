using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;

        public Product Product { get; set; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(DisplayPrice));
            }
        }

        public decimal TotalPrice
        {
            get
            {
                if (decimal.TryParse(Product.Price, out decimal price))
                    return price * Quantity;
                return 0;
            }
        }

        public string DisplayPrice
        {
            get
            {
                if (decimal.TryParse(Product.Price, out decimal price))
                    return (price * Quantity).ToString("C");
                return "₹0.00";
            }
        }

        public string Subtotal
        {
            get
            {
                if (decimal.TryParse(Product.Price, out decimal price))
                    return $"{price} × {Quantity.ToString()}";
                return "0 × ₹0.00";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class CartStorageItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public string TotalPrice { get; set; }
        }
    }
}
