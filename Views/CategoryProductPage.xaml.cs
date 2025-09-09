using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.DependencyInjection;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.Services;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class CategoryProductPage : ContentPage
{
    private readonly CategoryProductsViewModel vm;
    private readonly IWoocommerceServices _woocommerceServices;

    public CategoryProductPage(IWoocommerceServices woocommerceServices,int categoryId, string categoryName)
    {
        InitializeComponent();
        vm = new CategoryProductsViewModel(woocommerceServices, categoryId, categoryName);
        _woocommerceServices = woocommerceServices;

        // Bind the UI to this instance
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await vm.LoadProductsAsync();
    }

    private async void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as Product;
        if (selected == null) return;

        // TODO: Navigate to product detail page
        await DisplayAlert(selected.Name, $"Open product detail for ID {selected.Id}", "OK");

        // clear selection
        ((CollectionView)sender).SelectedItem = null;
    }
    private async void OnProductTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Product product)
        {
            // Navigate to product detail page with product ID
            await Navigation.PushAsync(new ProductDetailPage(product.Id, _woocommerceServices));
        }
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
                        BackgroundColor = Colors.DarkGreen,
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