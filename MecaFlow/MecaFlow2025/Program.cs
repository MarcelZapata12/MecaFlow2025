using Microsoft.EntityFrameworkCore;                // necesario para UseSqlServer
using MecaFlow2025.Models;                          // tu namespace del Context
using QuestPDF.Infrastructure;                      // <<— IMPORTANTE: agrega esto

var builder = WebApplication.CreateBuilder(args);

// Configurar QuestPDF para usar la licencia Community (gratis para proyectos educativos)
QuestPDF.Settings.License = LicenseType.Community;

// 1) Agrega MVC
builder.Services.AddControllersWithViews();

// 2) Registra tu DbContext con la cadena de conexión de appsettings.json
builder.Services.AddDbContext<MecaFlowContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MecaFlowConnection")
    )
);

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

app.UseAuthorization();

// 3) Mapea tus controladores MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
