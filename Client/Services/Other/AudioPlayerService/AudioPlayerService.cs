using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;

namespace Client.Services.Other.AudioPlayerService;

public class AudioPlayerService : IAudioPlayerService
{
    private readonly int _sampleRate = 44000;
    private readonly int _channels = 1;
    private readonly Queue<byte[]> _bufferQueue = new();
    private readonly OpusToPcmDecoder _opusDecoder = new();
    private int _stream;

    public async Task StartAsync()
    {
        _stream = Bass.CreateStream(_sampleRate, _channels, BassFlags.Default, MyStreamCallback, IntPtr.Zero);
        Bass.ChannelPlay(_stream);
        Bass.ChannelSetAttribute(_stream, ChannelAttribute.Volume, 0.6);
        await Task.CompletedTask;
    }

    public void PlayOpus(byte[] opusData)
    {
        _bufferQueue.Enqueue(_opusDecoder.Decode(opusData));
    }
    
    public void PlayPcm(byte[] pcmData)
    {
        for (int i = 0; i < pcmData.Length; i += 2)
        {
            (pcmData[i], pcmData[i + 1]) = (pcmData[i + 1], pcmData[i]);
        }
        
        _bufferQueue.Enqueue(pcmData);
    }
    
    public void Stop()
    {
        Bass.ChannelStop(_stream);
        Bass.StreamFree(_stream);
    }

    public void Dispose()
    {
         _opusDecoder.Dispose();
    }
    
    private int MyStreamCallback(int handle, IntPtr buffer, int length, IntPtr user)
    {
        if (!_bufferQueue.TryDequeue(out var data) || data == null || data.Length == 0)
            return 0;
        for (int i = 0; i < data.Length; i += 2)
        {
            short sample = BitConverter.ToInt16(data, i);
            
            float gain = 0.7f;
            int amplified = (int)(sample * gain);
            
            if (amplified > short.MaxValue) amplified = short.MaxValue;
            if (amplified < short.MinValue) amplified = short.MinValue;
            
            data[i] = (byte)(amplified & 0xFF);
            data[i + 1] = (byte)((amplified >> 8) & 0xFF);
        }
        int copyLength = Math.Min(length, data.Length);
        Marshal.Copy(data, 0, buffer, copyLength);
        //Console.WriteLine($"Frame dequeued at {DateTime.Now:HH:mm:ss.fff}, Size {data.Length} bytes");
        return copyLength;
    }
    
    private byte[] GenerateTestToneFrame()
    {
        int sampleRate = 8000;
        int channels = 1;
        int durationMs = 1000;
        int samples = sampleRate * durationMs / 1000;
        short[] frame = new short[samples * channels];

        double freq = 440.0;
        for (int i = 0; i < samples; i++)
        {
            short value = (short)(Math.Sin(2 * Math.PI * freq * i / sampleRate) * short.MaxValue);
            frame[i * 2] = value;     // Left
            frame[i * 2 + 1] = value; // Right
        }

        byte[] bytes = new byte[frame.Length * 2];
        Buffer.BlockCopy(frame, 0, bytes, 0, bytes.Length);
        return bytes;
    }
}
