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
        private int _selectedRating = 0;
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
        // ⭐ Selected Rating
        public int SelectedRating
        {
            get => _selectedRating;
            set
            {
                _selectedRating = value;
                OnPropertyChanged();
            }
        }

        // ⭐ Command to set rating
        public ICommand SetRatingCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand BuyNowCommand { get; }
        public ICommand DeleteReviewCommand { get; } 

        public ProductViewModel(IWoocommerceServices woocommerceServices, int productId)
        {
            _wooCommerceService = woocommerceServices;
            AddToCartCommand = new Command(AddToCart);
            BuyNowCommand = new Command(BuyNow);
            SetRatingCommand = new Command<string>(ratingParam =>
            {
                if (int.TryParse(ratingParam, out int rating))
                {
                    SelectedRating = rating;
                }
            });

            DeleteReviewCommand = new Command<Review>(async review =>
            {
                if (review.Reviewer != App.CurrentCustomerName)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "You cannot delete this review.", "OK");
                    return;
                }

                bool confirm = await Application.Current.MainPage.DisplayAlert("Delete Review", "Are you sure you want to delete this review?", "Yes", "No");
                if (!confirm) return;

                var success = await _wooCommerceService.DeleteReviewAsync(review.Id);
                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert("Deleted", "Your review has been deleted.", "OK");
                    LoadReviews(productId);
                 }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete review.", "OK");
                }
            });
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
