using Microsoft.UI.Xaml.Data;
using System;

namespace HealthAssist.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                if (parameter is string formatString && !string.IsNullOrEmpty(formatString))
                {
                    // Handle predefined formats
                    if (formatString.Equals("DateOnly", StringComparison.OrdinalIgnoreCase))
                    {
                        return dateTime.ToString("MMM dd, yyyy"); // e.g., May 23, 2025
                    }
                    else if (formatString.Equals("ShortDate", StringComparison.OrdinalIgnoreCase))
                    {
                        return dateTime.ToString("d"); // Short date pattern, e.g., 5/23/2025
                    }
                    else if (formatString.Equals("TimeOnly", StringComparison.OrdinalIgnoreCase))
                    {
                        return dateTime.ToString("h:mm tt"); // e.g., 3:19 PM
                    }

                    // Try to use the parameter as a custom format string
                    try
                    {
                        return dateTime.ToString(formatString);
                    }
                    catch (FormatException)
                    {
                        // If custom format is invalid, fall back to default
                    }
                }
                // Default format if no parameter or invalid parameter
                return dateTime.ToString("MMM dd, yyyy 'at' h:mm tt");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}