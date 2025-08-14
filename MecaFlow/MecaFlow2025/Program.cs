
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Middleware;
using MecaFlow2025.Services;
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

// 4) Registrar servicios
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // ← AGREGAR ESTA LÍNEA

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ----- Localizaci�n: es-CR (Col�n costarricense) -----
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

// 5) Usar sesiones
app.UseSession();

// 6) Agregar middleware de autorización personalizado
app.UseSession();

// Middleware de autorizaci�n por roles (tu custom)
app.UseMiddleware<RoleAuthorizationMiddleware>();

app.UseAuthorization();

// 7) Mapear controladores MVC - inicia en Register
// Ruta por defecto (como la ten�as)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Register}/{id?}"
);

app.Run();