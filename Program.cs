using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using caso_de_uso_6_ejercer_turno.Data;
using Microsoft.EntityFrameworkCore;
using caso_de_uso_6_ejercer_turno.Services;
using caso_de_uso_6_ejercer_turno.Events;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddHostedService<GameOrchestratorHostedService>();
builder.Services.AddScoped<UsuarioRegistradoEventHandler>();
builder.Services.AddScoped<CuentaService>();

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

app.Run();
        