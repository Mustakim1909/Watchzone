using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchzone.Models;

namespace Watchzone.Interfaces
{
    public interface IWoocommerceServices
    {
        Task<bool> RegisterCustomer(object customerData);
        Task<Customer> AuthenticateUser(string username, string password);
        Task<bool> ValidateWithJwtAuth(string username, string password);
        Task<bool> ResetPassword(string email);
        Task<List<Product>> GetLatestProductsAsync(int perPage = 20);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Order>> GetCustomerOrdersAsync(int customerId);
        Task<Customer> GetCustomerAsync(int customerId);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int perPage = 100);
        Task<Product> GetProductAsync(int productId);
        Task<List<Review>> GetProductReviewsAsync(int productId);
        Task<List<Product>> GetProductsAsync();
        Task<List<Product>> GetProductsAsync(int page = 1, int perPage = 20);
        Task<List<Customer>> GetCustomersAsync();
        Task<bool> AddToCartAsync(int customerId, int productId, int quantity = 1);
        Task<bool> RemoveFromCartAsync(int customerId, int productId);
        Task<bool> UpdateCartItemAsync(int customerId, int productId, int quantity);
        Task<List<CartItem>> GetCartAsync(int customerId);
        Task<bool> ApplyCouponAsync(int customerId, string couponCode);
        Task<bool> RemoveCouponAsync(int customerId);
        Task<Coupon> GetCouponAsync(string couponCode);
        Task<Order> CreateOrderAsync(int customerId, Address shippingAddress);
        Task ClearCartAsync(int customerId);
       // Task<decimal> CalculateShippingAsync(int customerId, string method = "flat_rate");
        //Task<decimal> CalculateTaxAsync(int customerId);
        Task SaveCartToStorage(int customerId);
        Task LoadCartFromStorage(int customerId);
        Task<bool> AddReviewAsync(int productId, int customerId, int rating, string reviewText);
        Task<bool> DeleteReviewAsync(int reviewId);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
    }
}
