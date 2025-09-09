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
    public class CheckoutViewModel : BaseViewModel
    {
        private readonly IWoocommerceServices _wooCommerceService;
        private Customer _currentCustomer;
        private Address _shippingAddress;
        private Address _billingAddress;
        private bool _useSameAddressForBilling = true;
        private string _couponCode;
        private decimal _shippingCost;
        private decimal _taxAmount;
        private decimal _discountAmount;
        private ObservableCollection<CartItem> _cartItems;
        private bool _isBusy;

        public CheckoutViewModel(IWoocommerceServices wooCommerceService)
        {
            _wooCommerceService = wooCommerceService;
            ShippingAddress = new Address();
            BillingAddress = new Address();
            CartItems = new ObservableCollection<CartItem>();

            ApplyCouponCommand = new Command(async () => await ApplyCoupon());
            RemoveCouponCommand = new Command(async () => await RemoveCoupon());
            PlaceOrderCommand = new Command(async () => await PlaceOrder());
        }

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public Customer CurrentCustomer
        {
            get => _currentCustomer;
            set => SetProperty(ref _currentCustomer, value);
        }

        public Address ShippingAddress
        {
            get => _shippingAddress;
            set => SetProperty(ref _shippingAddress, value);
        }

        public Address BillingAddress
        {
            get => _billingAddress;
            set => SetProperty(ref _billingAddress, value);
        }

        public bool UseSameAddressForBilling
        {
            get => _useSameAddressForBilling;
            set
            {
                SetProperty(ref _useSameAddressForBilling, value);
                if (value)
                {
                    BillingAddress = ShippingAddress;
                }
            }
        }

        public string CouponCode
        {
            get => _couponCode;
            set => SetProperty(ref _couponCode, value);
        }

        public decimal Subtotal => CartItems.Sum(x => x.TotalPrice);

        public decimal ShippingCost
        {
            get => _shippingCost;
            set => SetProperty(ref _shippingCost, value);
        }

        public decimal TaxAmount
        {
            get => _taxAmount;
            set => SetProperty(ref _taxAmount, value);
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set => SetProperty(ref _discountAmount, value);
        }

        public decimal Total => Subtotal + ShippingCost + TaxAmount - DiscountAmount;

        public bool IsCouponApplied => !string.IsNullOrEmpty(CouponCode);

        public ICommand ApplyCouponCommand { get; }
        public ICommand RemoveCouponCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public async Task LoadData()
        {
            if (_isBusy) return;
            _isBusy = true;

            try
            {
                int customerId = App.CurrentCustomerId;

                if (customerId > 0)
                {
                    CurrentCustomer = await _wooCommerceService.GetCustomerAsync(customerId);

                    if (CurrentCustomer != null)
                    {
                        if (CurrentCustomer.Shipping != null)
                            ShippingAddress = CurrentCustomer.Shipping;

                        if (CurrentCustomer.Billing != null)
                            BillingAddress = CurrentCustomer.Billing;
                    }

                    var cart = await _wooCommerceService.GetCartAsync(customerId);
                    CartItems = new ObservableCollection<CartItem>(cart);

                    await CalculateTotals();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please login first", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task ApplyCoupon()
        {
            if (_isBusy || CurrentCustomer == null || string.IsNullOrEmpty(CouponCode)) return;

            _isBusy = true;
            try
            {
                var success = await _wooCommerceService.ApplyCouponAsync(CurrentCustomer.Id, CouponCode);
                if (success)
                {
                    await CalculateTotals();
                    await Application.Current.MainPage.DisplayAlert("Success", "Coupon applied successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid or expired coupon code", "OK");
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task RemoveCoupon()
        {
            if (_isBusy || CurrentCustomer == null) return;

            _isBusy = true;
            try
            {
                var success = await _wooCommerceService.RemoveCouponAsync(CurrentCustomer.Id);
                if (success)
                {
                    CouponCode = string.Empty;
                    await CalculateTotals();
                    await Application.Current.MainPage.DisplayAlert("Success", "Coupon removed successfully", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to remove coupon", "OK");
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task PlaceOrder()
        {
            if (_isBusy || CurrentCustomer == null || !CartItems.Any()) return;

            _isBusy = true;
            try
            {
                // Validate shipping
                if (string.IsNullOrEmpty(ShippingAddress.FirstName) ||
                    string.IsNullOrEmpty(ShippingAddress.LastName) ||
                    string.IsNullOrEmpty(ShippingAddress.Address1) ||
                    string.IsNullOrEmpty(ShippingAddress.City) ||
                    string.IsNullOrEmpty(ShippingAddress.Postcode) ||
                    string.IsNullOrEmpty(ShippingAddress.Country))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all required shipping information", "OK");
                    return;
                }

                // Validate billing
                if (!UseSameAddressForBilling &&
                    (string.IsNullOrEmpty(BillingAddress.FirstName) ||
                     string.IsNullOrEmpty(BillingAddress.LastName) ||
                     string.IsNullOrEmpty(BillingAddress.Address1) ||
                     string.IsNullOrEmpty(BillingAddress.City) ||
                     string.IsNullOrEmpty(BillingAddress.Postcode) ||
                     string.IsNullOrEmpty(BillingAddress.Country)))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all required billing information", "OK");
                    return;
                }

                var order = await _wooCommerceService.CreateOrderAsync(
                    CurrentCustomer.Id,
                    ShippingAddress,
                    UseSameAddressForBilling ? null : BillingAddress);

                if (order != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", $"Order #{order.Id} placed successfully", "OK");
                    CartItems.Clear();
                    await CalculateTotals();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to place order", "OK");
                }
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async Task CalculateTotals()
        {
            if (CurrentCustomer == null) return;

           // ShippingCost = await _wooCommerceService.CalculateShippingAsync(CurrentCustomer.Id);
            //TaxAmount = await _wooCommerceService.CalculateTaxAsync(CurrentCustomer.Id);

            if (IsCouponApplied)
            {
                var coupon = await _wooCommerceService.GetCouponAsync(CouponCode);
                if (coupon != null)
                {
                    if (coupon.DiscountType == "percent")
                    {
                        if (decimal.TryParse(coupon.Amount, out decimal percent))
                        {
                            DiscountAmount = Subtotal * (percent / 100);
                        }
                    }
                    else if (coupon.DiscountType == "fixed_cart" || coupon.DiscountType == "fixed_product")
                    {
                        if (decimal.TryParse(coupon.Amount, out decimal fixedAmount))
                        {
                            DiscountAmount = fixedAmount;
                        }
                    }
                }
            }
            else
            {
                DiscountAmount = 0;
            }

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }
    
}
}
