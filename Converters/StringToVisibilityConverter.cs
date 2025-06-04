using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace HealthAssist.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        // If 'parameter' is "Negate", it inverts the logic (Visible if empty/null)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var s = value as string;
            bool negate = parameter is string p && p.Equals("Negate", StringComparison.OrdinalIgnoreCase);
            bool isNullOrEmpty = string.IsNullOrEmpty(s);

            if (negate)
            {
                return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException(); // Not needed for one-way binding
        }
    }
}