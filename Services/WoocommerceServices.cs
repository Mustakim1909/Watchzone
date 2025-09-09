using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Watchzone.Interfaces;
using Watchzone.Models;
using static Watchzone.Models.CartItem;

namespace Watchzone.Services
{
    public class WoocommerceServices : IWoocommerceServices
    {
        private readonly AppSettings _appSettings;
        //private  readonly string WooCommerceBaseUrl = "https://thewatchzone.in/wp-json/wc/v3/";
        //private  readonly string ConsumerKey = "ck_a459b3278e2f97fb4b71c16fed4383684a099872";
        //private  readonly string ConsumerSecret = "cs_a06975fea66f45e79b166e15a098dde06373aa53";
        //private  readonly string JwtAuthUrl = "https://thewatchzone.in/wp-json/jwt-auth/v1/token";
        private readonly HttpClient _httpClient = new HttpClient();
        private static Dictionary<int, Dictionary<int, CartItem>> _sessionCarts = new Dictionary<int, Dictionary<int, CartItem>>();
        private readonly Dictionary<int, string> _appliedCoupons = new Dictionary<int, string>();
        public WoocommerceServices(IOptions<AppSettings> appsettings) 
        { 
            _appSettings =appsettings.Value;
            var authData = Encoding.ASCII.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authData));

