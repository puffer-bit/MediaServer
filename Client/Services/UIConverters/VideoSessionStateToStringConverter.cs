using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Client.Services.Server.Video;
using Client.ViewModels.Sessions.VideoSession;
using Shared.Enums;

namespace Client.Services.UIConverters;

public class VideoSessionStateToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not VideoSessionApproveState state)
            return "Undefined";

        return state switch
        {
            VideoSessionApproveState.Approved => "Connecting",
            VideoSessionApproveState.Rejected => "Rejected",
            VideoSessionApproveState.WaitingForApprove => "Waiting",
            VideoSessionApproveState.WaitingForNegotiation => "WebRTC connecting",
            VideoSessionApproveState.Connected => "Connected",
            VideoSessionApproveState.Kicked => "Kicked",
            VideoSessionApproveState.Banned => "Banned",
            _ => "Undefined"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

