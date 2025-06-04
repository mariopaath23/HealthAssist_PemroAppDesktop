using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace HealthAssist.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // NumberBox value can be double, so handle both int and double
            double numericValue = 0;
            bool isNumeric = false;

            if (value is int intVal)
            {
                numericValue = intVal;
                isNumeric = true;
            }
            else if (value is double doubleVal)
            {
                numericValue = doubleVal;
                isNumeric = true;
            }

            if (isNumeric)
            {
                if (parameter is string paramString && paramString == "GreaterThanZero")
                {
                    return numericValue > 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                // Default behavior: visible if non-zero
                return numericValue != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}