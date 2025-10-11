using System;
using Avalonia.Media.Imaging;
using Client.Services.Other.ScreenCastService;
using SIPSorceryMedia.Abstractions;

namespace Client.Services.Other.FrameProcessor;

public interface IFrameProcessor : IDisposable
{
    WriteableBitmap ProceedRawFrame(RawImage rawImage);
}