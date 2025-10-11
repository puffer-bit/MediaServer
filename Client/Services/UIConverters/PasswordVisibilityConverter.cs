using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Client.UIConverters;

public class PasswordVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isVisible = value is bool b && b;
        return isVisible ? "avares://Client/Assets/Icons/HidePassword.svg" : "avares://Client/Assets/Icons/ShowPassword.svg";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
