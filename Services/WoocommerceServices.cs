using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Watchzone.Models;
using static System.Net.WebRequestMethods;

namespace Watchzone.Services
{
    public class WoocommerceServices
    {
        private static readonly string WooCommerceBaseUrl = "https://thewatchzone.in/wp-json/wc/v3/";
        private static readonly string ConsumerKey = "ck_a459b3278e2f97fb4b71c16fed4383684a099872";
        private static readonly string ConsumerSecret = "cs_a06975fea66f45e79b166e15a098dde06373aa53";
        private static readonly string JwtAuthUrl = "https://thewatchzone.in/wp-json/jwt-auth/v1/token";
        private readonly HttpClient _httpClient = new HttpClient();

        public static async Task<bool> RegisterCustomer(object customerData)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // WooCommerce API authentication
                    var authBytes = Encoding.UTF8.GetBytes($"{ConsumerKey}:{ConsumerSecret}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(authBytes));

                    // Serialize customer data
                    var json = JsonConvert.SerializeObject(customerData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Send POST request to create customer
                    var response = await client.PostAsync($"{WooCommerceBaseUrl}customers", content);

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
        public static async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                // Method 1: Using WooCommerce REST API (if you have customer list access)
                // This method checks if the credentials match any customer
                var customers = await GetCustomersByEmail(username);

                if (customers != null && customers.Any())
                {
                    // For security reasons, we can't verify password via WooCommerce API directly
                    // So we might need to use a different approach

                    // Method 2: Using WordPress REST API or JWT Authentication
                    return await ValidateWithJwtAuth(username, password);
                }

                // If username is not an email, try searching by username
                if (!IsEmail(username))
                {
                    var customerByUsername = await GetCustomerByUsername(username);
                    if (customerByUsername != null)
                    {
                        return await ValidateWithJwtAuth(username, password);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication Error: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> ValidateWithJwtAuth(string username, string password)
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

                    var response = await client.PostAsync(JwtAuthUrl, content);

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

        private static async Task<List<Customer>> GetCustomersByEmail(string email)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // WooCommerce API authentication
                    var authBytes = Encoding.UTF8.GetBytes($"{ConsumerKey}:{ConsumerSecret}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(authBytes));

                    // URL encode the email
                    var encodedEmail = HttpUtility.UrlEncode(email);
                    var response = await client.GetAsync($"{WooCommerceBaseUrl}customers?email={encodedEmail}");

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

        private static async Task<Customer> GetCustomerByUsername(string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // WooCommerce API authentication
                    var authBytes = Encoding.UTF8.GetBytes($"{ConsumerKey}:{ConsumerSecret}");
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                            Convert.ToBase64String(authBytes));

                    // Search customers - note: WooCommerce API doesn't directly support username search
                    // This is a workaround by searching all customers and filtering
                    var response = await client.GetAsync($"{WooCommerceBaseUrl}customers");

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

        public static async Task<bool> ResetPassword(string email)
        {
            // This would typically use a WordPress plugin or custom endpoint
            // For now, we'll just simulate success
            await Task.Delay(1000);
            return true;
        }

        private static bool IsEmail(string input)
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
                var apiUrl = $"{WooCommerceBaseUrl}products?orderby=date&order=desc&per_page={perPage}";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ConsumerKey}:{ConsumerSecret}"));
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
                var url = $"{WooCommerceBaseUrl}products/categories?per_page=100";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ConsumerKey}:{ConsumerSecret}"));
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
                var url = $"{WooCommerceBaseUrl}/wp-json/wc/v3/orders?" +
                         $"consumer_key={ConsumerKey}&" +
                         $"consumer_secret={ConsumerSecret}&" +
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

        public async Task<User> GetCustomerAsync(int customerId)
        {
            try
            {
                var url = $"{WooCommerceBaseUrl}/wp-json/wc/v3/customers/{customerId}?" +
                         $"consumer_key={ConsumerKey}&" +
                         $"consumer_secret={ConsumerSecret}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<User>(content);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching customer: {ex.Message}");
                return null;
            }
        }

        // Services/WoocommerceServices.cs (add this method to your existing service)
        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int perPage = 100)
        {
            var all = new List<Product>();
            int page = 1;

            while (true)
            {
                var url = $"{WooCommerceBaseUrl}products?category={categoryId}&per_page={perPage}&page={page}";
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{ConsumerKey}:{ConsumerSecret}"));
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


    }
}
