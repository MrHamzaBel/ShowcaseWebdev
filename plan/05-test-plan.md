# 05 – Testplan

## Overzicht Testniveaus

| Niveau | Tool | Component | US |
|--------|------|-----------|-----|
| Unit tests | xUnit + Moq | MessageService, controllers | US6 Server |
| Integratie tests | xUnit + InMemory DB | AppDbContext, repositories | US6 Server |
| E2E tests | Cypress | Volledige flows in browser | US6 Client |
| Security scan | OWASP ZAP | Hele applicatie | US6 Security |

---

## 1. Unit Tests (xUnit + Moq)

### Waarom deze tests? (argumentatie voor rubric)
> "De unit tests voor MessageService zijn belangrijk omdat deze de core business-logica
> bewaken: de autorisatieregels die bepalen of een gebruiker berichten mag lezen.
> Als deze logica fout is, heeft security een kritiek lek. Unit tests vangen regressies
> vroeg op, vóór ze in productie komen. Mocking zorgt ervoor dat tests snel en
> deterministisch zijn, onafhankelijk van database of externe services."

### Test: MessageService

**Bestand**: `ShowcaseChat.Tests/MessageServiceTests.cs`

```csharp
// Test 1: Happy path – user haalt eigen berichten op
[Fact]
public async Task GetMessagesForUser_ReturnsOwnMessages()

// Test 2: Unhappy path – user probeert andermans berichten op te halen
[Fact]
public async Task GetMessagesForUser_ThrowsUnauthorized_WhenAccessingOtherUserMessages()

// Test 3: Admin mag alle berichten opvragen
[Fact]
public async Task GetAllMessages_ReturnsAll_WhenCalledByAdmin()

// Test 4: Lege content wordt geweigerd
[Fact]
public async Task SendMessage_ThrowsValidationException_WhenContentIsEmpty()

// Test 5: Content te lang wordt afgekapt / geweigerd
[Fact]
public async Task SendMessage_ThrowsValidationException_WhenContentExceeds1000Chars()

// Test 6: SenderId wordt server-side gezet, niet van client
[Fact]
public async Task SendMessage_UsesSenderIdFromContext_NotFromInput()
```

### Test: AccountController (integratie)

**Bestand**: `ShowcaseChat.Tests/AccountControllerTests.cs`

```csharp
// Test 7: Registratie met geldig model slaagt
[Fact]
public async Task Register_RedirectsToChat_WhenModelValid()

// Test 8: Registratie met ongeldig model geeft formulier terug
[Fact]
public async Task Register_ReturnsView_WhenModelInvalid()

// Test 9: Login met verkeerde gegevens geeft foutmelding
[Fact]
public async Task Login_ReturnsError_WhenCredentialsInvalid()
```

---

## 2. Integratietests (xUnit + InMemory DB)

**Bestand**: `ShowcaseChat.Tests/MessageRepositoryIntegrationTests.cs`

```csharp
// Test 10: Bericht wordt daadwerkelijk opgeslagen in DB
[Fact]
public async Task SendMessage_PersistsToDatabase()

// Test 11: Filter per gebruiker geeft correcte subset
[Fact]
public async Task GetMessagesByUser_ReturnsCorrectSubset()
```

Setup: gebruik `UseInMemoryDatabase("TestDb")` in DbContext factory.

---

## 3. Cypress E2E Tests

### Installatie
```bash
npm install cypress --save-dev
npx cypress open
```

### Configuratie `cypress.config.js`
```javascript
module.exports = {
  e2e: {
    baseUrl: 'https://localhost:7xxx', // port van UC3
    specPattern: 'cypress/e2e/**/*.cy.js',
  }
}
```

