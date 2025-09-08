using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchzone.Converters
{
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int rating = 0;
            if (value != null)
                rating = System.Convert.ToInt32(value);

            var stars = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                    stars.Add("star_filled.png"); // filled star
            }

            return stars;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
