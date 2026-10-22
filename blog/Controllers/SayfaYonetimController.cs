using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

[Route("yonetim/sayfa")]
public class SayfaYonetimController : YonetimControllerTabani
{
    private readonly BlogDbContext _db;

    public SayfaYonetimController(BlogDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════════════
    //  YARDIMCI: slug üret ve doğrula
    //
    //  Üç kontrol:
    //    1. Boş mu?
    //    2. Yasaklı mı? (sistem adresi)
    //    3. Başka sayfada var mı?
    // ══════════════════════════════════════════════════════
    private string SlugHazirla(string? istenenSlug, string baslik, long haricId = 0)
    {
        // Kullanıcı slug yazdıysa onu temizle, yazmadıysa başlıktan üret
        string temel = !string.IsNullOrWhiteSpace(istenenSlug)
            ? SlugYardimcisi.Uret(istenenSlug)
            : SlugYardimcisi.Uret(baslik);

        if (string.IsNullOrEmpty(temel))
            temel = "sayfa";

        return SlugYardimcisi.BenzersizUret(temel, aday =>
            SlugYardimcisi.SlugYasakliMi(aday)
            || _db.Sayfalar
                  .IgnoreQueryFilters()
                  .Any(s => s.Slug == aday && s.SayfaId != haricId)
            // ⭐ Yazı slug'larıyla da çakışmasın —
            //    ikisi de farklı seviyede ama karışıklık yaratır
            || _db.Yazilar
                  .IgnoreQueryFilters()
                  .Any(y => y.Slug == aday));
    }

    // ══════════════════════════════════════════════════════
    //  LİSTE
    // ══════════════════════════════════════════════════════
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Sayfalar";

        var sayfalar = await _db.Sayfalar
            .AsNoTracking()
            .OrderBy(s => s.Sira)
            .ThenBy(s => s.Baslik)
            .ToListAsync();

        return View(sayfalar);
    }

    // ══════════════════════════════════════════════════════
    //  OLUŞTUR
    // ══════════════════════════════════════════════════════
    [HttpGet("olustur")]
    public IActionResult Olustur()
    {
        ViewData["Title"] = "Yeni sayfa";
        return View(new Sayfa());
    }

    [HttpPost("olustur")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Olustur(Sayfa sayfa, string? istenenSlug)
    {
        ViewData["Title"] = "Yeni sayfa";

        if (!ModelState.IsValid)
            return View(sayfa);

        sayfa.Slug = SlugHazirla(istenenSlug, sayfa.Baslik);
        sayfa.CreatedDate = DateTime.Now;
        sayfa.UpdatedDate = null;
        sayfa.AktifMi = true;

        _db.Sayfalar.Add(sayfa);
        await _db.SaveChangesAsync();

        TempData["Basarili"] = $"\"{sayfa.Baslik}\" sayfası oluşturuldu.";
        return RedirectToAction(nameof(Duzenle), new { id = sayfa.SayfaId });
    }

    // ══════════════════════════════════════════════════════
    //  DÜZENLE
    // ══════════════════════════════════════════════════════
    [HttpGet("duzenle/{id:long}")]
    public async Task<IActionResult> Duzenle(long id)
    {
        var sayfa = await _db.Sayfalar.FindAsync(id);

        if (sayfa == null)
            return NotFound();

        ViewData["Title"] = "Sayfayı düzenle";
        return View(sayfa);
    }

    [HttpPost("duzenle/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(long id, Sayfa gelen, string? istenenSlug)
    {
        ViewData["Title"] = "Sayfayı düzenle";

        if (!ModelState.IsValid)
            return View(gelen);

        var mevcut = await _db.Sayfalar.FindAsync(id);

        if (mevcut == null)
            return NotFound();

        mevcut.Baslik       = gelen.Baslik;
        mevcut.Icerik       = gelen.Icerik;
        mevcut.MenudeGoster = gelen.MenudeGoster;
        mevcut.Sira         = gelen.Sira;
        mevcut.UpdatedDate  = DateTime.Now;

        // ⭐ Yazılardan FARKLI: sayfa slug'ı değiştirilebilir.
        //    Sayfalar nadiren paylaşılır ve sayıları azdır.
        //    Kullanıcı açıkça yeni bir slug yazarsa güncelliyoruz.
        if (!string.IsNullOrWhiteSpace(istenenSlug))
        {
            string yeni = SlugHazirla(istenenSlug, gelen.Baslik, id);

            if (yeni != mevcut.Slug)
            {
                TempData["Uyari"] = $"Adres değişti: /{mevcut.Slug} → /{yeni}. " +
                                     "Eski bağlantılar artık çalışmayacak.";
                mevcut.Slug = yeni;
            }
        }

        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Sayfa güncellendi.";
        return RedirectToAction(nameof(Duzenle), new { id });
    }

    // ══════════════════════════════════════════════════════
    //  SİL
    // ══════════════════════════════════════════════════════
    [HttpPost("sil/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(long id)
    {
        var sayfa = await _db.Sayfalar.FindAsync(id);

        if (sayfa == null)
            return NotFound();

        sayfa.AktifMi = false;
        sayfa.UpdatedDate = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Sayfa silindi.";
        return RedirectToAction(nameof(Index));
    }
}