### Test: Authenticatie (`cypress/e2e/auth.cy.js`)
```javascript
// TC-AUTH-01: Registreren met geldige gegevens
it('registers a new user successfully', () => {
  cy.visit('/Account/Register')
  cy.get('#DisplayName').type('Testgebruiker')
  cy.get('#Email').type('test@example.com')
  cy.get('#Password').type('Test@1234!')
  cy.get('#ConfirmPassword').type('Test@1234!')
  cy.get('button[type=submit]').click()
  cy.url().should('include', '/Account/Enable2FA') // redirect naar 2FA setup
})

// TC-AUTH-02: Inloggen met geldige gegevens
it('logs in with valid credentials', () => { ... })

// TC-AUTH-03: Inloggen met ongeldige gegevens toont fout
it('shows error on invalid credentials', () => { ... })

// TC-AUTH-04: Uitloggen
it('logs out successfully', () => { ... })
```

### Test: Chat (`cypress/e2e/chat.cy.js`)
```javascript
// TC-CHAT-01: Anonieme gebruiker wordt doorgestuurd naar login
it('redirects anonymous user to login', () => {
  cy.visit('/Chat')
  cy.url().should('include', '/Account/Login')
})

// TC-CHAT-02: Ingelogde user kan bericht sturen
it('authenticated user can send a message', () => { ... })

// TC-CHAT-03: Bericht verschijnt in chatvenster
it('sent message appears in chat window', () => { ... })

// TC-CHAT-04: User ziet alleen eigen berichten
it('user only sees own messages', () => { ... })
```

### Test: Admin (`cypress/e2e/admin.cy.js`)
```javascript
// TC-ADMIN-01: User kan /Admin NIET bereiken
it('regular user cannot access admin dashboard', () => {
  cy.loginAsUser() // custom command
  cy.visit('/Admin')
  cy.url().should('not.include', '/Admin') // redirect of 403
})

// TC-ADMIN-02: Admin ziet alle berichten
it('admin sees all messages', () => { ... })

// TC-ADMIN-03: Admin kan filteren op gebruiker
it('admin can filter messages by user', () => { ... })
```

### Custom Cypress Commands (`cypress/support/commands.js`)
```javascript
Cypress.Commands.add('loginAsUser', () => {
  cy.session('user', () => {
    cy.visit('/Account/Login')
    cy.get('#Email').type('user@test.com')
    cy.get('#Password').type('Test@1234!')
    cy.get('button[type=submit]').click()
    // 2FA code invoeren (gebruik test-account met bekende seed)
  })
})

Cypress.Commands.add('loginAsAdmin', () => { ... })
```

---

## 4. Security Tests

### OWASP ZAP Scan
1. Download OWASP ZAP (gratis)
2. Start applicatie lokaal op HTTPS
3. ZAP → Automated Scan → URL: `https://localhost:7xxx`
4. Genereer rapport als HTML
5. Sla op als `/plan/security-report-zap.html`
6. Bekijk medium+ risico's en documenteer mitigaties

### Handmatige Security Checks
| Check | Hoe | Verwacht resultaat |
|-------|-----|-------------------|
| XSS via chatbericht | Stuur `<script>alert('xss')</script>` als bericht | Wordt als tekst getoond, geen alert |
| CSRF | Doe POST-request zonder anti-forgery token | 400 Bad Request |
| IDOR in admin filter | Als User, GET /Admin/FilterByUser?userId=andereId | 403 Forbidden of redirect login |
| SQL injectie | Stuur `' OR 1=1 --` in formuliervelden | Geen fout, geen data leak |
| Brute force | 6 keer verkeerd inloggen | Account locked |
| HTTPS | Doe HTTP-request | Redirect naar HTTPS |

---

## 5. Teststrategie Samenvatting

Alle vereiste testtechnieken (voor US6 Client/Server):

| Techniek | Toegepast waar |
|----------|---------------|
| Unit test | MessageService methodes |
| Integratie test | Database opslaan/ophalen |
| E2E test | Volledige gebruikersflows (Cypress) |
| Mocking | DbContext en UserManager in unit tests |
| Security test | ZAP scan + handmatige checks |
| Regressie | CI-run via GitHub Actions (zie deployment plan) |
