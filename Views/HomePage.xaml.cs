using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.Services;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class HomePage : ContentPage
{
    private IWoocommerceServices _woocommerceServices;
    public HomePage(IWoocommerceServices woocommerceServices)
    {
        InitializeComponent();
        _woocommerceServices = woocommerceServices;
        BindingContext = new MainViewModel(_woocommerceServices);

        // Hide navigation bar
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var vm = BindingContext as MainViewModel;
        if (vm.Products.Count == 0)
            vm.LoadProductsCommand.Execute(null);
        if (vm != null)
        {
            vm.LoadCategories(false);
        }
    }

    private async void OnCartClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CartPage(_woocommerceServices));
    }

    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        var searchBar = (SearchBar)sender;
        DisplayAlert("Search", $"Searching for: {searchBar.Text}", "OK");
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

    private void OnHomeClicked(object sender, EventArgs e) { }

    private async void OnCategoriesClicked(object sender, EventArgs e)
    {
        var vm = BindingContext as MainViewModel;
        await Navigation.PushAsync(new CategoryPage(_woocommerceServices));
    }

    private async void OnOrdersClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Orders", "Orders page would open here", "OK");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Profile", "Profile page would open here", "OK");
    }
     
    private async void OnCategoryTapped(object sender, EventArgs e)
    {
        if ((sender as Frame)?.BindingContext is Category selectedCategory)
        {
            if (!selectedCategory.IsMore)
            {
                var page = new CategoryProductPage(_woocommerceServices, selectedCategory.Id, selectedCategory.Name);
                await Navigation.PushAsync(page);
            }
            else
            {
                await Shell.Current.GoToAsync("//CategoryPage");
            }
        }
    }
    private async void OnProductTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is Product product)
        {
            // Navigate to product detail page with product ID
            await Navigation.PushAsync(new ProductDetailPage(product.Id, _woocommerceServices));
        }
    }
}

