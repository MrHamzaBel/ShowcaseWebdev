# 03 – Security Plan

## SSDLC Aanpak
Dit project volgt de **Secure Software Development Lifecycle (SSDLC)**:
1. **Requirements**: security-eisen opgenomen in requirements (zie checklist)
2. **Design**: threat modelling hieronder
3. **Implementatie**: mitigaties toegepast in code (zie commentaar in broncode)
4. **Test**: security-testen in testplan (zie 05-test-plan.md)
5. **Deploy**: OTAP-proces (zie 06-deployment-plan.md)

---

## Threat Model – STRIDE per component

### Component: Authenticatie (Login / Register)

| Threat | STRIDE | Risico | Mitigatie |
|--------|--------|--------|-----------|
| Brute-force login | Spoofing | Hoog | Rate limiting via `AddRateLimiter`, account lockout (`MaxFailedAccessAttempts = 5`) |
| Credential stuffing | Spoofing | Hoog | 2FA verplicht na registratie (TOTP) |
| Session hijacking | Spoofing | Middel | HTTPS only, `Secure` + `HttpOnly` cookie flags |
| Zwakke wachtwoorden | Spoofing | Middel | Identity password policy (min 8 tekens, hoofdletter, cijfer, symbool) |
| Registratie met nep-e-mail | Spoofing | Laag | E-mail format validatie (buiten scope: e-mail verificatie) |

### Component: Chat berichten

| Threat | STRIDE | Risico | Mitigatie |
|--------|--------|--------|-----------|
| XSS via chatbericht | Tampering | Hoog | Razor HTML-encoding automatisch; CSP header |
| CSRF op POST-endpoints | Tampering | Hoog | `[ValidateAntiForgeryToken]` op alle POST-acties |
| User leest andermans berichten | Elevation of Privilege | Hoog | Server-side userId check in MessageService |
| SQL injectie | Tampering | Middel | EF Core gebruikt altijd parameterized queries (geen raw SQL) |
| Opslaan van gevoelige data in berichten | Information Disclosure | Laag | Scope: alleen normale chatberichten, geen speciale data |

### Component: Admin Dashboard

| Threat | STRIDE | Risico | Mitigatie |
|--------|--------|--------|-----------|
| Niet-admin krijgt admin-toegang | Elevation of Privilege | Kritiek | `[Authorize(Roles = "Admin")]` op alle admin-endpoints |
| IDOR (admin-filter manipuleren) | Elevation of Privilege | Middel | Valideer userId-parameter server-side tegen bestaande users |
| Mass assignment | Tampering | Middel | `[Bind(...)]` of ViewModels gebruiken, nooit direct entity binden |

### Component: SignalR Hub

| Threat | STRIDE | Risico | Mitigatie |
|--------|--------|--------|-----------|
| Anonieme verbinding met hub | Elevation of Privilege | Hoog | `[Authorize]` op ChatHub |
| Bericht spoofing (andere userId) | Spoofing | Kritiek | SenderId altijd ophalen van `Context.User`, nooit van client |
| Denial of Service via bericht-spam | DoS | Middel | Input validatie: max 1000 tekens, server-side limiet |

### Component: API / MVC (UC2 contact)

| Threat | STRIDE | Risico | Mitigatie |
|--------|--------|--------|-----------|
| API zonder auth aanroepen | Elevation of Privilege | Middel | API key header of JWT vereist voor interne API-calls |
| SMTP credentials in code/git | Information Disclosure | Hoog | `dotnet user-secrets`, nooit in appsettings.json hardcoden |

---

## Mitigaties – Implementatiedetails

### 1. Authenticatie & 2FA
```csharp
// Program.cs – Identity configuratie
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    // Wachtwoordbeleid
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Account lockout na herhaalde mislukte pogingen
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // 2FA verplicht
    options.SignIn.RequireConfirmedAccount = false; // email verificatie buiten scope
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();
```

### 2. Security Headers
```csharp
// Program.cs – Security headers middleware
app.Use(async (context, next) => {
    // Content Security Policy – voorkomt XSS
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:");
    // Voorkomt clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    // Voorkomt MIME-type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    await next();
});
```

### 3. HTTPS & HSTS
```csharp
// Altijd HTTPS
app.UseHttpsRedirection();
// HSTS in productie (1 jaar)
app.UseHsts();
```

### 4. CSRF Bescherming
```csharp
// In alle POST-controllers:
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult SendMessage(ChatViewModel model) { ... }

// In views: @Html.AntiForgeryToken() of automatisch via tag helpers
```

### 5. Autorisatie Server-Side
```csharp
// Controller-niveau
[Authorize(Roles = "Admin")]
public class AdminController : Controller { ... }

// Service-niveau (extra check in business logic)
public async Task<IEnumerable<Message>> GetMessagesForUserAsync(string requestingUserId, string targetUserId)
{
    // Controleer of requestingUser admin is of targetUser zelf
    if (requestingUserId != targetUserId && !await _userManager.IsInRoleAsync(user, "Admin"))
        throw new UnauthorizedAccessException("Geen toegang tot berichten van andere gebruikers");
}
```

### 6. Input Validatie & Output Encoding
```csharp
// Model-niveau: DataAnnotations
[Required]
[StringLength(1000, MinimumLength = 1)]
public string Content { get; set; }

// Razor encoding automatisch: @Model.Content (veilig)
// NOOIT: @Html.Raw(Model.Content) op user-gegenereerde content
```

### 7. EF Core Parameterized Queries
```csharp
// EF Core gebruikt altijd parameterized queries
// Veilig:
var messages = await _context.Messages
    .Where(m => m.SenderId == userId)
    .ToListAsync();

// NOOIT raw SQL met string concatenation:
// _context.Database.ExecuteSqlRaw($"SELECT * WHERE Id = {userId}"); // FOUT
```

### 8. Logging van Security Events
```csharp
// Loggen van: failed logins, unauthorized attempts, admin acties
_logger.LogWarning("Failed login attempt for user {Email} from IP {IP}", email, ip);
_logger.LogInformation("Admin {AdminId} viewed messages for user {UserId}", adminId, userId);
```

---

## ASVS Mapping

| ASVS Versie | Beschrijving | Status |
|-------------|--------------|--------|
| V2 Authentication | 2FA, wachtwoordbeleid, lockout | Gepland (US4) |
| V4 Access Control | RBAC, server-side authz checks | Gepland (US4) |
| V5 Validation & Encoding | Input validatie, HTML-encoding, CSP | Gepland (US6) |
| V9 Communication | HTTPS, HSTS, TLS, WSS voor SignalR | Gepland (US5) |
| V13 API & Web Service | API auth, rate limiting, error handling | Gepland (US6) |
| V14.1 Build & Deploy | OTAP, security rapport, geen secrets in git | Gepland (US6) |

---

## Portfolio Documentatie (wat moet aantoonbaar zijn)

Voor het security-portfolio moet je kunnen aantonen:

1. **Threat model**: dit document (per component, STRIDE)
2. **2FA implementatie**: screenshot van TOTP-setup flow
3. **RBAC**: code + screenshot admin vs user toegang
4. **CSP header**: bewijs via browser DevTools → Network tab → Response Headers
5. **Pentest/scan**: OWASP ZAP scan output (zie testplan)
6. **Geen secrets in git**: `.gitignore` + `user-secrets` bewijs
7. **Logging**: voorbeeld log-output met security events
