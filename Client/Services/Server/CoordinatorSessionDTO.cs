using Shared.Models;
using Shared.Models.DTO;

namespace Client.Services.Server;

public class CoordinatorSessionDTO
{
    public string Address { get; set; }
    public string Id { get; set; }
    public UserDTO User { get; set; }
    
    public CoordinatorSessionDTO(string id, string address, UserDTO user)
    {
        Id = id;
        Address = address;
        User = user;
    }
}