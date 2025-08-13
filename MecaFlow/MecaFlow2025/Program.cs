
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Middleware;
using MecaFlow2025.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configurar QuestPDF para usar la licencia Community (gratis para proyectos educativos)
QuestPDF.Settings.License = LicenseType.Community;

// 1) Agrega MVC
builder.Services.AddControllersWithViews();

// 2) Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3) Registra tu DbContext con la cadena de conexión de appsettings.json
builder.Services.AddDbContext<MecaFlowContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MecaFlowConnection")
    )
);

// 4) Registrar servicios
builder.Services.AddScoped<ChatbotService>();
builder.Services.AddScoped<IEmailService, EmailService>(); // ← AGREGAR ESTA LÍNEA

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 5) Usar sesiones
app.UseSession();

// 6) Agregar middleware de autorización personalizado
app.UseMiddleware<RoleAuthorizationMiddleware>();

app.UseAuthorization();

// 7) Mapear controladores MVC - inicia en Register
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Register}/{id?}");

app.Run();