using System.Collections.ObjectModel;
using System.Windows.Input;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.Views;

namespace Watchzone.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private readonly IWoocommerceServices _wooCommerceService;
        private Customer _currentCustomer;
        private ObservableCollection<CartItem> _cartItems;
        private Address _shippingAddress;
        private Address _billingAddress;
        private bool _isBusy;
        private string _couponCode;
        private decimal _shippingCost;
        private decimal _taxAmount;
        private decimal _discountAmount;
        private bool _useSameAddressForBilling = true;

        public CartViewModel(IWoocommerceServices wooCommerceService)
        {
            _wooCommerceService = wooCommerceService;
            CartItems = new ObservableCollection<CartItem>();
            ShippingAddress = new Address();
            BillingAddress = new Address();

            LoadCustomerDataCommand = new Command(async () => await LoadCustomerData());
            UpdateCartCommand = new Command(async () => await UpdateCart());
            CheckoutCommand = new Command(async () => await Checkout());
            IncreaseQuantityCommand = new Command<CartItem>(async (item) => await IncreaseQuantity(item));
            DecreaseQuantityCommand = new Command<CartItem>(async (item) => await DecreaseQuantity(item));
            RemoveItemCommand = new Command<CartItem>(async (item) => await RemoveItem(item));
            //ApplyCouponCommand = new Command(async () => await ApplyCoupon());
            //RemoveCouponCommand = new Command(async () => await RemoveCoupon());
            CalculateTotalsCommand = new Command(async () => await CalculateTotals());
        }

        #region Properties

        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set => SetProperty(ref _cartItems, value);
        }

        public decimal Subtotal => CartItems.Sum(item => item.TotalPrice);

        public decimal Total => Subtotal + ShippingCost + TaxAmount - DiscountAmount;

        public int TotalItems => CartItems.Sum(item => item.Quantity);

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

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string CouponCode
        {
            get => _couponCode;
            set => SetProperty(ref _couponCode, value);
        }

        public decimal ShippingCost
        {
            get => _shippingCost;
            set
            {
                SetProperty(ref _shippingCost, value);
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal TaxAmount
        {
            get => _taxAmount;
            set
            {
                SetProperty(ref _taxAmount, value);
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set
            {
                SetProperty(ref _discountAmount, value);
                OnPropertyChanged(nameof(Total));
            }
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

        public bool HasCouponApplied => !string.IsNullOrEmpty(CouponCode);

        #endregion

        #region Commands

        public ICommand LoadCustomerDataCommand { get; }
        public ICommand UpdateCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ApplyCouponCommand { get; }
        public ICommand RemoveCouponCommand { get; }
        public ICommand CalculateTotalsCommand { get; }

        #endregion

        #region Methods

        public async Task LoadCustomerData()
        {
            if (IsBusy) return;
            IsBusy = true;

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

                    // Clear existing items before loading new ones
                    CartItems.Clear();

                    var cart = await _wooCommerceService.GetCartAsync(customerId);

                    // Add items one by one instead of replacing the collection
                    foreach (var item in cart)
                    {
                        CartItems.Add(item);
                    }

                    if (!cart.Any())
                    {
                        //await Application.Current.MainPage.DisplayAlert("Info", "Your cart is empty", "OK");
                    }

                    await CalculateTotals();
                }
                else
                {
                   // await Application.Current.MainPage.DisplayAlert("Error", "Please login first", "OK");
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateCart()
        {
            if (IsBusy || CurrentCustomer == null) return;
            IsBusy = true;

            try
            {
                foreach (var item in CartItems)
                {
                    await _wooCommerceService.UpdateCartItemAsync(CurrentCustomer.Id, item.Product.Id, item.Quantity);
                }

               // await Application.Current.MainPage.DisplayAlert("Success", "Cart updated successfully", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task Checkout()
        {
            await Shell.Current.Navigation.PushModalAsync(new CheckoutPage(_wooCommerceService));
        }

        private async Task IncreaseQuantity(CartItem item)
        {
            if (item != null && CurrentCustomer != null)
            {
                if (item.Product.StockQuantity.HasValue && item.Quantity >= item.Product.StockQuantity.Value)
                {
                    await Application.Current.MainPage.DisplayAlert("Stock Limit", $"Only {item.Product.StockQuantity} items available in stock", "OK");
                    return;
                }

                item.Quantity++;
                await _wooCommerceService.UpdateCartItemAsync(CurrentCustomer.Id, item.Product.Id, item.Quantity);
                await CalculateTotals();
            }
        }

        private async Task DecreaseQuantity(CartItem item)
        {
            if (item != null && item.Quantity > 1 && CurrentCustomer != null)
            {
                item.Quantity--;
                await _wooCommerceService.UpdateCartItemAsync(CurrentCustomer.Id, item.Product.Id, item.Quantity);
                await CalculateTotals();
            }
        }

        private async Task RemoveItem(CartItem item)
        {
            if (item != null && CurrentCustomer != null)
            {
                CartItems.Remove(item);
                await _wooCommerceService.RemoveFromCartAsync(CurrentCustomer.Id, item.Product.Id);
                await CalculateTotals();
            }
        }

        //private async Task ApplyCoupon()
        //{
        //    if (IsBusy || CurrentCustomer == null || string.IsNullOrEmpty(CouponCode)) return;
        //    IsBusy = true;

        //    try
        //    {
        //        var success = await _wooCommerceService.ApplyCouponAsync(CurrentCustomer.Id, CouponCode);
        //        if (success)
        //        {
        //            await CalculateTotals();
        //            await Application.Current.MainPage.DisplayAlert("Success", "Coupon applied successfully", "OK");
        //        }
        //        else
        //        {
        //            await Application.Current.MainPage.DisplayAlert("Error", "Invalid or expired coupon code", "OK");
        //        }
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        //private async Task RemoveCoupon()
        //{
        //    if (IsBusy || CurrentCustomer == null) return;
        //    IsBusy = true;

        //    try
        //    {
        //        var success = await _wooCommerceService.RemoveCouponAsync(CurrentCustomer.Id);
        //        if (success)
        //        {
        //            CouponCode = string.Empty;
        //            await CalculateTotals();
        //            await Application.Current.MainPage.DisplayAlert("Success", "Coupon removed successfully", "OK");
        //        }
        //        else
        //        {
        //            await Application.Current.MainPage.DisplayAlert("Error", "Failed to remove coupon", "OK");
        //        }
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        private async Task CalculateTotals()
        {
            if (CurrentCustomer == null) return;

            //ShippingCost = await _wooCommerceService.CalculateShippingAsync(CurrentCustomer.Id);
            //TaxAmount = await _wooCommerceService.CalculateTaxAsync(CurrentCustomer.Id);

            if (HasCouponApplied)
            {
                var coupon = await _wooCommerceService.GetCouponAsync(CouponCode);
                if (coupon != null)
                {
                    if (coupon.DiscountType == "percent")
                    {
                        if (decimal.TryParse(coupon.Amount, out decimal percent))
                            DiscountAmount = Subtotal * (percent / 100);
                    }
                    else if (coupon.DiscountType == "fixed_cart" || coupon.DiscountType == "fixed_product")
                    {
                        if (decimal.TryParse(coupon.Amount, out decimal fixedAmount))
                            DiscountAmount = fixedAmount;
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
