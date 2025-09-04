using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Watchzone.Models;
using Watchzone.Services;

namespace Watchzone.ViewModels
{
    public partial class CategoryProductsViewModel : ObservableObject
    {
        private readonly WoocommerceServices _service;

        [ObservableProperty]
        private ObservableCollection<Product> products = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string title;

        public int CategoryId { get; }

        // For advanced paging
        private int currentPage = 1;
        private const int PerPage = 20; // smaller page if you want infinite scroll
        private bool hasMore = true;
        private bool isLoadingMore = false;

        public CategoryProductsViewModel(int categoryId, string categoryName)
        {
            _service = new WoocommerceServices(); // or inject existing service
            CategoryId = categoryId;
            Title = categoryName ?? "Products";
        }

        public async Task LoadProductsAsync(bool force = false)
        {
            if (IsLoading) return;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                Products.Clear();
                currentPage = 1;
                hasMore = true;

                // Using perPage=100 (single shot) OR PerPage for paging
                var list = await _service.GetProductsByCategoryAsync(CategoryId, perPage: 100);
                if (list != null && list.Any())
                {
                    foreach (var p in list)
                        Products.Add(p);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "No products found";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Advanced: incremental load (infinite scroll)
        public async Task LoadMoreProductsAsync()
        {
            if (!hasMore || isLoadingMore) return;
            isLoadingMore = true;

            try
            {
                var list = await _service.GetProductsByCategoryAsync(CategoryId, perPage: PerPage);
                // The service above returns *all pages* by default; for real incremental you'd add page param to service.
                // For demo: assume service supports page param or create GetProductsPageAsync(categoryId, page, perPage)
                // If here you get list with page info, add items and update hasMore accordingly.
            }
            catch { }
            finally { isLoadingMore = false; }
        }

        [RelayCommand]
        public async Task Refresh()
        {
            await LoadProductsAsync(force: true);
        }

        [RelayCommand]
        private void AddToCart(Product product)
        {
            if (product == null) return;

            // Yahan apna CartService call karo
            Console.WriteLine($"Added to cart: {product.Name}");

            // Optionally Toast / Alert
            Application.Current.MainPage.DisplayAlert("Cart", $"{product.Name} added to cart!", "OK");
        }
    }
}
