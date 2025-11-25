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
        if (value is not VideoSessionPeerState state)
            return "Undefined";

        return state switch
        {
            VideoSessionPeerState.Approved => "Connecting",
            VideoSessionPeerState.Rejected => "Rejected",
            VideoSessionPeerState.WaitingForApprove => "Waiting",
            VideoSessionPeerState.WaitingForNegotiation => "WebRTC connecting",
            VideoSessionPeerState.Connected => "Connected",
            VideoSessionPeerState.Kicked => "Kicked",
            VideoSessionPeerState.Banned => "Banned",
            _ => "Undefined"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

