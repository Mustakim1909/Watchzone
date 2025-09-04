using Watchzone.Models;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class CategoryPage : ContentPage
{
    public CategoryPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as MainViewModel).LoadCategories(true);
    }
    private async void OnCategorySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Category selectedCategory)
        {
            if (!selectedCategory.IsMore) // "More" pe click ignore
            {
                await Navigation.PushAsync(new CategoryProductPage(selectedCategory.Id, selectedCategory.Name));
            }

            ((CollectionView)sender).SelectedItem = null; // clear selection
        }
    }
    private async void OnCategoryTapped(object sender, EventArgs e)
    {
        if ((sender as Frame)?.BindingContext is Category selectedCategory)
        {
            if (!selectedCategory.IsMore)
            {
                await Navigation.PushAsync(new CategoryProductPage(selectedCategory.Id, selectedCategory.Name));
            }
            else
            {
                await DisplayAlert("Info", "More categories...", "OK");
            }
        }
    }



}