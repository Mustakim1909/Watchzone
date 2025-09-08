using Watchzone.Services;
using Watchzone.Views;

namespace Watchzone
{
    public partial class App : Application
    {
        private WoocommerceServices _woocommerceServices;
        public App(WoocommerceServices woocommerceServices)
        {
            InitializeComponent();
            _woocommerceServices = woocommerceServices;

            MainPage = new NavigationPage(new LoginPage(_woocommerceServices));
        }
    }
}
