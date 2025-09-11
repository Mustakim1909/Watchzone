using Watchzone.Interfaces;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class OrdersPage : ContentPage
{
    private readonly OrdersViewModel _viewModel;
    private readonly IWoocommerceServices _woocommerceServices;
    public OrdersPage(IWoocommerceServices woocommerceServices)
	{
		InitializeComponent();
        _woocommerceServices = woocommerceServices; 
        _viewModel = new OrdersViewModel(_woocommerceServices);
        BindingContext = _viewModel;
    }
    
}