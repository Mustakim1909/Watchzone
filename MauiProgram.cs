using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using Watchzone.Models;
using Watchzone.Services;
using Watchzone.ViewModels;
using Watchzone.Views;

namespace Watchzone
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("Watchzone.appsettings.json");
            var config = new ConfigurationBuilder()
           .AddJsonStream(stream)
           .Build();

            builder.Configuration.AddConfiguration(config);
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<WoocommerceServices>();
            builder.Services.AddTransient<LoginPage>();
            //builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<CategoryPage>();
            builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
           //builder.Services.AddSingleton<WoocommerceServices>();
            builder.Services.AddTransient<CategoryProductsViewModel>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<HomePage>();




            return builder.Build();
        }
    }
}
