using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Client.UIConverters;
public class ConnectionIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isConnected = value is bool b && b;

        return isConnected
            ? "avares://Client/Assets/Icons/Joined.svg"
            : "avares://Client/Assets/Icons/Login.svg";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

