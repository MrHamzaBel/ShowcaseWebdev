using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Showcase_Chat.Models;
using Showcase_Chat.Services;

namespace Showcase_Chat.Controllers
{
    // Alle acties vereisen inloggen
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IMessageService messageService, UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        // GET: /Chat
        public async Task<IActionResult> Index()
        {
            // Laad de laatste berichten voor de chatweergave
            var messages = await _messageService.GetAllMessagesAsync();
            // Nieuwste berichten onderaan tonen
            return View(messages.Reverse());
        }

        // POST: /Chat/Send — fallback als SignalR niet beschikbaar is
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(string content)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _messageService.SendMessageAsync(userId, content);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
