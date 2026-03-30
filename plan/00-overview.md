# 00 – Project Overzicht & Scope

## Wat is dit project?
Een student-showcasewebsite bestaande uit meerdere **Use Cases (UC's)**,
gebouwd met **ASP.NET Core MVC (.NET 8)** en een bijbehorende REST API.
Het doel is aantonen van full-stack webontwikkelingsvaardigheden inclusief
security, testing en deployment.

---

## Projectstructuur (mappen)

```
ShowcaseWebdev/
├── UC1-Profielpagina/              ← Profielpagina MVC app (UC1, US1)
│   └── Showcase-Profielpagina/
├── UC2-Contactpagina/              ← Contactpagina MVC + REST API (UC2, US2)
│   ├── Showcase-Contactpagina/
│   └── ShowcaseAPI/
├── UC2-Contactpagina-week-3-Vite/  ← Zelfde UC2 maar met Vite frontend (US3)
│   ├── Showcase-Contactpagina/
│   ├── ShowcaseAPI/
│   └── Vite/
├── UC3-Chatsysteem/                ← NIEUW: eigen UC chat (US4-US6)  ← BOUWEN
│   ├── Showcase-Chat/              (ASP.NET Core MVC + Identity + EF Core + SignalR)
│   └── ShowcaseChat.Tests/         (xUnit unit tests)
├── informatie/                     ← Alle documentatie
└── plan/                           ← Dit planningsmap
```

---

## Use Cases & Weken

| Week | User Story | Use Case | Status |
|------|-----------|----------|--------|
| 1 | US1 | UC1 – Profielpagina | Grotendeels klaar (GDPR aanwezig) |
| 2 | US2 | UC2 – Contactpagina (stub, mail nog niet werkend) | Deels klaar |
| 3 | US3 | UC2 + Vite frontend | Deels klaar |
| 4 | US4 | **Eigen UC – Auth/Authz + Chat** | **BOUWEN** |
| 5 | US5 | **Eigen UC – Realtime (SignalR) + Vue frontend** | **BOUWEN** |
| 6 | US6 | **Testing + Security hardening** | **BOUWEN** |

---

## Eigen UC – Chat Systeem (samenvatting)

**Functionaliteit:**
- Registreren / inloggen (ASP.NET Core Identity)
- Rollen: `User` en `Admin`
- Ingelogde gebruiker kan berichten sturen (chatten)
- Berichten worden persistent opgeslagen (SQLite via EF Core)
- `User` ziet alleen eigen gesprekken
- `Admin` ziet alle gesprekken + kan filteren op gebruiker

**Tech stack eigen UC:**
| Laag | Keuze | Reden |
|------|-------|-------|
| Framework | ASP.NET Core MVC (.NET 8) | Consistent met UC1/UC2 |
| Auth | ASP.NET Core Identity | Ingebouwd, RBAC, veilig |
| Database | SQLite + EF Core | Simpel, portable, geen server nodig |
| Realtime | SignalR (Hub) | Ingebouwd in .NET, simpel op te zetten |
| Frontend | Razor Views + Vue 3 (Vite) voor chat-component | Voldoet aan US5 Client-eis |
| CSS | Bootstrap 5 | Consistent met UC1/UC2 |
| Tests | xUnit + Moq | Standaard .NET test stack |
| E2E Tests | Cypress | Vereist door US6 |
| Logging | Microsoft.Extensions.Logging (ILogger) | Ingebouwd, geen extra dep nodig |

---

## Scope-afbakening (bewuste keuzes)

1. **Geen multi-tenant / group chats** — simpele "publieke chatroom per context" of 1-op-1 berichten naar Admin
2. **Geen e-mail verificatie bij registratie** — wel 2FA via TOTP (US4 security-eis)
3. **Geen aparte microservice** — alles in één MVC app (maar wél SignalR Hub als subsysteem)
4. **SQLite** ipv SQL Server — simpeler voor development/demo, EF Core maakt migratie naar SQL Server triviaal
5. **Polling als fallback** — SignalR werkt via WebSocket met automatische fallback naar long-polling
