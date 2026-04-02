using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShowcaseWebsite.Services;

namespace ShowcaseWebsite.Controllers;

/// <summary>
/// Chat-pagina is alleen toegankelijk voor ingelogde gebruikers.
/// De Vue-component neemt de UI over; dit is enkel de serverside entry.
/// </summary>
[Authorize]
public class ChatController : Controller
{
    private readonly IMessageService _messages;

    public ChatController(IMessageService messages) => _messages = messages;

    // GET: /Chat
    public async Task<IActionResult> Index()
    {
        // Geef de laatste berichten mee zodat de pagina direct laadt (server-side preload)
        var recent = await _messages.GetRecentAsync(50);
        return View(recent);
    }

    // API-endpoint voor Vue-component om berichten op te halen
    [HttpGet("/api/messages")]
    public async Task<IActionResult> GetMessages()
        => Json(await _messages.GetRecentAsync(50));
}
