using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Showcase_Profielpagina.Controllers
{
    public class ContactController : Controller
    {
        public ActionResult Me()
        {
            return View();
        }
    }
}
