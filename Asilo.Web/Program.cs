using Asilo.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login/AccesoDenegado";
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AsiloDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AsiloDb")));


builder.Services.AddControllersWithViews();

var urlNotificaciones =
    builder.Configuration["Microservicios:Notificaciones"]
    ?? "http://localhost:5240/";

builder.Services.AddHttpClient<Asilo.Web.Services.NotificacionesClient>(
    client =>
    {
        client.BaseAddress = new Uri(urlNotificaciones);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
