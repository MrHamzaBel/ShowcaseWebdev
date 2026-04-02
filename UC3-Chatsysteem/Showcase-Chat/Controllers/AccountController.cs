using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Showcase_Chat.Models;
using Showcase_Chat.ViewModels;
using System.Text;
using System.Text.Encodings.Web;

namespace Showcase_Chat.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly UrlEncoder _urlEncoder;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                DisplayName = model.DisplayName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Nieuwe gebruiker krijgt standaard de rol 'User'
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation("Nieuwe gebruiker geregistreerd: {Email}", model.Email);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Chat");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Gebruiker ingelogd: {Email}", model.Email);
                return LocalRedirect(returnUrl ?? "/Chat");
            }

            if (result.RequiresTwoFactor)
            {
                // Redirect naar 2FA-verificatie als 2FA is ingeschakeld
                return RedirectToAction("Verify2FA", new { returnUrl, model.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account vergrendeld voor: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Account is tijdelijk vergrendeld. Probeer het later opnieuw.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Ongeldige inloggegevens.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Gebruiker uitgelogd");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Enable2FA
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Genereer authenticator-sleutel
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var sharedKey = FormatKey(unformattedKey!);
            var authenticatorUri = GenerateQrCodeUri(user.Email!, unformattedKey!);

            // Genereer QR-code als PNG data URL
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(5);
            var qrBase64 = Convert.ToBase64String(qrBytes);

            var model = new Enable2FAViewModel
            {
                SharedKey = sharedKey,
                QrCodeImageUrl = $"data:image/png;base64,{qrBase64}",
                AuthenticatorUri = authenticatorUri
            };

            return View(model);
        }

        // GET: /Account/Verify2FA
        [HttpGet]
        public IActionResult Verify2FA(string? returnUrl = null, bool rememberMe = false)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new Verify2FAViewModel { RememberMe = rememberMe });
        }

        // POST: /Account/Verify2FA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify2FA(Verify2FAViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            // Verwijder spaties en koppeltekens die de user mogelijk invoert
            var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                code, model.RememberMe, rememberClient: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("2FA geslaagd");
                return LocalRedirect(returnUrl ?? "/Chat");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account vergrendeld.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Ongeldige verificatiecode.");
            return View(model);
        }

        // Hulpfuncties voor TOTP

        private static string FormatKey(string unformattedKey)
        {
            // Zet de Base32 key om naar blokken van 4 tekens voor leesbaarheid
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
                result.Append(unformattedKey.AsSpan(currentPosition));
            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            // otpauth:// URI formaat voor Google Authenticator / Aegis / etc.
            return $"otpauth://totp/ShowcaseChat:{_urlEncoder.Encode(email)}" +
                   $"?secret={unformattedKey}&issuer=ShowcaseChat&digits=6";
        }
    }
}
