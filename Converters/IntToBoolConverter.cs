using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Watchzone.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = (parameter as string) == "inverse";
            if (value is int count)
            {
                return inverse ? count == 0 : count > 0;
            }
            return inverse ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
