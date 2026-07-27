using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar servicios del contenedor de controladores con vistas
builder.Services.AddControllersWithViews();

// 2. REGISTRO CRÍTICO: Inyectar HttpClientFactory para la comunicación de red entre nodos
builder.Services.AddHttpClient();
builder.Services.AddScoped<OrbitNet.Services.DistributedRoutingService>();

// 3. Configurar Localización (i18n) soportando Español e Inglés sin usar diccionarios externos
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// 4. Configurar e Instanciar el Middleware de Internacionalización
var supportedCultures = new[] { new CultureInfo("es"), new CultureInfo("en") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();