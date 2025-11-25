using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Server.Domain.Entities
{
    public class UserEntity
    {
        [MaxLength(36)]
        public required string Id { get; set; }
        
        [MaxLength(36)]
        public required string UserIdentity { get; set; }
        
        [MaxLength(50)]
        public required string Username { get; set; }
        
        [MaxLength(45)]
        public required string Ip { get; set; }
        
        [MaxLength(64)]
        public required string CoordinatorInstanceId { get; set; }
        
        [MaxLength(100)]
        public string? RegionName { get; set; }
        
        [MaxLength(10)]
        public string? RegionCode { get; set; }
        
        [MaxLength(100)]
        public string? DisplayName { get; set; }
        
        [MaxLength(10)]
        public string? Prefix { get; set; }
        
        [MaxLength(2048)]
        public string? AvatarUrl { get; set; }
        public UserState State { get; set; }
        public DateTime FirstConnectionTime { get; set; }
        public DateTime LastConnectionTime { get; set; }
        public TimeSpan ConnectionTime => DateTime.UtcNow - LastConnectionTime;

        public UserEntity(string username)
        {
            Username = username;
        }

        public void ChangeDisplayName(string newName)
        {
            DisplayName = newName;
        }
    }
}
