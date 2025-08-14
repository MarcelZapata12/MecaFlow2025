using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Middleware;
using MecaFlow2025.Services;
using QuestPDF.Infrastructure;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF: licencia Community
QuestPDF.Settings.License = LicenseType.Community;

// MVC
builder.Services.AddControllersWithViews();

// Sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DbContext (connection string en appsettings.json)
builder.Services.AddDbContext<MecaFlowContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MecaFlowConnection"))
);

// Servicios
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ----- Localización: es-CR -----
var cr = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = cr;
CultureInfo.DefaultThreadCurrentUICulture = cr;

var locOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es-CR")
    .AddSupportedCultures("es-CR")
    .AddSupportedUICultures("es-CR");

app.UseRequestLocalization(locOptions);
// --------------------------------

app.UseRouting();

app.UseSession(); // ← una sola vez

// Middleware de autorización por roles (custom)
app.UseMiddleware<RoleAuthorizationMiddleware>();

app.UseAuthorization();

// Rutas: raíz y default -> Auth/Login
app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Auth", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
