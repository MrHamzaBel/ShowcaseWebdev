# 04 – Implementatie Stappenplan

Volg deze volgorde strikt. Markeer elke stap als DONE wanneer klaar.

---

## FASE 0: UC2 Afmaken (stubtaken)

> Doe dit eerst — UC2 is al gestart maar heeft stubs.

### Stap 0.1 – UC2 ContactController POST werkend maken
**Bestand**: `UC2-Contactpagina/Showcase-Contactpagina/Controllers/ContactController.cs`
- Vervang de stub `HttpResponseMessage response = new HttpResponseMessage();`
- Door: `HttpResponseMessage response = await _httpClient.PostAsync("/api/mail", content);`

### Stap 0.2 – UC2 MailController Mailtrap werkend maken
**Bestand**: `UC2-Contactpagina/ShowcaseAPI/Controllers/MailController.cs`
- Voeg SmtpClient implementatie toe met Mailtrap credentials
- Haal credentials op uit `appsettings.json` (via IConfiguration)
- Sla credentials op in `dotnet user-secrets` (NOOIT hardcoden)

### Stap 0.3 – UC2 API Program.cs opschonen
**Bestand**: `UC2-Contactpagina/ShowcaseAPI/Program.cs`
- Azure AD auth verwijderen (niet nodig voor intern gebruik)
- Simpelere configuratie; optioneel: API key authentication

---

## FASE 1: UC3 Projectopzet (US4 basis)

### Stap 1.1 – Nieuw project aanmaken
```bash
# Maak UC3-Chatsysteem map
mkdir UC3-Chatsysteem
cd UC3-Chatsysteem

# ASP.NET Core MVC met Individual Auth (Identity scaffolden)
dotnet new mvc -n Showcase-Chat --auth Individual
cd Showcase-Chat

# NuGet packages toevoegen
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Microsoft.AspNetCore.Authentication.Google  # optioneel voor social login
```

### Stap 1.2 – ApplicationUser model
**Nieuw bestand**: `Models/ApplicationUser.cs`
```csharp
// Extend IdentityUser met extra veld DisplayName
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
```

### Stap 1.3 – Message model
**Nieuw bestand**: `Models/Message.cs`
```csharp
public class Message
{
    public int Id { get; set; }
    [Required, StringLength(1000)]
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SenderId { get; set; }
    public ApplicationUser Sender { get; set; }
}
```

### Stap 1.4 – AppDbContext
**Nieuw bestand**: `Data/AppDbContext.cs`
```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Message> Messages { get; set; }
    // configuratie in OnModelCreating
}
```

### Stap 1.5 – Program.cs configureren
- Identity services toevoegen
- SQLite connectie
- Rollen seeden (Admin, User)
- SignalR toevoegen
- Security headers middleware
- HTTPS/HSTS

### Stap 1.6 – Database migratie
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Stap 1.7 – Rollen seeden
**Bestand**: `Data/DbSeeder.cs`
- Admin en User rollen aanmaken als ze niet bestaan
- Standaard admin-account aanmaken (wachtwoord via user-secrets)

---

## FASE 2: Authenticatie & Autorisatie (US4)

### Stap 2.1 – AccountController
**Nieuw bestand**: `Controllers/AccountController.cs`
- `GET/POST Register` – registreer nieuwe User
- `GET/POST Login` – inloggen + redirect na 2FA
- `POST Logout`
- `GET/POST Enable2FA` – 2FA activeren via TOTP
- `GET/POST Verify2FA` – verificatiecode invoeren bij inloggen

### Stap 2.2 – Register View
**Nieuw bestand**: `Views/Account/Register.cshtml`
- Formulier: DisplayName, Email, Wachtwoord, Bevestig wachtwoord
- Server-side validatie via RegisterViewModel

### Stap 2.3 – Login View
**Nieuw bestand**: `Views/Account/Login.cshtml`
- Formulier: Email, Wachtwoord, Remember me
- Link naar Register

### Stap 2.4 – 2FA Setup Views
**Nieuwe bestanden**:
- `Views/Account/Enable2FA.cshtml` – QR-code tonen voor authenticator app
- `Views/Account/Verify2FA.cshtml` – TOTP code invoeren

### Stap 2.5 – Testen authenticatie
- [ ] Registreren werkt
- [ ] Inloggen werkt
- [ ] 2FA setup werkt
- [ ] 2FA verificatie werkt
- [ ] Account lockout na 5 pogingen
- [ ] Anonieme gebruiker wordt redirect naar /Account/Login

---

## FASE 3: Chat functionaliteit (US4 + US5)

### Stap 3.1 – IMessageService interface
**Nieuw bestand**: `Services/IMessageService.cs`
```csharp
public interface IMessageService
{
    Task<IEnumerable<Message>> GetMessagesForUserAsync(string userId);
    Task<IEnumerable<Message>> GetAllMessagesAsync(); // Admin only
    Task<IEnumerable<Message>> GetMessagesByUserAsync(string targetUserId); // Admin filter
    Task<Message> SendMessageAsync(string senderId, string content);
}
```

### Stap 3.2 – MessageService implementatie
**Nieuw bestand**: `Services/MessageService.cs`
- Implementeer alle interface-methoden
- Gebruik AppDbContext (via DI)
- Valideer senderId server-side
- Sla altijd UTC-tijd op

