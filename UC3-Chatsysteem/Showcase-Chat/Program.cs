using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Showcase_Chat.Data;
using Showcase_Chat.Hubs;
using Showcase_Chat.Models;
using Showcase_Chat.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Database & Identity ─────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=showcase-chat.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Wachtwoordbeleid (ASVS V2.1)
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        // Account lockout na 5 mislukte pogingen (ASVS V2.2)
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        // E-mailbevestiging niet verplicht (maar 2FA wel beschikbaar)
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); // Nodig voor 2FA TOTP

// ─── Services ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IMessageService, MessageService>();

// ─── SignalR ─────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ─── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ─── Security: Cookie-configuratie (ASVS V3) ─────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;       // Voorkomt JS-toegang tot cookie (XSS-bescherming)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Alleen via HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

var app = builder.Build();

// ─── Middleware pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS: verplicht HTTPS voor 1 jaar (ASVS V9.1)
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Security headers (ASVS V14.4)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    // CSP: staat alleen eigen origin toe; unsafe-inline nodig voor SignalR JS
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; connect-src 'self' ws: wss:");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// SignalR Hub endpoint
app.MapHub<ChatHub>("/chatHub");

// ─── Database migratie + seeding ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Voer automatisch migraties uit bij opstarten
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await DbSeeder.SeedAsync(roleManager, userManager, config, logger);
}

app.Run();
