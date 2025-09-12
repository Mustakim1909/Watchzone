using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using Watchzone.Converters;
using Watchzone.Interfaces;
using Watchzone.Models;
using Watchzone.Services;
using Watchzone.ViewModels;
using Watchzone.Views;
using CommunityToolkit.Maui;

namespace Watchzone
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();
#if DEBUG
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("Watchzone.appsettings.json");
            var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
            builder.Configuration.AddConfiguration(config);
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<IWoocommerceServices, WoocommerceServices>();
            builder.Services.AddTransient<LoginPage>();
            //builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<CategoryPage>();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            //builder.Services.AddSingleton<WoocommerceServices>();
            builder.Services.AddTransient<CategoryProductsViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<CartPage>();
            builder.Services.AddTransient<CartViewModel>();
            builder.Services.AddSingleton<InverseBooleanConverter>();
            builder.Services.AddSingleton<ItemCountToHeightConverter>();
            builder.Services.AddTransient<CheckoutPage>();
            builder.Services.AddTransient<CheckoutViewModel>();
            builder.Services.AddTransient<OrdersPage>();
            builder.Services.AddTransient<OrdersViewModel>();
            return builder.Build();
        }
    }
}