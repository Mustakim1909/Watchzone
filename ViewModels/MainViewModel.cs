    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Watchzone.Helper;
using Watchzone.Interfaces;
using Watchzone.Models;
    using Watchzone.Services;

    namespace Watchzone.ViewModels
    {
        public partial class MainViewModel : ObservableObject
        {
            [ObservableProperty]
            private string welcomeMessage;

            [ObservableProperty]
            private bool isRefreshing;

            [ObservableProperty]
            private ObservableCollection<Product> products;

            [ObservableProperty]
            private ObservableCollection<Category> categories;

            [ObservableProperty]
            private bool isLoading;

            [ObservableProperty]
            private bool hasError;

            [ObservableProperty]
            private string errorMessage;

            private User currentUser;
            private readonly IWoocommerceServices _wooCommerceService;

            public MainViewModel(IWoocommerceServices wooCommerceService)
            {
                Products = new ObservableCollection<Product>();
                 _wooCommerceService = wooCommerceService;
                 Categories = new ObservableCollection<Category>();

                LoadUserData();
                LoadProductsCommand.ExecuteAsync(null);
            }

            private void LoadUserData()
            {
                // In a real app, you would get this from your authentication system
                currentUser = new User
                {
                    Username = "JohnDoe",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com"
                };

                WelcomeMessage = $"Hello, {currentUser.Username}";
            }

            [RelayCommand]
            public async Task LoadProducts()
            {
                if (IsLoading) return;

                IsLoading = true;
                IsRefreshing = true;
                HasError = false;
                ErrorMessage = string.Empty;

                try
                {
                    Products.Clear();

                    // Get products from WooCommerce API
                    var productsList = await _wooCommerceService.GetLatestProductsAsync(); // Get 20 latest products

                    if (productsList != null && productsList.Any())
                    {
                        foreach (var product in productsList.Skip(1))
                        {
                            Products.Add(product);
                        }
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
                    ErrorMessage = $"Error loading products: {ex.Message}";
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                    IsRefreshing = false;
                }
            }
        public async Task LoadCategories(bool loadAll = false)
        {
            try
            {
                Categories.Clear();
                var categories = await _wooCommerceService.GetCategoriesAsync();

                var list = loadAll ? categories : categories.Take(3);

                foreach (var cat in list)
                {
                    if (cat.ImageUrl?.Src != null)
                    {
                        string url = cat.ImageUrl.Src;

                        if (url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                        {
                            // WebP ko convert karo
                            cat.ImageUrl.LocalImage = await ImageHelper.LoadImageAsync(url);
                            Debug.WriteLine($"Category: {cat.Name}, Image: {cat.ImageUrl.LocalImage}");
                        }
                        else
                        {
                            cat.ImageUrl.LocalImage = ImageSource.FromUri(new Uri(url));
                            Console.WriteLine($"Category: {cat.Name}, Image: {cat.ImageUrl.LocalImage}");
                        }
                    }

                    Categories.Add(cat);
                }

                // Sirf Home ke liye "More" add karo
                if (!loadAll)
                {
                    Categories.Add(new Category
                    {
                        Name = "More",
                        ImageUrl = new ImageUrl
                        {
                            LocalImage = ImageSource.FromFile("more.png")
                        },
                        IsMore = true
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading categories: " + ex.Message);
            }
        }

    }

}
