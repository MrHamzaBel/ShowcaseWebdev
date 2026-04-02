using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Showcase_Chat.Models;
using Showcase_Chat.Services;

namespace Showcase_Chat.Controllers
{
    // Alleen admins mogen deze controller benaderen
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IMessageService messageService, UserManager<ApplicationUser> userManager)
        {
            _messageService = messageService;
            _userManager = userManager;
        }

        // GET: /Admin — alle berichten + gebruikerslijst voor filteroptie
        public async Task<IActionResult> Index()
        {
            var messages = await _messageService.GetAllMessagesAsync();
            var users = _userManager.Users.ToList();

            ViewBag.Users = users;
            return View(messages.Reverse());
        }

        // GET: /Admin/FilterByUser/{userId}
        public async Task<IActionResult> FilterByUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index");

            var messages = await _messageService.GetMessagesByUserAsync(userId);
            var targetUser = await _userManager.FindByIdAsync(userId);
            var allUsers = _userManager.Users.ToList();

            ViewBag.Users = allUsers;
            ViewBag.FilteredUser = targetUser?.DisplayName ?? "Onbekend";
            ViewBag.FilteredUserId = userId;

            return View(messages.Reverse());
        }
    }
}
