# 06 – Deployment Plan (OTAP)

## OTAP Omgevingen

| Fase | Omgeving | Doel | Database |
|------|----------|------|----------|
| O | Ontwikkeling (localhost) | Actief bouwen en testen | SQLite (lokaal) |
| T | Test (lokale CI) | Automatische tests draaien | SQLite (in-memory) |
| A | Acceptatie (Skylab/staging) | Demo aan docent | SQLite of SQL Server |
| P | Productie (Skylab/deployment) | Eindresultaat showcase | SQLite of SQL Server |

> **Aanname**: Skylab is het schoolplatform voor deployment. Vraag je docent om
> toegang en de juiste URL/instructies als dit nog niet geconfigureerd is.

---

## Lokale Ontwikkeling (O)

### Vereisten
- .NET 8 SDK
- Node.js 18+ (voor Vite/Vue/Cypress)
- VS Code of Visual Studio 2022

### Starten
```bash
# UC3 Chat app starten
cd UC3-Chatsysteem/Showcase-Chat
dotnet user-secrets set "DefaultAdminPassword" "Admin@1234!"
dotnet ef database update
dotnet run

# In apart terminal: Vite watch (hot reload)
cd wwwroot-src
npm install
npm run dev

# UC2 API ook starten als je contactformulier wil testen
cd UC2-Contactpagina/ShowcaseAPI
dotnet run
```

### Secrets Beheer (NOOIT in git)
```bash
# Mailtrap wachtwoord opslaan
cd UC2-Contactpagina/ShowcaseAPI
dotnet user-secrets set "Mailtrap:Password" "jouw-echte-wachtwoord"

# Admin startaccount wachtwoord
cd UC3-Chatsysteem/Showcase-Chat
dotnet user-secrets set "Seed:AdminPassword" "Admin@Showcase2024!"
```

**appsettings.json mag WEL bevatten** (geen geheimen):
```json
{
  "Mailtrap": {
    "Host": "sandbox.smtp.mailtrap.io",
    "Port": 2525,
    "Username": "b854595246ec99",
    "Password": ""  // leeg! ingevuld via user-secrets
  }
}
```

---

## GitHub Actions CI (T – Test omgeving)

**Bestand**: `.github/workflows/ci.yml`

```yaml
name: CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore UC3-Chatsysteem/Showcase-Chat/Showcase-Chat.csproj

      - name: Build
        run: dotnet build UC3-Chatsysteem/Showcase-Chat/Showcase-Chat.csproj --no-restore

      - name: Run unit tests
        run: dotnet test UC3-Chatsysteem/ShowcaseChat.Tests/ShowcaseChat.Tests.csproj --no-build --verbosity normal

  cypress:
    runs-on: ubuntu-latest
    needs: test
    steps:
      - uses: actions/checkout@v4
      - name: Run Cypress tests
        uses: cypress-io/github-action@v6
        with:
          start: dotnet run --project UC3-Chatsysteem/Showcase-Chat
          wait-on: 'https://localhost:7xxx'
```

---

## Skylab Deployment (A/P)

> Pas aan op basis van instructies van je docent.

### Stap 1: Publiceer de app
```bash
cd UC3-Chatsysteem/Showcase-Chat
dotnet publish -c Release -o ./publish

# Of voor specifiek runtime:
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish
```

### Stap 2: Omgevingsvariabelen instellen op Skylab
Zet de volgende environment variables in Skylab/IIS/hosting:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/path/to/chat.db
Seed__AdminPassword=<veilig wachtwoord>
Mailtrap__Password=<mailtrap wachtwoord>
```

### Stap 3: Database migratie op server
```bash
# In publish folder of via EF CLI:
dotnet Showcase-Chat.dll --migrate  # (als je een migratiecommando implementeert)
# OF automatisch bij startup via: app.MigrateDatabase()
```

### Stap 4: HTTPS certificaat
- Skylab regelt SSL/TLS certificaat
- In `appsettings.Production.json`: HSTS inschakelen

---

## Security bij Deployment (ASVS V14.1)

| Check | Hoe controleren |
|-------|----------------|
| Geen debug info in productie | `ASPNETCORE_ENVIRONMENT=Production` → geen stack traces |
| Secrets niet in git | `git log --all -- appsettings*.json` controleren |
| HTTPS verplicht | Test: HTTP-request → redirect 301 naar HTTPS |
| Swagger alleen in development | `if (app.Environment.IsDevelopment())` check |
| Database bestand buiten webroot | SQLite-bestand in `/data/` niet in `wwwroot/` |

---

## Deployment Checklist

- [ ] `dotnet user-secrets` geconfigureerd lokaal
- [ ] Geen hardcoded wachtwoorden in appsettings.json
- [ ] `.gitignore` bevat `appsettings.Development.json`, `*.db`
- [ ] GitHub Actions CI draait succesvol
- [ ] `dotnet publish` werkt zonder errors
- [ ] Productie-URL bereikbaar via HTTPS
- [ ] Admin-account aangemaakt na eerste deploy
- [ ] OWASP ZAP scan uitgevoerd op staging-URL
- [ ] Security rapport opgeslagen in `/plan/security-report-zap.html`
- [ ] Deployment beschreven in portfolio

---

## Rollback Plan

Bij problemen na deployment:
1. Vorige versie deployen via GitHub (tag vorige werkende commit)
2. SQLite database backup terugzetten (zet backup-script op server)
3. Bij dataverlies: herstel vanuit git-getagde migratie

```bash
# Backup script (periodiek uitvoeren op server)
cp /data/chat.db /data/chat_backup_$(date +%Y%m%d).db
```
