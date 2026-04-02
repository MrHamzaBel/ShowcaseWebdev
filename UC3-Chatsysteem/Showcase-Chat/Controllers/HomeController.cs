using Microsoft.AspNetCore.Mvc;

namespace Showcase_Chat.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