            // Set timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        public async Task<bool> RegisterCustomer(object customerData)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // WooCommerce API authentication
                    var authBytes = Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(authBytes));

                    // Serialize customer data
                    var json = JsonConvert.SerializeObject(customerData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Send POST request to create customer
                    var response = await client.PostAsync($"{_appSettings.BaseUrl}wp-json/wc/v3/customers", content);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"WooCommerce API Error: {ex.Message}");
                return false;
            }
        }
        public  async Task<Customer> AuthenticateUser(string username, string password)
        {
            try
            {
                // WooCommerce doesn't have direct login API, so we need to:
                // 1. Get all customers and find by username/email
                // 2. Or use WordPress REST API for authentication

                // Method 1: Get customer by email/username
                var response = await _httpClient.GetAsync($"{_appSettings.BaseUrl}/wp-json/wc/v3/customers?search={Uri.EscapeDataString(username)}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var customers = JsonConvert.DeserializeObject<List<Customer>>(content);

                    var customer = customers?.FirstOrDefault(c =>
                        c.Username.Equals(username, StringComparison.OrdinalIgnoreCase) ||
                        c.Email.Equals(username, StringComparison.OrdinalIgnoreCase));

                    if (customer != null)
                    {
                        // Note: WooCommerce REST API doesn't validate passwords directly
                        // In a real app, you might need to use WordPress REST API for authentication
                        return customer;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error authenticating user: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ValidateWithJwtAuth(string username, string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var authData = new
                    {
                        username = username,
                        password = password
                    };

                    var json = JsonConvert.SerializeObject(authData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{_appSettings.BaseUrl}wp-json/jwt-auth/v1/token", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var tokenData = JsonConvert.DeserializeObject<JwtTokenResponse>(responseContent);

                        // Store the token for future requests
                        if (!string.IsNullOrEmpty(tokenData?.Token))
                        {
                            Preferences.Set("AuthToken", tokenData.Token);
                            Preferences.Set("UserEmail", tokenData.UserEmail);
                            Preferences.Set("DisplayName", tokenData.UserDisplayName);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JWT Auth Error: {ex.Message}");
            }

            return false;
        }

        private async Task<List<Customer>> GetCustomersByEmail(string email)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // WooCommerce API authentication
                    var authBytes = Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey} : {_appSettings.ConsumerSecret}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(authBytes));

                    // URL encode the email
                    var encodedEmail = HttpUtility.UrlEncode(email);
                    var response = await client.GetAsync($"{_appSettings.BaseUrl}wp-json/wc/v3/customers?email={encodedEmail}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<Customer>>(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCustomers Error: {ex.Message}");
            }

            return null;
        }

        private async Task<Customer> GetCustomerByUsername(string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // WooCommerce API authentication
                    var authBytes = Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(authBytes));

                    // Search customers - note: WooCommerce API doesn't directly support username search
                    // This is a workaround by searching all customers and filtering
                    var response = await client.GetAsync($"{_appSettings.BaseUrl}wp-json/wc/v3/customers");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var customers = JsonConvert.DeserializeObject<List<Customer>>(content);

                        return customers?.FirstOrDefault(c =>
                            c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCustomerByUsername Error: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> ResetPassword(string email)
        {
            // This would typically use a WordPress plugin or custom endpoint
            // For now, we'll just simulate success
            await Task.Delay(1000);
            return true;
        }

        private  bool IsEmail(string input)
        {
            try
            {
                return Regex.IsMatch(input,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public async Task<List<Product>> GetLatestProductsAsync(int perPage = 20)
        {
            try
            {
                var apiUrl = $"{_appSettings.BaseUrl}wp-json/wc/v3/products?orderby=date&order=desc&per_page={perPage}";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}"));
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Product>>(content);
                }
                return new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching products: {ex.Message}");
                return new List<Product>();
            }
        }


        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}wp-json/wc/v3/products/categories?per_page=100";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}"));
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Category>>(content);
                }
                return new List<Category>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return new List<Category>();
            }
        }

        public async Task<List<Order>> GetOrdersAsync(int customerId)
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}wp-json/wc/v3/customers" +
                         $"consumer_key={_appSettings.ConsumerKey}&" +
                         $"consumer_secret={_appSettings.ConsumerSecret}&" +
                         $"customer={customerId}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Order>>(content);
                }
                return new List<Order>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
                return new List<Order>();
            }
        }

        public async Task<Customer> GetCustomerAsync(int customerId)
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}wp-json/wc/v3/customers/{customerId}";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}"));
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Customer>(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customer: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int perPage = 100)
        {
            var all = new List<Product>();
            int page = 1;

            while (true)
            {
                var url = $"{_appSettings.BaseUrl}wp-json/wc/v3/products?category={categoryId}&per_page={perPage}&page={page}";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}"));
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    // optionally log or throw; we'll break and return what we have
                    break;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<List<Product>>(json);

                if (list == null || list.Count == 0) break;

                all.AddRange(list);

                // if returned less than 'perPage' then no more pages
                if (list.Count < perPage) break;

                page++;
            }

            return all;
        }

        public async Task<Product> GetProductAsync(int productId)
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}/wp-json/wc/v3/products/{productId}";

                // Add authentication
                var authBytes = System.Text.Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
                var authHeader = Convert.ToBase64String(authBytes);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                var response = await _httpClient.GetStringAsync(url);
                var product = JsonConvert.DeserializeObject<Product>(response);

                //if (product != null && !string.IsNullOrEmpty(product.Description))
                //{
                //    // Remove HTML tags
                //    product.Description = Regex.Replace(product.Description, "<.*?>", string.Empty);
                //}

                return product;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching product: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Review>> GetProductReviewsAsync(int productId)
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}/wp-json/wc/v3/products/reviews?product={productId}";

                // Add authentication
                var authBytes = System.Text.Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
                var authHeader = Convert.ToBase64String(authBytes);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                var response = await _httpClient.GetStringAsync(url);

                var reviews = JsonConvert.DeserializeObject<List<Review>>(response);
                foreach (var review in reviews)
                {
                    review.ReviewText = Regex.Replace(review.ReviewText, "<.*?>", string.Empty).Trim();
                }

                return reviews;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching reviews: {ex.Message}");
                return new List<Review>();
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}/wp-json/wc/v3/products";
                var authBytes = System.Text.Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
                var authHeader = Convert.ToBase64String(authBytes);
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Product>>(content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching products: {ex.Message}");
            }
            return new List<Product>();
        }

        public async Task<List<Product>> GetProductsAsync(int page = 1, int perPage = 20)
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}/wp-json/wc/v3/products?page={page}&per_page={perPage}";
                var authBytes = System.Text.Encoding.UTF8.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}");
                var authHeader = Convert.ToBase64String(authBytes);
               _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Product>>(content);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching products: {ex.Message}");
            }
            return new List<Product>();
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            try
            {
                var url = $"{_appSettings.BaseUrl}wp-json/wc/v3/customers?per_page=100";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_appSettings.ConsumerKey}:{_appSettings.ConsumerSecret}"));
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Customer>>(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customer: {ex.Message}");
                return null;
            }
            return new List<Customer>();
        }

        public async Task<bool> AddToCartAsync(int customerId, int productId, int quantity = 1)
        {
            try
            {
                // Get the product from WooCommerce
                var product = await GetProductAsync(productId);
                if (product == null) return false;

                // Initialize session cart if it doesn't exist
                if (!_sessionCarts.ContainsKey(customerId))
                {
                    _sessionCarts[customerId] = new Dictionary<int, CartItem>();
                }

                // Check if product already exists in cart using dictionary
                if (_sessionCarts[customerId].TryGetValue(productId, out var existingItem))
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    _sessionCarts[customerId][productId] = new CartItem
                    {
                        Product = product,
                        Quantity = quantity
                    };
                }

                // Save to persistent storage
                await SaveCartToStorage(customerId);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to cart: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int customerId, int productId)
        {
            try
            {
                if (!_sessionCarts.ContainsKey(customerId)) return false;

                // Use dictionary removal by key instead of searching
                if (_sessionCarts[customerId].Remove(productId))
                {
                    // Save to persistent storage
                    await SaveCartToStorage(customerId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing from cart: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(int customerId, int productId, int quantity)
        {
            try
            {
                if (!_sessionCarts.ContainsKey(customerId)) return false;

                // Use dictionary lookup by key
                if (_sessionCarts[customerId].TryGetValue(productId, out var itemToUpdate))
                {
                    if (quantity <= 0)
                    {
                        return await RemoveFromCartAsync(customerId, productId);
                    }

                    itemToUpdate.Quantity = quantity;

                    // Save to persistent storage
                    await SaveCartToStorage(customerId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating cart item: {ex.Message}");
                return false;
            }
        }


        public async Task<List<CartItem>> GetCartAsync(int customerId)
        {
            try
            {
                // Try to load from memory first
                if (_sessionCarts.ContainsKey(customerId) && _sessionCarts[customerId].Count > 0)
                {
                    return _sessionCarts[customerId].Values.ToList();
                }

                // If not in memory, try to load from storage
                await LoadCartFromStorage(customerId);

                return _sessionCarts.ContainsKey(customerId) ?
                       _sessionCarts[customerId].Values.ToList() :
                       new List<CartItem>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting cart: {ex.Message}");
                return new List<CartItem>();
            }
        }

        public async Task<bool> ApplyCouponAsync(int customerId, string couponCode)
        {
            try
            {
                // Verify coupon exists and is valid
                var coupon = await GetCouponAsync(couponCode);
                if (coupon == null) return false;

                // Check if coupon is expired
                if (coupon.DateExpires.HasValue && coupon.DateExpires.Value < DateTime.Now)
                {
                    return false;
                }

                // Check minimum amount requirement
                var cart = await GetCartAsync(customerId);
                var subtotal = cart.Sum(item => item.TotalPrice);

                if (!string.IsNullOrEmpty(coupon.MinimumAmount) &&
                    decimal.TryParse(coupon.MinimumAmount, out decimal minAmount) &&
                    subtotal < minAmount)
                {
                    return false;
                }

                // Apply coupon to session
                _appliedCoupons[customerId] = couponCode;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying coupon: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveCouponAsync(int customerId)
        {
            try
            {
                if (_appliedCoupons.ContainsKey(customerId))
                {
                    _appliedCoupons.Remove(customerId);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing coupon: {ex.Message}");
                return false;
            }
        }

        public async Task<Coupon> GetCouponAsync(string couponCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.BaseUrl}/wp-json/wc/v3/coupons?code={Uri.EscapeDataString(couponCode)}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var coupons = JsonConvert.DeserializeObject<List<Coupon>>(content);
                    return coupons?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching coupon: {ex.Message}");
            }
            return null;
        }

        public async Task<Order> CreateOrderAsync(int customerId, Address shippingAddress, Address billingAddress = null)
        {
            try
            {
                var cart = await GetCartAsync(customerId);
                if (!cart.Any()) return null;

                var customer = await GetCustomerAsync(customerId);
                if (customer == null) return null;

                // Use billing address from customer if not provided
                billingAddress ??= customer.Billing;

                var lineItems = cart.Select(item => new
                {
                    product_id = item.Product.Id,
                    quantity = item.Quantity,
                    price = item.Product.Price
                }).ToList();

                // Build order data
                dynamic orderData = new ExpandoObject();
                orderData.customer_id = customerId;
                orderData.status = "pending";
                orderData.line_items = lineItems;

                // Add shipping address
                orderData.shipping = new
                {
                    first_name = shippingAddress.FirstName,
                    last_name = shippingAddress.LastName,
                    address_1 = shippingAddress.Address1,
                    address_2 = shippingAddress.Address2,
                    city = shippingAddress.City,
                    state = shippingAddress.State,
                    postcode = shippingAddress.Postcode,
                    country = shippingAddress.Country
                };

                // Add billing address
                orderData.billing = new
                {
                    first_name = billingAddress.FirstName,
                    last_name = billingAddress.LastName,
                    address_1 = billingAddress.Address1,
                    address_2 = billingAddress.Address2,
                    city = billingAddress.City,
                    state = billingAddress.State,
                    postcode = billingAddress.Postcode,
                    country = billingAddress.Country,
                    email = billingAddress.Email,
                    phone = billingAddress.Phone
                };

                // Apply coupon if any
                if (_appliedCoupons.ContainsKey(customerId))
                {
                    orderData.coupon_lines = new[]
                    {
                    new { code = _appliedCoupons[customerId] }
                };
                }

                var json = JsonConvert.SerializeObject(orderData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_appSettings.BaseUrl}/wp-json/wc/v3/orders", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var order = JsonConvert.DeserializeObject<Order>(responseContent);

                    // Clear cart and coupon after successful order
                    await ClearCartAsync(customerId);
                    if (_appliedCoupons.ContainsKey(customerId))
                    {
                        _appliedCoupons.Remove(customerId);
                    }

                    return order;
                }
                else
                {
                    Debug.WriteLine($"Failed to create order: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating order: {ex.Message}");
                return null;
            }
        }

        public async Task ClearCartAsync(int customerId)
        {
            try
            {
                if (_sessionCarts.ContainsKey(customerId))
                {
                    _sessionCarts[customerId].Clear();
                }

                // Clear from storage
                await SecureStorage.SetAsync($"cart_{customerId}", string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing cart: {ex.Message}");
            }
        }

        public Task<decimal> CalculateShippingAsync(int customerId, string method = "flat_rate")
        {
            throw new NotImplementedException();
        }

        public Task<decimal> CalculateTaxAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public async Task SaveCartToStorage(int customerId)
        {
            try
            {
                if (_sessionCarts.ContainsKey(customerId) && _sessionCarts[customerId].Count > 0)
                {
                    // Convert dictionary values to list for serialization
                    var cartItems = _sessionCarts[customerId].Values.ToList();
                    var cartJson = JsonConvert.SerializeObject(cartItems);
                    await SecureStorage.SetAsync($"cart_{customerId}", cartJson);
                }
                else
                {
                    // Clear the storage if cart is empty
                    SecureStorage.Remove($"cart_{customerId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving cart to storage: {ex.Message}");
            }
        }

        public async Task LoadCartFromStorage(int customerId)
        {
            try
            {
                var cartJson = await SecureStorage.GetAsync($"cart_{customerId}");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

                    if (cartItems != null && cartItems.Any())
                    {
                        // Convert list to dictionary for efficient lookup
                        _sessionCarts[customerId] = cartItems
                            .GroupBy(item => item.Product.Id)
                            .ToDictionary(group => group.Key, group => group.First());
                    }
                    else
                    {
                        _sessionCarts[customerId] = new Dictionary<int, CartItem>();
                    }
                }
                else
                {
                    _sessionCarts[customerId] = new Dictionary<int, CartItem>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cart from storage: {ex.Message}");
                _sessionCarts[customerId] = new Dictionary<int, CartItem>();
            }
        }
    }
}
