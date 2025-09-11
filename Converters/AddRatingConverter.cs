using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Converters
{
    public class AddRatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedRating = (value as int?) ?? 0;

            if (parameter == null)
                return "star_outline.png"; // fallback if parameter is missing

            if (!int.TryParse(parameter.ToString(), out int starPosition))
                return "star_outline.png"; // fallback if parameter is invalid

            return selectedRating >= starPosition ? "star_filled.png" : "star_outline.png";
        }
         public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
         {
            throw new NotImplementedException();
         }
    }
}
