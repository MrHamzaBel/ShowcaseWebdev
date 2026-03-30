# 02 – Architectuur

## C4 Context Diagram (niveau 1)

```
┌─────────────────────────────────────────────────────────────────┐
│                        Internet / Browser                        │
│                                                                  │
│   ┌──────────┐     ┌──────────────┐     ┌────────────────────┐ │
│   │  Bezoeker│     │ Ingelogde    │     │     Admin          │ │
│   │(anoniem) │     │ User         │     │                    │ │
│   └────┬─────┘     └──────┬───────┘     └─────────┬──────────┘ │
│        │                  │                        │            │
└────────┼──────────────────┼────────────────────────┼────────────┘
         │ HTTPS             │ HTTPS/WSS              │ HTTPS/WSS
         ▼                  ▼                        ▼
┌──────────────────────────────────────────────────────────────┐
│                  ShowcaseWebdev Applicaties                   │
│                                                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │ UC1-Profielpagina│  │ UC2-Contactpag. │  │ UC3-Chat    │  │
│  │ (MVC, .NET 8)   │  │ (MVC + REST API)│  │ (MVC+SignalR│  │
│  │ Port: 7xxx      │  │ MVC: 7xxx       │  │ Port: 7xxx) │  │
│  │                 │  │ API: 7278       │  │             │  │
│  └─────────────────┘  └────────┬────────┘  └──────┬──────┘  │
│                                │                   │         │
│                           ┌────▼────┐        ┌─────▼─────┐  │
│                           │Mailtrap │        │ SQLite DB │  │
│                           │(extern) │        │(EF Core)  │  │
│                           └─────────┘        └───────────┘  │
└──────────────────────────────────────────────────────────────┘
```

---

## C4 Container Diagram – UC3 Chat (niveau 2)

```
┌────────────────────────────────────────────────────────────┐
│                    UC3-Chatsysteem                         │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Showcase-Chat (ASP.NET Core MVC)        │  │
│  │                                                      │  │
│  │  Controllers/                                        │  │
│  │  ├── AccountController  ← Register, Login, 2FA      │  │
│  │  ├── ChatController     ← Berichten weergeven        │  │
│  │  └── AdminController    ← Alle chats, filter        │  │
│  │                                                      │  │
│  │  Hubs/                                               │  │
│  │  └── ChatHub            ← SignalR realtime           │  │
│  │                                                      │  │
│  │  Services/                                           │  │
│  │  ├── IMessageService                                 │  │
│  │  └── MessageService     ← Business logic             │  │
│  │                                                      │  │
│  │  Data/                                               │  │
│  │  ├── AppDbContext        ← EF Core DbContext         │  │
│  │  └── Migrations/                                     │  │
│  │                                                      │  │
│  │  Models/                                             │  │
│  │  ├── Message             ← Chat entity               │  │
│  │  └── ApplicationUser    ← Identity user             │  │
│  │                                                      │  │
│  │  Views/                                              │  │
│  │  ├── Account/           ← Login, Register, 2FA UI   │  │
│  │  ├── Chat/              ← Chatroom view              │  │
│  │  └── Admin/             ← Admin dashboard           │  │
│  │                                                      │  │
│  │  wwwroot/                                            │  │
│  │  └── vue-chat/          ← Vue 3 chat component       │  │
│  │      (gebundeld via Vite)                            │  │
│  └──────────────────────────────────────────────────────┘  │
│                          │                                  │
│                    ┌─────▼──────┐                           │
│                    │  SQLite    │                           │
│                    │  chat.db   │                           │
│                    └────────────┘                           │
└────────────────────────────────────────────────────────────┘
```

---

## Databasisschema (EF Core entities)

### ApplicationUser (extends IdentityUser)
```
ApplicationUser
├── Id          (string, GUID, PK)
├── UserName    (string)
├── Email       (string)
├── DisplayName (string) ← extra veld
└── [alle Identity velden: PasswordHash, TwoFactorEnabled, etc.]
```

### Message
```
Message
├── Id          (int, PK, auto-increment)
├── Content     (string, max 1000 tekens)
├── CreatedAt   (DateTime, UTC)
├── SenderId    (string, FK → ApplicationUser.Id)
└── Sender      (navigation property)
```

---

## Technologiekeuzes & Motivatie

| Keuze | Alternatief | Waarom deze keuze |
|-------|------------|-------------------|
| ASP.NET Core Identity | Custom auth / JWT | Ingebouwd, veilig, ondersteunt 2FA/TOTP out-of-the-box |
| SQLite + EF Core | SQL Server, Postgres | Geen extra installatie, portable, voldoende voor showcase |
| SignalR | WebSocket handmatig, polling | Ingebouwd in .NET, simpel, automatic fallback, voldoet aan US5 |
| Vue 3 (Vite) | React, Svelte, web components | Vite al aanwezig in project, Vue heeft lage leercurve |
| xUnit | NUnit, MSTest | Standaard in .NET ecosystem, goede tooling |
| Bootstrap 5 | Tailwind, custom CSS | Consistent met UC1/UC2, weinig extra werk |

---

## Layered Architecture (UC3)

```
┌─────────────────────────────────┐
│         Presentation Layer       │  Razor Views + Vue component
├─────────────────────────────────┤
│         Controllers / Hubs       │  AccountController, ChatController,
│                                 │  AdminController, ChatHub
├─────────────────────────────────┤
│         Service Layer            │  MessageService, IMessageService
├─────────────────────────────────┤
│         Data Access Layer        │  AppDbContext, EF Core repositories
├─────────────────────────────────┤
│         Database                 │  SQLite (chat.db)
└─────────────────────────────────┘
```

---

## Autorisatiematrix

| Actie | Anoniem | User | Admin |
|-------|---------|------|-------|
| Profielpagina bekijken | ✓ | ✓ | ✓ |
| Contactformulier invullen | ✓ | ✓ | ✓ |
| Registreren | ✓ | - | - |
| Inloggen | ✓ | - | - |
| Eigen berichten lezen | ✗ | ✓ | ✓ |
| Eigen bericht sturen | ✗ | ✓ | ✓ |
| Alle berichten lezen | ✗ | ✗ | ✓ |
| Filteren op gebruiker | ✗ | ✗ | ✓ |
| Gebruikersbeheer | ✗ | ✗ | ✓ |

---

## .NET Project Structuur UC3

```
UC3-Chatsysteem/
├── Showcase-Chat/
│   ├── Controllers/
│   │   ├── AccountController.cs
│   │   ├── ChatController.cs
│   │   └── AdminController.cs
│   ├── Hubs/
│   │   └── ChatHub.cs
│   ├── Models/
│   │   ├── ApplicationUser.cs
│   │   ├── Message.cs
│   │   └── ErrorViewModel.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Services/
│   │   ├── IMessageService.cs
│   │   └── MessageService.cs
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   └── ChatViewModel.cs
│   ├── Views/
│   │   ├── Account/
│   │   ├── Chat/
│   │   ├── Admin/
│   │   └── Shared/
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── vue-chat/ (Vite build output)
│   ├── Program.cs
│   ├── appsettings.json
│   └── Showcase-Chat.csproj
└── ShowcaseChat.Tests/
    ├── MessageServiceTests.cs
    └── ShowcaseChat.Tests.csproj
```
