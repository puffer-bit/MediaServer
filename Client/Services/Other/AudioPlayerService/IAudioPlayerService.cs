using System;
using System.Threading.Tasks;

namespace Client.Services.Other.AudioPlayerService;

public interface IAudioPlayerService : IDisposable
{
    Task StartAsync();
    void PlayOpus(byte[] opusData);
    void PlayPcm(byte[] pcmData);
    void Stop();
}