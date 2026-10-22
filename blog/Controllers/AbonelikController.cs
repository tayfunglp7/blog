using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;
using System.ComponentModel.DataAnnotations;

namespace blog.Controllers;

public class AbonelikController : GenelController
{
    private readonly BlogDbContext _db;

    public AbonelikController(BlogDbContext db)
    {
        _db = db;
    }

    // POST: /abonelik/kaydol
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kaydol(string? eposta)
    {
        // ── 1) Biçim kontrolü ──
        //
        // ⚠️ ViewModel kullanmıyoruz çünkü tek alan var.
        //    Ama doğrulamayı atlamıyoruz — elle yapıyoruz.
        if (string.IsNullOrWhiteSpace(eposta) ||
            !new EmailAddressAttribute().IsValid(eposta))
        {
            TempData["AboneSonuc"] = "Geçerli bir e-posta adresi girin.";
            return GeriDon();
        }

        // ⭐ Normalleştirme: boşlukları at, küçük harfe çevir.
        //    "  Ali@Site.COM " ile "ali@site.com" aynı kişi.
        //
        //    ⚠️ ToLowerInvariant — ToLower() DEĞİL.
        //    Türkçe kültürde "ALI@x.com".ToLower() → "alı@x.com"
        //    olurdu ve adres bozulurdu. Modül 2'deki slug tuzağının
        //    aynısı, bu sefer e-postada.
        string temiz = eposta.Trim().ToLowerInvariant();

        if (temiz.Length > 255)
        {
            TempData["AboneSonuc"] = "E-posta adresi çok uzun.";
            return GeriDon();
        }

        // ── 2) Zaten kayıtlı mı? ──
        //
        // ⚠️ Pasife alınmışları da arıyoruz (IgnoreQueryFilters).
        //    Yoksa daha önce abonelikten çıkmış biri tekrar
        //    kaydolmak istediğinde veritabanı kısıtına takılır
        //    ve kullanıcı anlamsız bir hata görür.
        var mevcut = await _db.Aboneler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Eposta == temiz);

        if (mevcut != null)
        {
            if (!mevcut.AktifMi)
            {
                // Geri dönen abone — kaydı yeniden etkinleştir
                mevcut.AktifMi = true;
                mevcut.CreatedDate = DateTime.Now;
                await _db.SaveChangesAsync();
            }

            // ⭐ Zaten kayıtlı olana da AYNI mesajı veriyoruz.
            //    Farklı mesaj verseydik, bir saldırgan formu
            //    deneyerek "bu adres abone mi?" sorusunu
            //    cevaplayabilirdi. Modül 4'teki kullanıcı
            //    sayımı (user enumeration) probleminin aynısı.
            TempData["AboneSonuc"] = "Teşekkürler! Kaydınız alındı.";
            return GeriDon();
        }

        // ── 3) Kaydet ──
        try
        {
            _db.Aboneler.Add(new Abone { Eposta = temiz });
            await _db.SaveChangesAsync();
            TempData["AboneSonuc"] = "Teşekkürler! Kaydınız alındı.";
        }
        catch (DbUpdateException)
        {
            // ⭐ Yarış koşulu: iki kişi aynı anda aynı adresi
            //    gönderdi, ikisinin de kontrolü "yok" dedi.
            //    Veritabanındaki benzersiz indeks ikincisini
            //    reddetti. Kullanıcı açısından sonuç aynı.
            TempData["AboneSonuc"] = "Teşekkürler! Kaydınız alındı.";
        }

        return GeriDon();
    }

    /// <summary>
    /// Formun gönderildiği sayfaya geri döner.
    /// Yan sütun her sayfada olduğu için sabit bir adrese
    /// dönemeyiz — kullanıcıyı okuduğu yazıdan koparırdık.
    /// </summary>
    private IActionResult GeriDon()
    {
        string? geldigiYer = Request.Headers.Referer.ToString();

        // ⚠️ Referer başlığı kullanıcıdan gelir, yani GÜVENİLMEZ.
        //    Url.IsLocalUrl olmadan açık yönlendirme (open redirect)
        //    açığı olurdu: saldırgan sahte bir Referer ile
        //    ziyaretçiyi kendi sitesine yollayabilirdi.
        //    Modül 4'te giriş dönüşünde aynı kontrolü yapmıştık.
        if (!string.IsNullOrEmpty(geldigiYer) && Url.IsLocalUrl(geldigiYer))
            return Redirect(geldigiYer);

        return RedirectToAction("Index", "Home");
    }
}