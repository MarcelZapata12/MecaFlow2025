using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Middleware;
using QuestPDF.Infrastructure;

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

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ----- Localización: es-CR (Colón costarricense) -----
var cr = new CultureInfo("es-CR");
CultureInfo.DefaultThreadCurrentCulture = cr;
CultureInfo.DefaultThreadCurrentUICulture = cr;

var locOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es-CR")
    .AddSupportedCultures("es-CR")
    .AddSupportedUICultures("es-CR");

app.UseRequestLocalization(locOptions);
// ------------------------------------------------------

app.UseRouting();

app.UseSession();

// Middleware de autorización por roles (tu custom)
app.UseMiddleware<RoleAuthorizationMiddleware>();

app.UseAuthorization();

// Ruta por defecto (como la tenías)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Register}/{id?}"
);

app.Run();