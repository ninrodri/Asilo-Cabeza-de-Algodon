using Asilo.Web.Data;
using Asilo.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ReciboPdfService>();

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
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

//using (var scope = app.Services.CreateScope())
//{
  //  var context = scope.ServiceProvider
    //    .GetRequiredService<AsiloDbContext>();

    //await DbSeeder.RecrearDatosDemoAsync(context);
//}

app.Run();
