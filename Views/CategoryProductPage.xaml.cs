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
}