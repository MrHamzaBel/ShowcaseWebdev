using Microsoft.AspNetCore.Mvc;

namespace Showcase_Profielpagina.Controllers
{
    public class TeamController : Controller
    {
        public IActionResult Topteam()
        {
            return View();
        }
    }
}
