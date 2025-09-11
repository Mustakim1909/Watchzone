using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Converters
{
    public class OrderStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Simplified status display
                return status.ToLower() switch
                {
                    "processing" => "Pending",
                    "pending"  => "Approved",
                    "on-hold"  => "Shipped",
                    "completed" => "Delivered",
                    "cancelled" or "refunded" => "Cancelled",
                    _ => "Processing"
                };
            }
            return "Processing";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "processing" => Color.FromArgb("#FF9800"), // Orange
                    "completed" => Color.FromArgb("#4CAF50"), // Green
                    "cancelled" or "refunded" => Color.FromArgb("#F44336"), // Red
                    "pending" => Color.FromArgb("#9E9E9E"), // Grey
                    "on-hold" => Color.FromArgb("#FFD700"), // Yellow (Gold-like)
                    _ => Color.FromArgb("#2196F3") // Blue (default)
                };
            }
            return Color.FromArgb("#2196F3");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
