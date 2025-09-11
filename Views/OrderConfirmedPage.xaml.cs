using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class OrderConfirmedPage : ContentPage
{
    private readonly IWoocommerceServices _woocommerceServices;
	public OrderConfirmedPage(IWoocommerceServices woocommerceServices, Order order)
	{
		InitializeComponent();
        _woocommerceServices = woocommerceServices;
        BindingContext = new OrderConfirmedViewModel(_woocommerceServices,order);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CheckoutViewModel viewModel)
        {
            // Load customer + cart data when page appears
            await viewModel.LoadData();
        }
    }
    private void OnContinueShoppingClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//HomePage");
    }

    private void OnViewOrderDetailsClicked(object sender, EventArgs e)
    {
        
    }
}
