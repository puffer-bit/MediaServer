namespace Server.Domain.Entities
{
    public class VideoSessionEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public int ConnectedUsersCount { get; set; }
        public int Capacity { get; set; }
        public string? HostId { get; set; }
        public string? HostPeerId { get; set; }
    }
}
