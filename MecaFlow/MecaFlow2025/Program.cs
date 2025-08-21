using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using MecaFlow2025.Middleware;
using MecaFlow2025.Services;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Caching.Memory;  
using Microsoft.AspNetCore.Http;            

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
    // Recomendado para producción:
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// DbContext (connection string en appsettings.json)
builder.Services.AddDbContext<MecaFlowContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MecaFlowConnection"))
);

// Servicios
builder.Services.AddScoped<IEmailService, EmailService>();

// Cache en memoria para throttle por sesión
builder.Services.AddMemoryCache();

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

// Debug: comprobar que la API key está cargada
app.MapGet("/debug/openai", (IConfiguration cfg) =>
{
    var ok = !string.IsNullOrWhiteSpace(cfg["OpenAI:ApiKey"]);
    return Results.Ok(new { configured = ok });
});


// =========== Helper: respuesta de respaldo (modo básico) ===========
static string FallbackReply(string? msg)
{
    var t = (msg ?? string.Empty).ToLowerInvariant();

    if (t.Contains("hora") || t.Contains("abren") || t.Contains("cierran"))
        return "Horario: L–V 7:00–17:00. Sábado y domingo: cerrado.";
    if (t.Contains("tel") || t.Contains("whats"))
        return "Tel/WhatsApp: +506 6052 7657.";
    if (t.Contains("direc") || t.Contains("ubic"))
        return "Dirección: Barrio del Carmen, San José, Costa Rica.";
    if (t.Contains("correo") || t.Contains("email"))
        return "Correo: tallerhiburt@gmail.com";

    return "Estoy en modo básico temporalmente.\n" +
           "• Horario: L–V 7:00–17:00\n" +
           "• Tel/WhatsApp: +506 6052 7657\n" +
           "• Correo: tallerhiburt@gmail.com\n" +
           "• Dirección: Barrio del Carmen, San José, CR\n" +
           "¿En qué te ayudo?";
}
// ================================================================


// ===================== CHATBOT: /api/chat =====================
app.MapPost("/api/chat", async (
    HttpContext http,
    IConfiguration cfg,
    IWebHostEnvironment env,
    IMemoryCache cache // ← para throttle
) =>
{
    // Solo usuarios autenticados (el Layout solo muestra chat con sesión)
    if (http.Session.GetString("UserId") == null)
        return Results.Unauthorized();

    // -------- Throttle sencillo por sesión: 1 request cada 10s --------
    var sessionId = http.Session.Id ?? "anon";
    var throttleKey = $"chat:last:{sessionId}";
    if (cache.TryGetValue<DateTime>(throttleKey, out var lastTs))
    {
        var elapsed = DateTime.UtcNow - lastTs;
        var minDelay = TimeSpan.FromSeconds(10); // ajusta si lo necesitas
        if (elapsed < minDelay)
        {
            var wait = minDelay - elapsed;
            await Task.Delay(wait);
        }
    }
    cache.Set(throttleKey, DateTime.UtcNow, TimeSpan.FromMinutes(30));
    // ------------------------------------------------------------------

    try
    {
        using var reader = new StreamReader(http.Request.Body, Encoding.UTF8);
        var bodyText = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(bodyText))
            return Results.BadRequest(new { error = "Body vacío" });

        using var docIn = JsonDocument.Parse(bodyText);

        if (!docIn.RootElement.TryGetProperty("message", out var msgEl))
            return Results.BadRequest(new { error = "Falta 'message'" });

        var message = msgEl.GetString();
        if (string.IsNullOrWhiteSpace(message))
            return Results.BadRequest(new { error = "'message' vacío" });

        // chatId opcional (no se persiste en servidor)
        string? chatId = null;
        if (docIn.RootElement.TryGetProperty("chatId", out var chatIdEl) && chatIdEl.ValueKind == JsonValueKind.String)
            chatId = chatIdEl.GetString();

        var apiKey = cfg["OpenAI:ApiKey"];
        // === Si NO hay API key -> responder en modo básico (HTTP 200) ===
        if (string.IsNullOrWhiteSpace(apiKey))
            return Results.Ok(new { reply = FallbackReply(message) });

        var systemPrompt = """
        Eres el asistente virtual del Taller MecaFlow.
        Responde en español, breve y claro.
        Si te piden info del taller, utiliza:
        - Horarios: L-V 7:00–17:00, Sáb Cerrado, Domingo cerrado.
        - Teléfono: +506 6052 7657
        - WhatsApp: +506 6052 7657
        - Correo: tallerhiburt@gmail.com   
        - Dirección: Barrio del Carmen, San José, Costa Rica.
        Si la pregunta no es del taller, ayuda de forma general y concisa.
        """;

        // Mensajes: system + (historial opcional) + user
        var msgs = new List<object> { new { role = "system", content = systemPrompt } };

        if (docIn.RootElement.TryGetProperty("history", out var hist) && hist.ValueKind == JsonValueKind.Array)
        {
            foreach (var m in hist.EnumerateArray())
            {
                try
                {
                    var role = m.GetProperty("role").GetString();
                    var content = m.GetProperty("content").GetString();
                    if (!string.IsNullOrWhiteSpace(role) && !string.IsNullOrWhiteSpace(content))
                        msgs.Add(new { role, content });
                }
                catch { /* ignora entradas inválidas */ }
            }
        }

        msgs.Add(new { role = "user", content = message });

        var payload = new
        {
            model = "gpt-4o-mini",
            messages = msgs.ToArray(),
            temperature = 0.3
        };

        using var client = new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // Opcional si usas Projects
        var projectId = cfg["OpenAI:ProjectId"];
        if (!string.IsNullOrWhiteSpace(projectId))
            client.DefaultRequestHeaders.Add("OpenAI-Project", projectId);

        // ---------- Reintentos con backoff para 429/5xx ----------
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using var reqJson = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("chat/completions", reqJson);
            var raw = await resp.Content.ReadAsStringAsync();

            if (resp.IsSuccessStatusCode)
            {
                string reply = "No pude generar respuesta.";
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                        reply = choices[0].GetProperty("message").GetProperty("content").GetString() ?? reply;
                }
                catch { /* deja fallback */ }

                return Results.Ok(new { reply });
            }

            var status = (int)resp.StatusCode;

            // Si es rate limit (429) o >=500: backoff y reintentar
            if (status == 429 || status >= 500)
            {
                var wait = resp.Headers.RetryAfter?.Delta ??
                           TimeSpan.FromSeconds(Math.Pow(2, attempt + 1) * 2); 
                await Task.Delay(wait);
                continue;
            }

            // Otros errores 4xx (p.ej. 400/401) -> responder en modo básico
            return Results.Ok(new { reply = FallbackReply(message) });
        }

        // Si se agotaron los reintentos -> modo básico
        return Results.Ok(new { reply = FallbackReply(message) });
        // ---------------------------------------------------------
    }
    catch (Exception)
    {
        // Cualquier excepción -> modo básico
        return Results.Ok(new { reply = FallbackReply("fallback") });
    }
})
.WithName("ChatApi");


// ===== Reinicio del chat: limpia cualquier estado del lado servidor 
app.MapPost("/api/chat/reset", (HttpContext ctx) =>
{
    // Si en el futuro guardas algo del chat en sesión, límpialo aquí:
    ctx.Session.Remove("ChatHistory");
    ctx.Items.Remove("ChatContext");
    return Results.NoContent();
})
.WithName("ChatResetApi");

// =============================================================

app.Run();
