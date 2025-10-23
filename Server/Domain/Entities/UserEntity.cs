using Shared.Enums;

namespace Server.Domain.Entities
{
    public class UserEntity
    {
        public required string Id { get; set; }
        public required string UserIdentity { get; set; }
        public required string Username { get; set; }
        public required string Ip { get; set; }
        public required string CoordinatorInstanceId { get; set; }
        public string? RegionName { get; set; }
        public string? RegionCode { get; set; }
        public string? DisplayName { get; set; }
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
