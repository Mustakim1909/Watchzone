using Watchzone.Interfaces;
using Watchzone.ViewModels;

namespace Watchzone.Views
{
    public partial class CheckoutPage : ContentPage
    {
        private readonly IWoocommerceServices _wooCommerceService;

        public CheckoutPage(IWoocommerceServices wooCommerceService)
        {
            InitializeComponent();
            _wooCommerceService = wooCommerceService;
            BindingContext = new CheckoutViewModel(_wooCommerceService);
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
    }
}
