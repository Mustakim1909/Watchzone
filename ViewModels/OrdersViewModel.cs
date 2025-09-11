using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Watchzone.Interfaces;
using Watchzone.Models;

namespace Watchzone.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly IWoocommerceServices _woocommerceServices;
        public ObservableCollection<Order> Orders { get; set; }

        public ICommand LoadOrdersCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand BuyAgainCommand { get; }
        public ICommand ContinueShoppingCommand { get; }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public OrdersViewModel()
        {
            
        }
        public OrdersViewModel(IWoocommerceServices woocommerceServices)
        {
            _woocommerceServices = woocommerceServices;
            Orders = new ObservableCollection<Order>();

            // Initialize commands
            LoadOrdersCommand = new Command(async () => await LoadOrders());
            ViewDetailsCommand = new Command<Order>(ViewOrderDetails);
            BuyAgainCommand = new Command<Order>(BuyAgain);
            ContinueShoppingCommand = new Command(ContinueShopping);

            // Load initial data
            LoadOrders();
        }

        public async Task LoadOrders()
        {
            IsRefreshing = true;

            try
            {
                Orders.Clear();

                // Get orders from WooCommerce API
                var orders = await _woocommerceServices.GetCustomerOrdersAsync(App.CurrentCustomerId);

                foreach (var order in orders.OrderByDescending(o => o.DateCreated))
                {
                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async void ViewOrderDetails(Order order)
        {
            // Navigate to order details page
            await Shell.Current.GoToAsync($"orderdetails?id={order.Id}");
        }

        private async void BuyAgain(Order order)
        {
            // Add all items from order to cart
            foreach (var item in order.LineItems)
            {
                // Implement your add to cart logic here
            }

            await Application.Current.MainPage.DisplayAlert("Success", "Items added to cart", "OK");
            await Shell.Current.GoToAsync("//cart");
        }

        private async void ContinueShopping()
        {
            await Shell.Current.GoToAsync("//products");
        }
    }
}