using System;
using Client.Services.Server.Coordinator;
using Client.Services.Server.Coordinator.Authentication;
using Client.Services.Server.Video;
using Client.Services.Server.Video.Peer;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models;

namespace Client.Services.Server.Factories.VideoSessionFactory;

public class VideoSessionFactory : IVideoSessionFactory
{
    private readonly IServiceProvider _provider;
    private CoordinatorSession? _coordinatorSession;

    public VideoSessionFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public void Initialize(CoordinatorSession coordinatorSession)
    {
        _coordinatorSession = coordinatorSession;
    }

    public IVideoSession CreateVideoSession(VideoSessionDTO videoSessionDTO, string? peerId) =>
        ActivatorUtilities.CreateInstance<VideoSession>(_provider, videoSessionDTO, _coordinatorSession!, CreatePeer(peerId));

    public IPeer CreatePeer(string? peerId) =>
        ActivatorUtilities.CreateInstance<Peer>(_provider, peerId!);
}