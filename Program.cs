using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using caso_de_uso_6_ejercer_turno.Services;
using caso_de_uso_6_ejercer_turno.Events;

var builder = WebApplication.CreateBuilder(args);

// ============================================
//        SERVICIOS
// ============================================
builder.Services.AddControllersWithViews();

// Agrega sesiones
builder.Services.AddSession();

// servicios personalizados
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddSingleton<TurnManager>();
builder.Services.AddHostedService<GameOrchestratorHostedService>();
builder.Services.AddScoped<UsuarioRegistradoEventHandler>();
builder.Services.AddScoped<CuentaService>();

var app = builder.Build();

// ============================================
//        MIDDLEWARE
// ============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

// ============================================
//        REDIRECCIÓN RAÍZ
// ============================================
app.MapGet("/", context =>
{
    context.Response.Redirect("/Cuenta/Login");
    return Task.CompletedTask;
});

// ============================================
//        RUTEO
// ============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cuenta}/{action=Login}/{id?}");

app.Run();
.