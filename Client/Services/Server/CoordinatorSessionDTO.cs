using Shared.Models;

namespace Client.Services.Server;

public class CoordinatorSessionDTO
{
    public string Address { get; set; }
    public UserDTO User { get; set; }
    
    public CoordinatorSessionDTO(string address, UserDTO user)
    {
        Address = address;
        User = user;
    }
}