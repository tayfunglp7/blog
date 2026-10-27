using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;      // ⭐ yeni
using Microsoft.AspNetCore.Authorization;               // ⭐ yeni
using Microsoft.AspNetCore.Mvc.Authorization;           // ⭐ yeni
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Türkçe kültür (Modül 3) ─────────────────────────────────
var kultur = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = kultur;
CultureInfo.DefaultThreadCurrentUICulture = kultur;

// ── Site ayarları (Modül 3) ─────────────────────────────────
builder.Services.AddSingleton(
    builder.Configuration.GetSection("Site").Get<SiteAyarlari>()
    ?? new SiteAyarlari());

// ── MVC + global "giriş zorunlu" ⭐ ─────────────────────────
//
// ⭐ Varsayılan KAPALI. Genel sayfalar GenelController'dan
//    miras alarak [AllowAnonymous] kazanıyor.
builder.Services.AddControllersWithViews(secenekler =>
{
    var politika = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    secenekler.Filters.Add(new AuthorizeFilter(politika));
})
.AddRazorOptions(secenekler =>
{
    // Yönetim view'ları Views/Yonetim/ altında
    secenekler.ViewLocationFormats.Add("/Views/Yonetim/{1}/{0}.cshtml");
    secenekler.ViewLocationFormats.Add("/Views/Yonetim/Shared/{0}.cshtml");
});

// ── Çerez tabanlı kimlik doğrulama ──────────────────────────
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(secenekler =>
    {
        secenekler.LoginPath = "/giris";
        secenekler.LogoutPath = "/cikis";
        secenekler.ReturnUrlParameter = "donusUrl";   // controller parametresiyle aynı!
        secenekler.ExpireTimeSpan = TimeSpan.FromDays(14);
        secenekler.SlidingExpiration = true;
        secenekler.Cookie.HttpOnly = true;            // JS erişemez → XSS koruması
        secenekler.Cookie.SameSite = SameSiteMode.Lax;// CSRF koruması
        secenekler.Cookie.Name = "blog.Oturum";
    });

// ── EF Core ─────────────────────────────────────────────────
builder.Services.AddDbContext<BlogDbContext>(secenekler =>
{
    secenekler.UseSqlServer(
        builder.Configuration.GetConnectionString("BlogDb"));

    if (builder.Environment.IsDevelopment())
    {
        secenekler.LogTo(Console.WriteLine, LogLevel.Information);
        secenekler.EnableSensitiveDataLogging();
    }
});

builder.Services.AddScoped<GorselYardimcisi>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/hata");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// ⭐ 404 ve diğer durum kodlarını kendi sayfamıza yönlendir
app.UseStatusCodePagesWithReExecute("/hata/{0}");
app.UseRouting();

// ⭐⭐ SIRA KRİTİK
app.UseAuthentication();   // ÖNCE: "sen kimsin?"  (çerezi okur)
app.UseAuthorization();    // SONRA: "girebilir mi?" (filtreyi uygular)

// ── Yönlendirme ─────────────────────────────────────────────
//
// ⭐ Öznitelik tabanlı rotalar ([Route], [HttpGet("/giris")])
//    bu kuraldan ÖNCE değerlendirilir.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "sayfa",
    pattern: "{slug}",
    defaults: new { controller = "Sayfa", action = "Goster" });

app.Run();