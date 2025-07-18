using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Middleware; // ← AGREGAR ESTE USING

var builder = WebApplication.CreateBuilder(args);

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

// 4) Usar sesiones
app.UseSession();

// 5) Agregar middleware de autorización personalizado
app.UseMiddleware<RoleAuthorizationMiddleware>(); // ← AGREGAR ESTA LÍNEA

app.UseAuthorization();

// 6) Mapea tus controladores MVC - inicia en Register
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Register}/{id?}");

app.Run();