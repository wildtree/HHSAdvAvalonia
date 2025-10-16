using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
namespace HHSAdvAvalonia.Converters
{
    public class IntToBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue && int.TryParse(parameter?.ToString(), out var param))
                return intValue == param;
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b && int.TryParse(parameter?.ToString(), out var param))
                return param;
            return BindingOperations.DoNothing;
        }
    }
}