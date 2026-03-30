# 01 – Requirements Checklist

Alle eisen uit de Requirements showcase (PDF US4-US6) plus
de eigen UC-opdracht. Status: [ ] = nog te doen, [x] = klaar.

---

## UC1 – Profielpagina (US1) — al gestart

- [x] Profielpagina toont CV / developer-beschrijving
- [x] Skills en opleiding zichtbaar
- [x] GDPR-consent aanwezig (localStorage-gebaseerd)
- [x] Responsive design (Bootstrap 5)
- [ ] **Controleer**: HTTPS geconfigureerd? (HSTS in productie)
- [ ] **Controleer**: Afbeeldingen geoptimaliseerd / geen grote binaries in git

---

## UC2 – Contactpagina (US2/US3) — deels klaar, mail nog stub

- [x] Contactformulier met velden: voornaam, achternaam, e-mail, telefoon
- [x] Server-side validatie via DataAnnotations (Required, StringLength, EmailAddress, Phone)
- [x] `[ValidateAntiForgeryToken]` aanwezig op POST
- [x] Foutmelding bij invalide invoer
- [x] Succes-melding na verzenden
- [x] MVC app communiceert via HTTP naar REST API (ShowcaseAPI)
- [ ] **TODO**: HTTP POST daadwerkelijk uitvoeren in `ContactController.cs` (stub vervangen)
- [ ] **TODO**: Mailtrap-integratie werkend maken in `MailController.cs`
- [ ] **TODO**: Mailtrap SMTP credentials veilig in `appsettings.json` / `user-secrets`
- [x] Vite-versie aanwezig (UC2-week-3-Vite)
- [ ] **TODO**: Client-side validatie werkend in Vite versie (ESLint geconfigureerd)
- [ ] **TODO**: CAPTCHA of rate-limiting op contactformulier (OWASP spam-preventie)

---

## US4 – Week 4: Authenticatie & Autorisatie

### Server (NFR)
- [ ] Gebruikers kunnen registreren (eigen account aanmaken)
- [ ] Gebruikers kunnen inloggen
- [ ] Twee rollen: `User` en `Admin`
- [ ] `User` heeft toegang tot eigen chatberichten only
- [ ] `Admin` heeft toegang tot alle chatberichten
- [ ] Server-side `[Authorize(Roles = "...")]` op alle gevoelige endpoints

### Security (NFR)
- [ ] 2FA geïmplementeerd (TOTP via authenticator app)
- [ ] RBAC via ASP.NET Core Identity roles
- [ ] Policies gedefinieerd (bijv. `RequireAdminRole`)
- [ ] Geen client-side-only autorisatie (alles server-side enforced)

---

## US5 – Week 5: Realtime & Frontend Framework

### Client (NFR)
- [ ] Chat-UI gebouwd als Vue 3 component (via Vite)
- [ ] Component voldoet aan web component / modern JS framework eis

### Server (NFR)
- [ ] SignalR Hub geïmplementeerd (`ChatHub`)
- [ ] Berichten worden realtime via SignalR bezorgd bij connected clients
- [ ] Fallback naar polling indien SignalR niet beschikbaar

### Security (NFR) — ASVS V9 Communication
- [ ] HTTPS verplicht (HSTS geconfigureerd)
- [ ] TLS 1.2+ alleen toegestaan
- [ ] SignalR verbinding loopt over WSS (WebSocket Secure)
- [ ] CORS-policy restrictief geconfigureerd (alleen eigen origin)

---

## US6 – Week 6: Testing & Security Hardening

### Server – Backend Tests (NFR)
- [ ] xUnit projectaangemaakt (`ShowcaseChat.Tests`)
- [ ] Unit tests voor `ChatService` (business logic)
- [ ] Unit tests voor `MessageRepository`
- [ ] Mocking via `Moq` of `NSubstitute`
- [ ] Argumentatie gedocumenteerd waarom deze tests belangrijk zijn
- [ ] Minimaal: happy path + unhappy path per functie

### Client – Cypress Tests (NFR)
- [ ] Cypress geïnstalleerd in `/cypress/` folder
- [ ] E2E test: registreren
- [ ] E2E test: inloggen
- [ ] E2E test: bericht versturen
- [ ] E2E test: Admin filtert op gebruiker
- [ ] Alle testtechnieken gebruikt: unit, integratie, E2E

### Security – ASVS V5 Validation, Sanitization & Encoding
- [ ] Input validation op alle form fields (server-side)
- [ ] HTML-encoding van output (Razor doet dit standaard, controleren)
- [ ] SQL-injectie preventie via EF Core parameterized queries (geen raw SQL)
- [ ] XSS preventie: `Content-Security-Policy` header geconfigureerd

### Security – ASVS V13 API and Web Service
- [ ] API endpoints hebben `[Authorize]` waar nodig
- [ ] API retourneert geen stack traces in productie
- [ ] Rate limiting op auth-endpoints (login pogingen)
- [ ] Anti-CSRF tokens op alle state-changing requests

### Security – ASVS V14.1 Build and Deploy (Principles)
- [ ] Deployment-proces gedocumenteerd (OTAP)
- [ ] Security test rapport aanwezig (zie `/plan/05-test-plan.md`)
- [ ] Geen secrets in Git (gebruik dotnet user-secrets of environment vars)
- [ ] `.gitignore` bevat `appsettings.Development.json` met credentials

---

## Non-Functional Requirements (algemeen)

- [ ] Logging aanwezig op kritieke plekken (auth events, errors)
- [ ] Error handling: geen raw exceptions naar gebruiker
- [ ] Performance: pagina laadt binnen 3 seconden (geen zware blocking calls)
- [ ] Toegankelijkheid: semantische HTML, alt-teksten op afbeeldingen
- [ ] Consistente huisstijl over UC1, UC2, UC3

---

## Portfolio / Aftekenmomenten (aannames)

> **Aanname**: op basis van de handleiding wordt per week afgetekend.
> Controleer met je docent welke exacte deliverables per week ingeleverd moeten worden.

| Week | Deliverable |
|------|------------|
| 1 | UC1 draait, profielpagina zichtbaar |
| 2 | UC2 contactformulier werkt + mail verstuurd via Mailtrap |
| 3 | UC2 Vite-versie met client-side validatie |
| 4 | Eigen UC met auth (register/login/rollen) + 2FA |
| 5 | Eigen UC met SignalR realtime chat + Vue component |
| 6 | Tests (xUnit + Cypress) + security rapport + deployment documentatie |

---

## Onduidelijkheden & Aannames

| # | Onduidelijkheid | Aanname |
|---|----------------|---------|
| 1 | UC2 mail: wachtwoord in credentials.txt is gemaskerd | Gebruik `dotnet user-secrets` voor echte waarde; vraag docent om volledig wachtwoord |
| 2 | "Inbox" of "chatroom" voor eigen UC? | Eenvoudige chatroom-stijl: iedereen kan berichten sturen; admin ziet alles |
| 3 | Skylab deployment: geen Skylab-docs aanwezig | Zie `/plan/06-deployment-plan.md`; vraag docent om Skylab-toegang als vereist |
| 4 | UC2 API heeft Azure AD in Program.cs | Dit is vermoedelijk een scaffolding-template; vervangen door simpelere auth of API key |
