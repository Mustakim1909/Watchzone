using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Converters
{
    public class DefaultCountryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If no country, return "India" by default
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return "India";

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // When user types, always return the typed text
            return value?.ToString();
        }
    }
}