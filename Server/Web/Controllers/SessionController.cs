using Shared;
using Shared.Enums;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Server.MainServer.Main.Server.Coordinator;

namespace Server.Web.Controllers
{
    public class SessionController : Controller
    {
        private readonly ILogger<SessionController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CoordinatorInstance _coordinatorInstance;
        
        public SessionController(ILogger<SessionController> logger, UserManager<IdentityUser> userManager, CoordinatorInstance coordinatorInstance)
        {
            _logger = logger;
            _userManager = userManager;
            _coordinatorInstance = coordinatorInstance;
        }
        
        [Authorize]
        public async Task<IActionResult> ActiveSession(string id)
        {
            var result = _coordinatorInstance.GetVideoSessionAsModel(id, out var curentSession);
            var user = await _userManager.GetUserAsync(User);
            if (result == SessionRequestResult.NoError)
            {
                if (curentSession.HostId == user!.Id)
                {
                    ViewBag.User = await _userManager.GetUserAsync(User);
                    return View("/Web/Views/Session/ActiveSession.cshtml", curentSession);
                }
                else
                {
                    ViewBag.User = await _userManager.GetUserAsync(User);
                    return View("/Web/Views/Session/ActiveSession.cshtml", curentSession);
                }
            }
            else
            {
                TempData["Error"] = ErrorProvider.GetErrorDescription(result);
                return View("/Web/Views/Session/ActiveSession.cshtml");
            }
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> StartNewSession()
        {
            var user = await _userManager.GetUserAsync(User);

            return View("/Web/Views/Session/StartNewSession.cshtml");
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> StartNewSession(VideoSessionDTO newSession)
        {
            var user = await _userManager.GetUserAsync(User);
            newSession.HostId = user.Id;
            CreateSessionResult result = _coordinatorInstance.CreateSession(newSession, user.Id);
            TempData["Error"] = result;
            return View("/Web/Views/Session/StartNewSession.cshtml", newSession);
        }
        
        [HttpGet]
        [Authorize]
        public IActionResult SessionsList()
        {
            return View("/Web/Views/Session/SessionsList.cshtml",_coordinatorInstance.GetAllSessions());
        }
        
        [HttpPost]
        [Authorize]
        public IActionResult SessionsList(SessionDTO session)
        {
            return RedirectToAction("ActiveSession", "Session", new { id = session.Id });
        }
    }
}
