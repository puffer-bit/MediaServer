using System;
using Concentus;

namespace Client.Services.Other.AudioPlayerService;

public class OpusToPcmDecoder : IDisposable
{
    private readonly IOpusDecoder _decoder;
    private readonly int _channels;
    private readonly int _frameSize;

    public OpusToPcmDecoder(int sampleRate = 48000, int channels = 2, int frameSizeMs = 20)
    {
        _channels = channels;
        _frameSize = sampleRate / 1000 * frameSizeMs;

        _decoder = OpusCodecFactory.CreateDecoder(sampleRate, channels);
    }

    public byte[] Decode(byte[] opusData)
    {
        Span<byte> inputSpan = opusData.AsSpan();
        Span<short> pcmSpan = new short[_frameSize * _channels];

        int decodedSamples = _decoder.Decode(inputSpan, pcmSpan, pcmSpan.Length, false);
        byte[] pcmBytes = new byte[decodedSamples * _channels * 2];
        Buffer.BlockCopy(pcmSpan.ToArray(), 0, pcmBytes, 0, pcmBytes.Length);

        return pcmBytes;
    }

    public void Dispose()
    {
        _decoder.Dispose();
        GC.SuppressFinalize(this);
    }
}
