using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using caso_de_uso_6_ejercer_turno.Data;
using Microsoft.EntityFrameworkCore;
using caso_de_uso_6_ejercer_turno.Services;
using caso_de_uso_6_ejercer_turno.Events;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");
// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ParchisDB"));
});

// MVC
builder.Services.AddControllersWithViews();

//  Sesion
builder.Services.AddSession();

//  TempData basado en sesion 
builder.Services.AddSingleton<
    Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider,
    Microsoft.AspNetCore.Mvc.ViewFeatures.SessionStateTempDataProvider>();

// Servicios personalizados
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddSingleton<TurnManager>();
// LobbyService allows multiple independent lobbies
builder.Services.AddSingleton<LobbyService>();
builder.Services.AddHostedService<GameOrchestratorHostedService>();
builder.Services.AddScoped<UsuarioRegistradoEventHandler>();
builder.Services.AddScoped<CuentaService>();
builder.Services.AddSingleton<SocketGameService>();
builder.Services.AddSignalR();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

//  autenticacion
app.UseAuthentication();
app.UseAuthorization();

// Redireccion raiz
app.MapGet("/", context =>
{
    context.Response.Redirect("/Cuenta/Login");
    return Task.CompletedTask;
});

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");
app.MapHub<caso_de_uso_6_ejercer_turno.Hubs.LobbyHub>("/lobbyHub");
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        try
        {
            // 1. Buscar la IP local de la máquina (que no sea localhost)
            var ipAddress = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                ?.ToString();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                // 2. Construir la URL con tu IP y el puerto 5000
                string url = $"http://{ipAddress}:5000/Cuenta/Login";

                Console.WriteLine($"? Abriendo navegador en: {url}");

                // 3. Abrir el navegador predeterminado de Windows
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("No se pudo abrir el navegador automáticamente: " + ex.Message);
        }
    });
}

app.Run();