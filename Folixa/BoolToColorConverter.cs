using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Folixa
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSentByCurrentUser)
            {
                return isSentByCurrentUser ? Color.FromArgb("#007AFF") : Color.FromArgb("#E5E5EA");
            }
            return Color.FromArgb("#E5E5EA");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
