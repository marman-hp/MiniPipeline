using Avalonia.Data.Converters;
using Material.Icons;
using System.Globalization;
using System;
namespace AvaloniaDemo.Converter
{
    public class BusyToIconKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool busy && busy) ? MaterialIconKind.Close : MaterialIconKind.Refresh;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}