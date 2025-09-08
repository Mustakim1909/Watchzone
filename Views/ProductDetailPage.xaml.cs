using Watchzone.Interfaces;
using Watchzone.ViewModels;

namespace Watchzone.Views;

public partial class ProductDetailPage : ContentPage
{
    private readonly IWoocommerceServices _woocommerceServices;
    private readonly ProductViewModel _viewModel;
    public ProductDetailPage(int productId, IWoocommerceServices woocommerceServices)
	{
		InitializeComponent();
        _woocommerceServices = woocommerceServices;

        // Initialize ViewModel
        _viewModel = new ProductViewModel(_woocommerceServices, productId);
        BindingContext = _viewModel;

        // Hide navigation bar
        NavigationPage.SetHasNavigationBar(this, false);

        // Load product data
        LoadProductData(productId);
    }
    private async void LoadProductData(int productId)
    {
        _viewModel.LoadProduct(productId);
         _viewModel.LoadReviews(productId);
    }
}