using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaDemo.Converter
{
    public class ZeroToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count == 0 ? false : true;  // Return false when count is 0 (hidden), true otherwise (visible)

            return true;  // Default to visible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


}
