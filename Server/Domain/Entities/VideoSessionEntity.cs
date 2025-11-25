using System.ComponentModel.DataAnnotations;

namespace Server.Domain.Entities
{
    public class VideoSessionEntity
    {
        [MaxLength(36)]
        public required string Id { get; set; }
        
        [MaxLength(50)]
        public required string Name { get; set; }
        
        [MaxLength(36)]
        public required string CoordinatorInstanceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public int ConnectedUsersCount { get; set; }
        public int Capacity { get; set; }
        
        [MaxLength(36)]
        public string? HostId { get; set; }
        
        [MaxLength(36)]
        public string? HostPeerId { get; set; }
    }
}
