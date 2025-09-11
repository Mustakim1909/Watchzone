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
        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var vm = BindingContext as ViewModels.CheckoutViewModel;
            if (vm == null) return;

            if (e.Value) // Checked
            {
                vm.SaveShippingAddress = true;
                vm.SaveAddressToPreferences(); // Save immediately
            }
            else // Unchecked
            {
                vm.SaveShippingAddress = false;

                // Optional: Clear saved address
                Preferences.Remove("SavedFirstName");
                Preferences.Remove("SavedLastName");
                Preferences.Remove("SavedPhone");
                Preferences.Remove("SavedAddress1");
                Preferences.Remove("SavedAddress2");
                Preferences.Remove("SavedCity");
                Preferences.Remove("SavedState");
                Preferences.Remove("SavedPostcode");
                Preferences.Remove("SavedCountry");
            }
        }
    }
}
