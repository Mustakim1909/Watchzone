using CommunityToolkit.Mvvm.DependencyInjection;
using Watchzone.Models;
using Watchzone.Services;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class HomePage : ContentPage
{
    private WoocommerceServices _woocommerceServices;
    public HomePage(WoocommerceServices woocommerceServices)
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

    private void OnCartClicked(object sender, EventArgs e)
    {
        DisplayAlert("Cart", "Cart button clicked", "OK");
    }

    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        var searchBar = (SearchBar)sender;
        DisplayAlert("Search", $"Searching for: {searchBar.Text}", "OK");
    }

    private void OnAddToCartClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var product = (Product)button.CommandParameter;
        DisplayAlert("Added to Cart", $"{product.Name} added to cart", "OK");
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

