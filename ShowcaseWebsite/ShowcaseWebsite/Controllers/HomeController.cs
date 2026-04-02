using Microsoft.AspNetCore.Mvc;

namespace ShowcaseWebsite.Controllers;

/// <summary>
/// Publieke CV/profielpagina – geen authenticatie vereist.
/// </summary>
public class HomeController : Controller
{
    public IActionResult Index() => View();

    // Foutpagina (gebruikt door ExceptionHandler)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
