using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Controls.Primitives;
namespace AvaloniaDemo.Converter
{
    public class MenuScrollingVisibilityConverter : IMultiValueConverter
    {
        public static readonly MenuScrollingVisibilityConverter Instance = new();

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 4)
                return false;

            if (values[0] is ScrollBarVisibility vis &&
                values[1] is double offset &&
                values[2] is double extent &&
                values[3] is double viewport)
            {
                if (extent <= viewport)
                    return false;

                var threshold = 0.0;
                if (parameter != null && double.TryParse(parameter.ToString(), out var parsed))
                    threshold = parsed;

                return offset > threshold;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}