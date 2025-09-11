using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Watchzone.Interfaces;
using Watchzone.Models;
using Microsoft.Maui.Storage;
using Watchzone.Views; // For Preferences

namespace Watchzone.ViewModels
{
    public class CheckoutViewModel : BaseViewModel
    {
        private readonly IWoocommerceServices _wooCommerceService;
        private Customer _currentCustomer;
        private Address _shippingAddress;
        private Address _billingAddress;
        private bool _useSameAddressForBilling = true;
        private bool _saveShippingAddress; // **Added for checkbox**
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

            LoadSavedAddress(); // **Load saved address on initialization**
        }

        #region Properties

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

        // **Updated: Checkbox property**
        public bool SaveShippingAddress
        {
            get => _saveShippingAddress;
            set
            {
                SetProperty(ref _saveShippingAddress, value);

                if (value) // Save immediately when checkbox clicked
                    SaveAddressToPreferences();
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

        #endregion

        #region Load / Save Address

        // **Load saved address**
        private void LoadSavedAddress()
        {
            ShippingAddress.FirstName = Preferences.Get("SavedFirstName", string.Empty);
            ShippingAddress.LastName = Preferences.Get("SavedLastName", string.Empty);
            ShippingAddress.Phone = Preferences.Get("SavedPhone", string.Empty);
            ShippingAddress.Address1 = Preferences.Get("SavedAddress1", string.Empty);
            ShippingAddress.Address2 = Preferences.Get("SavedAddress2", string.Empty);
            ShippingAddress.City = Preferences.Get("SavedCity", string.Empty);
            ShippingAddress.State = Preferences.Get("SavedState", string.Empty);
            ShippingAddress.Postcode = Preferences.Get("SavedPostcode", string.Empty);
            ShippingAddress.Country = Preferences.Get("SavedCountry", "India"); // Default India

            // Checkbox state if address exists
            SaveShippingAddress = !string.IsNullOrEmpty(ShippingAddress.FirstName);
        }

        // **Save address immediately**
        public void SaveAddressToPreferences()
        {
            Preferences.Set("SavedFirstName", ShippingAddress.FirstName ?? string.Empty);
            Preferences.Set("SavedLastName", ShippingAddress.LastName ?? string.Empty);
            Preferences.Set("SavedPhone", ShippingAddress.Phone ?? string.Empty);
            Preferences.Set("SavedAddress1", ShippingAddress.Address1 ?? string.Empty);
            Preferences.Set("SavedAddress2", ShippingAddress.Address2 ?? string.Empty);
            Preferences.Set("SavedCity", ShippingAddress.City ?? string.Empty);
            Preferences.Set("SavedState", ShippingAddress.State ?? string.Empty);
            Preferences.Set("SavedPostcode", ShippingAddress.Postcode ?? string.Empty);
            Preferences.Set("SavedCountry", ShippingAddress.Country ?? "India");
        }

        #endregion

        #region Load Data

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
                // ONLY load from server if Preferences are empty
                if (string.IsNullOrEmpty(ShippingAddress.FirstName))
                {
                    if (CurrentCustomer.Shipping != null)
                        ShippingAddress = CurrentCustomer.Shipping;
                }

                if (string.IsNullOrEmpty(BillingAddress.FirstName))
                {
                    if (CurrentCustomer.Billing != null)
                        BillingAddress = CurrentCustomer.Billing;
                }
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


        #endregion

        #region Coupon Methods

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

        #endregion

        #region Place Order

        private async Task PlaceOrder()
        {
            if (_isBusy || CurrentCustomer == null || !CartItems.Any()) return;

            _isBusy = true;

            try
            {
                // Validate shipping
                if (string.IsNullOrEmpty(ShippingAddress.FirstName) ||
                    string.IsNullOrEmpty(ShippingAddress.LastName) ||
                    string.IsNullOrEmpty(ShippingAddress.Phone) ||
                    string.IsNullOrEmpty(ShippingAddress.Address1) ||
                    string.IsNullOrEmpty(ShippingAddress.City) ||
                    string.IsNullOrEmpty(ShippingAddress.Postcode) ||
                    string.IsNullOrEmpty(ShippingAddress.State) ||
                    string.IsNullOrEmpty(ShippingAddress.Country))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Please fill in all required shipping information",
                        "OK");
                    return;
                }

                // Save address if checkbox checked
                if (SaveShippingAddress)
                    SaveAddressToPreferences();

                // Create order
                var order = await _wooCommerceService.CreateOrderAsync(CurrentCustomer.Id, ShippingAddress);

                if (order != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Success",
                        $"Order #{order.Id} placed successfully",
                        "OK");
                    await Shell.Current.Navigation.PushModalAsync(new OrderConfirmedPage(_wooCommerceService,order));

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

        #endregion

        #region Calculate Totals

        private async Task CalculateTotals()
        {
            if (CurrentCustomer == null) return;

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

        #endregion
    }
}
