using System.Security.Claims;
using HotelManager.Application.Interfaces;
using HotelManager.Infrastructure;
using HotelManager.Web.Components;
using HotelManager.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

var dbPath = builder.Configuration["HotelManager:DbPath"] ?? DbPaths.DefaultDbPath;
builder.Services.AddDbContextFactory<HotelDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.AccessDeniedPath = "/login";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddFoundationServices();
// Child workstreams: register additional services in your own
// Extensions/*ServiceRegistration.cs file and call it here.
builder.Services.AddMasterDataServices();
builder.Services.AddReservationServices();
builder.Services.AddOrderServices();
builder.Services.AddInventoryServices();
builder.Services.AddHrPayrollServices();
builder.Services.AddReportServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/account/login", async (HttpContext http, IAuthService auth) =>
{
    var form = await http.Request.ReadFormAsync();
    var userName = form["username"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith('/')) returnUrl = "/";

    var result = await auth.ValidateCredentialsAsync(userName, password);
    if (!result.Succeeded)
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, result.UserName!),
        new(ClaimTypes.Role, result.UserType ?? "User")
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Redirect(returnUrl);
});

app.MapGet("/account/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<HotelDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await DbSeeder.SeedAsync(db);
}

app.Run();
