using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using ShowcaseWebsite.Models;
using ShowcaseWebsite.ViewModels;

namespace ShowcaseWebsite.Controllers;

/// <summary>
/// Behandelt Register, Login, Logout en 2FA (TOTP via Google Authenticator).
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            DisplayName = vm.DisplayName,
            EmailConfirmed = true // Geen e-mailverificatie in deze fase
        };

        var result = await _userManager.CreateAsync(user, vm.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(vm);
        }

        // Nieuw account krijgt standaard User-rol
        await _userManager.AddToRoleAsync(user, "User");
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _signInManager.PasswordSignInAsync(
            vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        // 2FA vereist
        if (result.RequiresTwoFactor)
            return RedirectToAction(nameof(Verify2FA), new { returnUrl });

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account tijdelijk geblokkeerd na te veel mislukte pogingen.");
            return View(vm);
        }

        ModelState.AddModelError(string.Empty, "Ongeldige inloggegevens.");
        return View(vm);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ── 2FA inschakelen ───────────────────────────────────────────────────────

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Enable2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Reset en genereer nieuwe sleutel
        await _userManager.ResetAuthenticatorKeyAsync(user);
        string? key = await _userManager.GetAuthenticatorKeyAsync(user);
        string uri = _userManager.GenerateAuthenticatorUri(user.Email!, key!);

        // Genereer QR-code als base64 PNG
        using var qrGen = new QRCodeGenerator();
        var qrData = qrGen.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        byte[] qrBytes = qrCode.GetGraphic(5);
        ViewBag.QrCodeBase64 = Convert.ToBase64String(qrBytes);

        return View(new Enable2FAViewModel { SharedKey = key!, AuthenticatorUri = uri });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable2FA(Enable2FAViewModel vm, string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Verifieer de door de gebruiker ingevoerde TOTP-code
        bool valid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code.Replace(" ", ""));

        if (!valid)
        {
            ModelState.AddModelError(string.Empty, "Ongeldige verificatiecode.");
            return View(vm);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        TempData["Success"] = "Twee-factor authenticatie is ingeschakeld.";
        return RedirectToAction("Index", "Home");
    }

    // ── 2FA verificatie bij login ─────────────────────────────────────────────

    [HttpGet]
    public IActionResult Verify2FA(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify2FA(Verify2FAViewModel vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            vm.Code.Replace(" ", ""), isPersistent: false, rememberClient: false);

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/");

        ModelState.AddModelError(string.Empty, "Ongeldige verificatiecode.");
        return View(vm);
    }
}

// Hulpextensie voor het genereren van de authenticator-URI
public static class UserManagerExtensions
{
    public static string GenerateAuthenticatorUri(
        this UserManager<ApplicationUser> manager,
        string email, string key)
    {
        // RFC 6238 otpauth URI formaat
        return $"otpauth://totp/ShowcaseWebsite:{Uri.EscapeDataString(email)}" +
               $"?secret={key}&issuer=ShowcaseWebsite&digits=6";
    }
}
