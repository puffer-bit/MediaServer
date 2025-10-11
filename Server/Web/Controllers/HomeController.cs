using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Server.Web.Models;

namespace Server.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Download()
        {
            return View("/Web/Views/Home/Download.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("/Web/Views/Home/Privacy.cshtml");
        }

        public IActionResult Index()
        {
            return View("/Web/Views/Home/Index.cshtml");
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("/Web/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
