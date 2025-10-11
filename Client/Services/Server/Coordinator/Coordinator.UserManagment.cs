using System;
using Shared.Models;

namespace Client.Services.Server.Coordinator;

public partial class CoordinatorSession
{
    public UserDTO GetUser()
    {
        return _userManager.GetUser();
    }

    public void SetUser(UserDTO user)
    {
        _userManager.SetUser(user);
    }
}