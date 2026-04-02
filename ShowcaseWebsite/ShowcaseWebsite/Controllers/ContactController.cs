using Microsoft.AspNetCore.Mvc;
using ShowcaseWebsite.Services;
using ShowcaseWebsite.ViewModels;

namespace ShowcaseWebsite.Controllers;

/// <summary>
/// Publiek contactformulier – verstuurt mail via MailService (Mailtrap).
/// </summary>
public class ContactController : Controller
{
    private readonly IMailService _mail;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IMailService mail, ILogger<ContactController> logger)
    {
        _mail = mail;
        _logger = logger;
    }

    // GET: /Contact
    public IActionResult Index() => View(new ContactViewModel());

    // POST: /Contact
    [HttpPost]
    [ValidateAntiForgeryToken]  // CSRF-bescherming (ASVS V4.2.2)
    public async Task<IActionResult> Index(ContactViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            string body = $"Naam: {vm.Name}\nE-mail: {vm.Email}\n\n{vm.Message}";
            await _mail.SendAsync(vm.Name, vm.Email, vm.Subject, body);
            TempData["Success"] = "Uw bericht is verzonden. We nemen zo snel mogelijk contact op.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contactformulier mail versturen mislukt");
            TempData["Error"] = "Het versturen is mislukt. Probeer het later opnieuw.";
        }

        return RedirectToAction(nameof(Index));
    }
}
