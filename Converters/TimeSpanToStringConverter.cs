using Microsoft.UI.Xaml.Data;
using System;

namespace HealthAssist.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan ts)
            {
                // Format as HH:mm (e.g., 08:30)
                return ts.ToString(@"hh\:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // Not needed for one-way display
        }
    }
}