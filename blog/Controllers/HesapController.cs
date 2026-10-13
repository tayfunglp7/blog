using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

[AllowAnonymous]
public class HesapController : Controller
{
    private readonly BlogDbContext _db;

    public HesapController(BlogDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════════════
    //  GİRİŞ FORMU
    //  GET: /giris
    // ══════════════════════════════════════════════════════
    [HttpGet("/giris")]
    public IActionResult Giris(string? donusUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Panel");

        ViewData["Title"] = "Giriş";
        ViewBag.DonusUrl = donusUrl;
        return View();
    }

    // ══════════════════════════════════════════════════════
    //  GİRİŞİ İŞLE
    //  POST: /giris
    // ══════════════════════════════════════════════════════
    [HttpPost("/giris")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisViewModel model, string? donusUrl = null)
    {
        ViewData["Title"] = "Giriş";
        ViewBag.DonusUrl = donusUrl;

        if (!ModelState.IsValid)
            return View(model);

        string hash = SifreYardimcisi.Hashle(model.Sifre);

        // Karşılaştırmayı veritabanında yapıyoruz — hash dolaşmıyor.
        // EF değerleri otomatik parametreler, SQL injection yok.
        Kullanici? kullanici = await _db.Kullanicilar
            .FirstOrDefaultAsync(k => k.KullaniciAdi == model.KullaniciAdi
                                   && k.SifreHash == hash);

        if (kullanici == null)
        {
            // ⚠️ "Kullanıcı yok" ile "şifre yanlış"ı AYIRMA.
            //    Ayırsaydık saldırgan kayıtlı kullanıcı adlarını
            //    tek tek deneyerek öğrenirdi (user enumeration).
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        var iddialar = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, kullanici.KullaniciId.ToString()),
            new Claim(ClaimTypes.Name, kullanici.AdSoyad),
            new Claim("KullaniciAdi", kullanici.KullaniciAdi)
        };

        var kimlik = new ClaimsIdentity(iddialar,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(kimlik),
            new AuthenticationProperties
            {
                IsPersistent = model.BeniHatirla,
                AllowRefresh = true
            });

        // ⚠️ Url.IsLocalUrl ŞART — olmadan açık yönlendirme açığı olur.
        //    /giris?donusUrl=https://sahte-site.com
        if (!string.IsNullOrEmpty(donusUrl) && Url.IsLocalUrl(donusUrl))
            return Redirect(donusUrl);

        return RedirectToAction("Index", "Panel");
    }

    // ══════════════════════════════════════════════════════
    //  ÇIKIŞ
    //  POST: /cikis
    //
    //  ⚠️ POST olmalı. GET olsaydı kötü niyetli sitedeki
    //     <img src="...cikis"> etiketi sizi habersizce çıkartırdı.
    // ══════════════════════════════════════════════════════
    [HttpPost("/cikis")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Anasayfa");
    }
}