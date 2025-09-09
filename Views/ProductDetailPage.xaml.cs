using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class ProductDetailPage : ContentPage
{
    private readonly IWoocommerceServices _woocommerceServices;
    private readonly ProductViewModel _viewModel;
    public ProductDetailPage(int productId, IWoocommerceServices woocommerceServices)
	{
		InitializeComponent();
        _woocommerceServices = woocommerceServices;

        // Initialize ViewModel
        _viewModel = new ProductViewModel(_woocommerceServices, productId);
        BindingContext = _viewModel;

        // Hide navigation bar
        NavigationPage.SetHasNavigationBar(this, false);

        // Load product data
        LoadProductData(productId);
    }
    private async void LoadProductData(int productId)
    {
        _viewModel.LoadProduct(productId);
        _viewModel.LoadReviews(productId);
    }
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
}