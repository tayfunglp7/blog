using Microsoft.AspNetCore.Mvc;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

[Route("iletisim")]
public class IletisimController : GenelController
{
    private readonly BlogDbContext _db;
    private readonly ILogger<IletisimController> _gunluk;

    // İnsanın formu doldurması en az bu kadar sürer
    private const int EnAzSaniye = 3;

    public IletisimController(BlogDbContext db, ILogger<IletisimController> gunluk)
    {
        _db = db;
        _gunluk = gunluk;
    }

    // ══════════════════════════════════════════════════════
    //  FORM
    //  GET: /iletisim
    // ══════════════════════════════════════════════════════
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewData["Title"] = "İletişim";

        return View(new IletisimViewModel
        {
            // ⭐ Form açılış zamanını damgala
            AcilisZamani = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    // ══════════════════════════════════════════════════════
    //  GÖNDER
    //  POST: /iletisim
    // ══════════════════════════════════════════════════════
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IletisimViewModel model)
    {
        ViewData["Title"] = "İletişim";

        // ── 1) BAL KÜPÜ ─────────────────────────────────────
        //
        // ⭐ Bota hata GÖSTERME. "Başarılı" de ve kaydetme.
        //    Hata gösterseydik bot neyin yanlış olduğunu öğrenir
        //    ve bir dahakine o alanı boş bırakırdı.
        if (!string.IsNullOrEmpty(model.Website))
        {
            _gunluk.LogWarning("Bal küpü doldu — bot şüphesi. IP: {ip}",
                HttpContext.Connection.RemoteIpAddress);

            return View("Tesekkur");       // sahte başarı
        }

        // ── 2) ZAMAN KONTROLÜ ───────────────────────────────
        long simdi = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long gecenSure = simdi - model.AcilisZamani;

        if (model.AcilisZamani > 0 && gecenSure < EnAzSaniye)
        {
            _gunluk.LogWarning("Çok hızlı gönderim ({sn} sn) — bot şüphesi.", gecenSure);
            return View("Tesekkur");       // sahte başarı
        }

        // ── 3) NORMAL DOĞRULAMA ─────────────────────────────
        if (!ModelState.IsValid)
        {
            // ⚠️ Açılış zamanını yenile — kullanıcı formu düzeltirken
            //    eski damga kalırsa sorun olmaz ama yeni damga daha doğru.
            model.AcilisZamani = simdi;
            return View(model);
        }

        // ── 4) KAYDET ───────────────────────────────────────
        //
        // ⭐ ViewModel'den Mesaj'a ELLE kopyalıyoruz.
        //    Sistem alanlarını (OkunduMu, AktifMi, CreatedDate)
        //    biz dolduruyoruz — kullanıcıdan almıyoruz.
        var mesaj = new Mesaj
        {
            Ad          = model.Ad.Trim(),
            Eposta      = model.Eposta.Trim(),
            Konu        = model.Konu.Trim(),
            Icerik      = model.Icerik.Trim(),
            OkunduMu    = false,
            CreatedDate = DateTime.Now,
            AktifMi     = true
        };

        _db.Mesajlar.Add(mesaj);
        await _db.SaveChangesAsync();

        // POST-Redirect-GET değil, doğrudan görünüm:
        // kullanıcı F5'e basarsa tarayıcı uyarır, çift kayıt olmaz.
        // (Yönlendirme de yapılabilirdi; ikisi de doğru.)
        return View("Tesekkur");
    }
}