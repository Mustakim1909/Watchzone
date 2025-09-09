using System.Diagnostics;
using Watchzone.Interfaces;
using Watchzone.Services;
using Watchzone.Views;

namespace Watchzone
{
    public partial class App : Application
    {
        private IWoocommerceServices _woocommerceServices;
        public static int CurrentCustomerId { get; set; }
        public static string CurrentCustomerName { get; set; }
        public static string CurrentCustomerEmail { get; set; }
        public App(IWoocommerceServices woocommerceServices)
        {
            InitializeComponent();
            _woocommerceServices = woocommerceServices;

            var rememberMe = Preferences.Get("RememberMe", false);
            if (rememberMe)
            {
                var savedCustomerId = Preferences.Get("CustomerId", 0);
                if (savedCustomerId > 0)
                {
                    CurrentCustomerId = savedCustomerId;
                    CurrentCustomerName = Preferences.Get("CustomerName", string.Empty);
                    CurrentCustomerEmail = Preferences.Get("CustomerEmail", string.Empty);
                    _ = PreloadCartAsync(savedCustomerId);
                    MainPage = new AppShell(_woocommerceServices);
                }
                else
                {
                    MainPage = new LoginPage(_woocommerceServices);
                }
            }
            else
            {
                MainPage = new LoginPage(_woocommerceServices);
            }
        }
        private async Task PreloadCartAsync(int customerId)
        {
            try
            {
                await _woocommerceServices.GetCartAsync(customerId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error preloading cart: {ex.Message}");
            }
        }
    }
}
