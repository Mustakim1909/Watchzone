using Watchzone.Interfaces;
using Watchzone.Services;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class CartPage : ContentPage
{
    private IWoocommerceServices _woocommerceServices;
    public CartPage(IWoocommerceServices woocommerceServices)
    {
        InitializeComponent();
        _woocommerceServices = woocommerceServices;
        BindingContext = new CartViewModel(_woocommerceServices);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load customer data when the page appears
        if (BindingContext is CartViewModel viewModel)
        {
            await viewModel.LoadCustomerData();
        }
    }
}