### Stap 3.3 – ChatController
**Nieuw bestand**: `Controllers/ChatController.cs`
- `[Authorize]` op klasse-niveau
- `GET Index` – toon chatroom
- `POST Send` – bericht opslaan (fallback als SignalR niet werkt)

### Stap 3.4 – AdminController
**Nieuw bestand**: `Controllers/AdminController.cs`
- `[Authorize(Roles = "Admin")]` op klasse-niveau
- `GET Index` – alle berichten tonen
- `GET FilterByUser(string userId)` – filteren per gebruiker
- `GET Users` – lijst van alle gebruikers

### Stap 3.5 – Chat Views
**Nieuwe bestanden**:
- `Views/Chat/Index.cshtml` – chatroom UI (Vue component mountpunt + fallback)
- `Views/Admin/Index.cshtml` – admin dashboard
- `Views/Admin/FilterByUser.cshtml` – gefilterde weergave

---

## FASE 4: SignalR Hub (US5)

### Stap 4.1 – ChatHub
**Nieuw bestand**: `Hubs/ChatHub.cs`
```csharp
[Authorize] // Alleen ingelogde gebruikers
public class ChatHub : Hub
{
    // SendMessage: sla op in DB én stuur naar alle clients
    public async Task SendMessage(string content)
    {
        // BELANGRIJK: SenderId ALTIJD van Context.User, nooit van client!
        var senderId = Context.UserIdentifier;
        var message = await _messageService.SendMessageAsync(senderId, content);
        await Clients.All.SendAsync("ReceiveMessage", message.Sender.DisplayName, message.Content, message.CreatedAt);
    }
}
```

### Stap 4.2 – SignalR registratie in Program.cs
```csharp
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/chatHub");
```

### Stap 4.3 – Vue 3 Chat Component (Vite)
**Nieuwe map**: `wwwroot-src/` (Vite source, apart van wwwroot)
```
wwwroot-src/
├── package.json
├── vite.config.js
└── src/
    └── ChatApp.vue   ← Vue component voor real-time chat
```

De Vue component:
- Verbindt via SignalR JS client
- Toont berichten lijst
- Input voor nieuw bericht
- Stuurt bericht via SignalR Hub

```bash
# In wwwroot-src/
npm install
npm install @microsoft/signalr
npm install vue
```

### Stap 4.4 – Vite build integreren
- `vite.config.js`: output naar `wwwroot/js/chat-app.js`
- In Chat/Index.cshtml: `<script src="/js/chat-app.js" type="module"></script>`
- Mount-element in view: `<div id="chat-app"></div>`

---

## FASE 5: Tests (US6)

### Stap 5.1 – xUnit test project aanmaken
```bash
cd UC3-Chatsysteem
dotnet new xunit -n ShowcaseChat.Tests
dotnet add ShowcaseChat.Tests/ShowcaseChat.Tests.csproj reference Showcase-Chat/Showcase-Chat.csproj
dotnet add ShowcaseChat.Tests package Moq
dotnet add ShowcaseChat.Tests package Microsoft.EntityFrameworkCore.InMemory
```

### Stap 5.2 – MessageService unit tests
**Bestand**: `ShowcaseChat.Tests/MessageServiceTests.cs`
- Test: SendMessage slaat op in DB
- Test: GetMessagesForUser retourneert alleen eigen berichten
- Test: GetMessagesForUser gooit exception bij onbevoegde toegang
- Test: lege content wordt geweigerd
- Test: content langer dan 1000 tekens wordt geweigerd

### Stap 5.3 – Cypress installeren & configureren
```bash
# In root van project of UC3-Chatsysteem:
npm install cypress --save-dev
npx cypress open  # eerste keer setup
```

### Stap 5.4 – Cypress tests schrijven
**Bestanden**: `cypress/e2e/`
- `auth.cy.js` – registreren, inloggen, uitloggen
- `chat.cy.js` – bericht versturen, zichtbaar in UI
- `admin.cy.js` – admin ziet alle berichten, filter werkt
- `security.cy.js` – anoniem naar /Chat redirect naar login, user kan /Admin niet bereiken

---

## FASE 6: Security Hardening & Deployment (US6)

### Stap 6.1 – Security headers toevoegen (zie 03-security.md)
### Stap 6.2 – Rate limiting op auth-endpoints
### Stap 6.3 – OWASP ZAP scan uitvoeren
### Stap 6.4 – dotnet user-secrets voor alle credentials
### Stap 6.5 – Deployment documentatie (zie 06-deployment-plan.md)
### Stap 6.6 – Security rapport schrijven

---

## Volgorde samenvatting

```
0. UC2 stubs afmaken (30 min)
1. UC3 projectopzet + Identity + EF Core + SQLite (2-3 uur)
2. Auth: Register/Login/Rollen/2FA (3-4 uur)
3. Chat: Service/Controller/Views (2-3 uur)
4. SignalR Hub + Vue component (3-4 uur)
5. xUnit tests (2-3 uur)
6. Cypress tests (2-3 uur)
7. Security hardening (1-2 uur)
8. Deployment documentatie (1 uur)
```
