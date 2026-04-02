using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShowcaseWebsite.Models;
using ShowcaseWebsite.Services;

namespace ShowcaseWebsite.Controllers;

/// <summary>
/// Admin-dashboard. Alleen toegankelijk voor gebruikers met de Admin-rol.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMessageService _messages;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(IMessageService messages, UserManager<ApplicationUser> userManager)
    {
        _messages = messages;
        _userManager = userManager;
    }

    // GET: /Admin – overzicht van alle berichten
    public async Task<IActionResult> Index()
    {
        var messages = await _messages.GetRecentAsync(200);
        return View(messages);
    }

    // GET: /Admin/FilterByUser?userName=... – berichten gefilterd op gebruiker
    public async Task<IActionResult> FilterByUser(string? userName)
    {
        var users = _userManager.Users.ToList();
        ViewBag.Users = users;
        ViewBag.SelectedUser = userName;

        var messages = string.IsNullOrWhiteSpace(userName)
            ? await _messages.GetRecentAsync(200)
            : await _messages.GetByUserAsync(userName);

        return View(messages);
    }
}
