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
        Task<bool> AuthenticateUser(string username, string password);
        Task<bool> ValidateWithJwtAuth(string username, string password);
        Task<bool> ResetPassword(string email);
        Task<List<Product>> GetLatestProductsAsync(int perPage = 20);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Order>> GetOrdersAsync(int customerId);
        Task<User> GetCustomerAsync(int customerId);
        Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int perPage = 100);
        Task<Product> GetProductAsync(int productId);
        Task<List<Review>> GetProductReviewsAsync(int productId);
    }
}
