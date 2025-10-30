using Shared.Enums;
using Shared.Models;

namespace Server.MainServer.Main.Server.Video.Peer;

public interface IPeer
{
    public bool IsStreamHost { get; set; }
    public bool IsAudioRequested { get; set; }
    public VideoSessionApproveState ApproveState { get; set; }
    
    string GetId();
    string GetUserId();
    void MakeHost();
}