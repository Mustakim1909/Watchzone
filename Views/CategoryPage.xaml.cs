using CommunityToolkit.Mvvm.DependencyInjection;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.Services;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class CategoryPage : ContentPage
{
    private IWoocommerceServices _woocommerceServices;
    public CategoryPage(IWoocommerceServices woocommerceServices)
    {
        InitializeComponent();
        _woocommerceServices = woocommerceServices;
        BindingContext = new MainViewModel(_woocommerceServices);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as MainViewModel).LoadCategories(true);
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
                await DisplayAlert("Info", "More categories...", "OK");
            }
        }
    }



}