using Watchzone.Models;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class CategoryProductPage : ContentPage
{
    private readonly CategoryProductsViewModel vm;

    public CategoryProductPage(int categoryId, string categoryName)
    {
        InitializeComponent();
        vm = new CategoryProductsViewModel(categoryId, categoryName);
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
}