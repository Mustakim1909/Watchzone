using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Converters
{
    public class CurrentCustomerReviewConverter : IValueConverter
    {
        public string CurrentCustomerName { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;

            string reviewCustomerName = value.ToString();
            return reviewCustomerName.Equals(CurrentCustomerName, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
