using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.Services;

namespace Watchzone.ViewModels
{
    public class ProductViewModel : INotifyPropertyChanged
    {
        private Product _product;
        private int _quantity = 1;
        private List<Review> _reviews;
        private readonly IWoocommerceServices _wooCommerceService;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged();
            }
        }

        public List<Review> Reviews
        {
            get => _reviews;
            set
            {
                _reviews = value;
                OnPropertyChanged();
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand BuyNowCommand { get; }

        public ProductViewModel(IWoocommerceServices woocommerceServices, int productId)
        {
            _wooCommerceService = woocommerceServices;
            AddToCartCommand = new Command(AddToCart);
            BuyNowCommand = new Command(BuyNow);

            LoadProduct(productId);
            LoadReviews(productId);
        }

        public async void LoadProduct(int productId)
        {
            // Fetch product data from WooCommerce
            Product = await _wooCommerceService.GetProductAsync(productId);
        }

        public async void LoadReviews(int productId)
        {
            // Fetch reviews from WooCommerce
            Reviews = await _wooCommerceService.GetProductReviewsAsync(productId);
        }

        private void AddToCart()
        {
            // Add to cart logic
            //CartService.AddItem(Product, Quantity);
            Application.Current.MainPage.DisplayAlert("Success", "Product added to cart!", "OK");
        }

        private async void BuyNow()
        {
            // Buy now logic
           // CartService.AddItem(Product, Quantity);
            await Shell.Current.GoToAsync("//cart");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
