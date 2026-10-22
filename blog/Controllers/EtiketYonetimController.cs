using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

[Route("yonetim/etiket")]
public class EtiketYonetimController : YonetimControllerTabani
{
    private readonly BlogDbContext _db;

    public EtiketYonetimController(BlogDbContext db)
    {
        _db = db;
    }

    private string SlugUret(string ad, long haricId = 0)
    {
        return SlugYardimcisi.BenzersizUret(ad, aday =>
            SlugYardimcisi.SlugYasakliMi(aday)
            || _db.Etiketler
                  .IgnoreQueryFilters()
                  .Any(e => e.Slug == aday && e.EtiketId != haricId));
    }

    // ══════════════════════════════════════════════════════
    //  LİSTE  — yazı sayılarıyla birlikte
    // ══════════════════════════════════════════════════════
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Etiketler";

        // ⭐ Select içinde alt sorgu: Include'a gerek yok.
        //    Include kullansaydık TÜM yazılar belleğe gelirdi;
        //    biz sadece SAYI istiyoruz.
        var etiketler = await _db.Etiketler
            .AsNoTracking()
            .Select(e => new EtiketSatir
            {
                EtiketId     = e.EtiketId,
                Ad           = e.Ad,
                Slug         = e.Slug,
                ToplamYazi   = e.Yazilar.Count(y => y.AktifMi),
                YayindaYazi  = e.Yazilar.Count(y => y.AktifMi
                                                 && y.Durum == YaziDurum.Yayinda)
            })
            .OrderByDescending(x => x.ToplamYazi)
            .ThenBy(x => x.Ad)
            .ToListAsync();

        return View(etiketler);
    }

    // ══════════════════════════════════════════════════════
    //  EKLE  — liste sayfasındaki satır içi formdan
    // ══════════════════════════════════════════════════════
    [HttpPost("ekle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(string ad)
    {
        if (string.IsNullOrWhiteSpace(ad))
        {
            TempData["Uyari"] = "Etiket adı boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        ad = ad.Trim();

        bool varMi = await _db.Etiketler
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Ad == ad);

        if (varMi)
        {
            TempData["Uyari"] = $"\"{ad}\" etiketi zaten var.";
            return RedirectToAction(nameof(Index));
        }

        _db.Etiketler.Add(new Etiket
        {
            Ad = ad,
            Slug = SlugUret(ad),
            AktifMi = true
        });

        await _db.SaveChangesAsync();

        TempData["Basarili"] = $"\"{ad}\" etiketi eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  YENİDEN ADLANDIR
    // ══════════════════════════════════════════════════════
    [HttpPost("duzenle/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(long id, string ad)
    {
        var etiket = await _db.Etiketler.FindAsync(id);

        if (etiket == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(ad))
        {
            TempData["Uyari"] = "Etiket adı boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        ad = ad.Trim();

        bool varMi = await _db.Etiketler
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Ad == ad && e.EtiketId != id);

        if (varMi)
        {
            TempData["Uyari"] = $"\"{ad}\" adında başka bir etiket var.";
            return RedirectToAction(nameof(Index));
        }

        string eskiSlug = etiket.Slug;
        etiket.Ad = ad;
        etiket.Slug = SlugUret(ad, id);

        await _db.SaveChangesAsync();

        if (eskiSlug != etiket.Slug)
        {
            TempData["Uyari"] = $"Adres değişti: /etiket/{eskiSlug} → /etiket/{etiket.Slug}";
        }

        TempData["Basarili"] = "Etiket güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  SİL
    // ══════════════════════════════════════════════════════
    [HttpPost("sil/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(long id)
    {
        var etiket = await _db.Etiketler
            .Include(e => e.Yazilar)
            .FirstOrDefaultAsync(e => e.EtiketId == id);

        if (etiket == null)
            return NotFound();

        // ⭐ Etiket silinirken yazılarla bağlantısını da kes.
        //    Kesmezsek ara tabloda "hayalet" satırlar kalır:
        //    yazı pasif bir etikete bağlı görünür.
        etiket.Yazilar.Clear();

        etiket.AktifMi = false;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = $"\"{etiket.Ad}\" etiketi silindi.";
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Liste ekranının taşıyıcısı — tablo değil.</summary>
public class EtiketSatir
{
    public long EtiketId { get; set; }
    public string Ad { get; set; } = "";
    public string Slug { get; set; } = "";
    public int ToplamYazi { get; set; }
    public int YayindaYazi { get; set; }
}