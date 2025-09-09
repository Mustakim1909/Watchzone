using System.Diagnostics;
using Watchzone.Interfaces;

namespace Watchzone
{
    public partial class AppShell : Shell
    {
        private readonly IWoocommerceServices _woocommerceServices;
        public AppShell(IWoocommerceServices woocommerceServices)
        {
            InitializeComponent();
            _woocommerceServices = woocommerceServices;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Update cart badge count
            await UpdateCartBadge();
        }

        private async Task UpdateCartBadge()
        {
            try
            {
                if (App.CurrentCustomerId > 0)
                {
                    var cart = await _woocommerceServices.GetCartAsync(App.CurrentCustomerId);
                    var totalItems = cart.Sum(item => item.Quantity);

                    // Update cart tab badge
                    // You'll need to implement this based on your UI framework
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating cart badge: {ex.Message}");
            }
        }
    }
}
