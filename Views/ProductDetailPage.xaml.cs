using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Windows.Input;
using Watchzone.Converters;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class ProductDetailPage : ContentPage
{
    private readonly IWoocommerceServices _woocommerceServices;
    private readonly ProductViewModel _viewModel;
    private int _selectedQuantity = 1;
    public ProductDetailPage(int productId, IWoocommerceServices woocommerceServices)
	{
		InitializeComponent();
        _woocommerceServices = woocommerceServices;

        // Initialize ViewModel
        _viewModel = new ProductViewModel(_woocommerceServices, productId);
        BindingContext = _viewModel;

        // Hide navigation bar
        NavigationPage.SetHasNavigationBar(this, false);
        var converter = (CurrentCustomerReviewConverter)this.Resources["CurrentCustomerReviewConverter"];
        converter.CurrentCustomerName = App.CurrentCustomerName;

        // Load product data
        LoadProductData(productId);
    }
    private async void LoadProductData(int productId)
    {
        _viewModel.LoadProduct(productId);
        _viewModel.LoadReviews(productId);
    }
    private async void OnSubmitReviewClicked(object sender, EventArgs e)
    {
        int customerId = App.CurrentCustomerId;
        if (customerId <= 0)
        {
            await DisplayAlert("Login Required", "Please login to submit a review.", "OK");
            return;
        }

        if (_viewModel.SelectedRating <= 0)
        {
            await DisplayAlert("Error", "Please select a rating.", "OK");
            return;
        }

        string reviewText = ReviewEditor.Text?.Trim();
        if (string.IsNullOrWhiteSpace(reviewText))
        {
            await DisplayAlert("Error", "Please enter a review.", "OK");
            return;
        }

        int rating = _viewModel.SelectedRating;

        var success = await _woocommerceServices.AddReviewAsync(_viewModel.Product.Id, customerId, rating, reviewText);

        if (success)
        {
            await DisplayAlert("Success", "Your review has been submitted.", "OK");
            ReviewEditor.Text = "";
            _viewModel.SelectedRating = 0; // reset stars
            _viewModel.LoadReviews(_viewModel.Product.Id);
        }
        else
        {
            await DisplayAlert("Error", "Failed to submit review.", "OK");
        }
    }


    // ✅ Delete Review Command
    public ICommand DeleteReviewCommand => new Command<Review>(async (review) =>
    {
        bool confirm = await DisplayAlert("Delete Review", "Are you sure you want to delete this review?", "Yes", "No");
        if (!confirm) return;

        var success = await _woocommerceServices.DeleteReviewAsync(review.Id);

        if (success)
        {
            await DisplayAlert("Deleted", "Your review has been deleted.", "OK");
            _viewModel.LoadReviews(_viewModel.Product.Id);
        }
        else
        {
            await DisplayAlert("Error", "Failed to delete review.", "OK");
        }
    });
    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var product = (Product)button.CommandParameter;
        if (IsBusy || product == null) return;

        IsBusy = true;
        try
        {
            int customerId = App.CurrentCustomerId;

            if (customerId <= 0)
            {
                var snackbar = Snackbar.Make(
                    "Please login first",
                    async () => await Shell.Current.GoToAsync("//LoginPage"),
                    "Login",
                    TimeSpan.FromSeconds(3),
                    new SnackbarOptions
                    {
                        BackgroundColor = Colors.DarkRed,   // 🔴 Background
                        TextColor = Colors.White,          // ⚪ Text
                        ActionButtonTextColor = Colors.Yellow, // 🟡 Action text
                        CornerRadius = new CornerRadius(8), // Rounded corners
                        CharacterSpacing = 1.2
                    });

                await snackbar.Show();
                return;
            }

            var success = await _woocommerceServices.AddToCartAsync(customerId, product.Id, 1);
            if (success)
            {
                var snackbar = Snackbar.Make(
                    $"Added to Cart",
                    async () => await Navigation.PushAsync(new CartPage(_woocommerceServices)),
                    "View Cart",
                    TimeSpan.FromSeconds(3),
                    new SnackbarOptions
                    {
                        BackgroundColor = Colors.Green,
                        TextColor = Colors.White,
                        ActionButtonTextColor = Colors.Orange,
                        CornerRadius = new CornerRadius(10)
                    });

                await snackbar.Show();
            }
            else
            {
                var snackbar = Snackbar.Make(
                    "Failed to add product to cart",
                    null,
                    null,
                    TimeSpan.FromSeconds(3),
                    new SnackbarOptions
                    {
                        BackgroundColor = Colors.Gray,
                        TextColor = Colors.White,
                        CornerRadius = new CornerRadius(5)
                    });

                await snackbar.Show();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private async void OnBuyNowClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var product = (Product)button.CommandParameter;
        if (IsBusy || product == null) return;

        IsBusy = true;
        try
        {
            int customerId = App.CurrentCustomerId;

            if (customerId <= 0)
            {
                var snackbar = Snackbar.Make(
                    "Please login first",
                    async () => await Shell.Current.GoToAsync("//LoginPage"),
                    "Login",
                    TimeSpan.FromSeconds(3),
                    new SnackbarOptions
                    {
                        BackgroundColor = Colors.DarkRed,
                        TextColor = Colors.White,
                        ActionButtonTextColor = Colors.Yellow,
                        CornerRadius = new CornerRadius(8),
                        CharacterSpacing = 1.2
                    });

                await snackbar.Show();
                return;
            }

            // ✅ Pehle cart me add karo
            var success = await _woocommerceServices.AddToCartAsync(customerId, product.Id, 1);

            if (success)
            {
                // ✅ Agar add ho gaya, to directly CheckoutPage khol do
                await Shell.Current.Navigation.PushAsync(new CheckoutPage(_woocommerceServices));
            }
            else
            {
                var snackbar = Snackbar.Make(
                    "Failed to add product to cart",
                    null,
                    null,
                    TimeSpan.FromSeconds(3),
                    new SnackbarOptions
                    {
                        BackgroundColor = Colors.Gray,
                        TextColor = Colors.White,
                        CornerRadius = new CornerRadius(5)
                    });

                await snackbar.Show();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

}