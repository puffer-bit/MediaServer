using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SIPSorceryMedia.Abstractions;

namespace Client.Services.Other.FrameProcessor;

public class FrameProcessor : IFrameProcessor, IDisposable
{
    private byte[]? _rgb24;
    private byte[]? _bgra;
    private WriteableBitmap? _bmp;
    
    public WriteableBitmap ProceedRawFrame(RawImage rawImage)
    {
        int rgb24Length = rawImage.Stride * rawImage.Height;
        int bgraLength = rawImage.Width * rawImage.Height * 4;

        if (_rgb24 == null || _rgb24.Length != rgb24Length)
        {
            _rgb24 = new byte[rgb24Length];
        }

        if (_bgra == null || _bgra.Length != bgraLength)
        {
            _bgra = new byte[bgraLength];
        }

        Marshal.Copy(rawImage.Sample, _rgb24, 0, rgb24Length);

        unsafe
        {
            byte* src = (byte*)rawImage.Sample;
            fixed (byte* dst = _bgra)
            {
                for (int i = 0, j = 0; i < rgb24Length; i += 3, j += 4)
                {
                    dst[j] = src[i + 2];     // B
                    dst[j + 1] = src[i + 1]; // G
                    dst[j + 2] = src[i];     // R
                    dst[j + 3] = 255;        // A
                }
            }
        }

        _bmp = new WriteableBitmap(
            new PixelSize(rawImage.Width, rawImage.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque);

        using (var fb = _bmp.Lock())
        {
            Marshal.Copy(_bgra, 0, fb.Address, _bgra.Length);
        }
        return _bmp;
    }
    
    public void Dispose()
    {
        _bmp?.Dispose();
        GC.SuppressFinalize(this);
    }
}