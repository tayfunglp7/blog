using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

[Route("yazi")]
public class YaziController : GenelController
{
    private readonly BlogDbContext _db;

    public YaziController(BlogDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════════════
    //  YAZI DETAYI
    //  GET: /yazi/merhaba-dunya
    // ══════════════════════════════════════════════════════
    [HttpGet("{slug}")]
    public async Task<IActionResult> Detay(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var yazi = await _db.Yazilar
            .AsNoTracking()
            .Include(y => y.Etiketler)
            .FirstOrDefaultAsync(y => y.Slug == slug
                                   && y.Durum == YaziDurum.Yayinda);

        // ⚠️ Taslak yazının adresini bilen biri de 404 almalı.
        //    Yukarıdaki Durum koşulu bunu sağlıyor.
        if (yazi == null)
            return NotFound();

        // ── Okunma sayacı ───────────────────────────────────
        //
        // ⭐ ExecuteUpdateAsync: nesneyi çekmeden tek sorguda artır.
        //    AsNoTracking ile çektiğimiz için zaten güncelleyemezdik.
        //
        // ⚠️ Bu basit bir sayaç. Aynı kişi sayfayı 10 kez
        //    yenilerse 10 sayılır. Gerçek analitik için
        //    oturum/IP takibi gerekir — bu dersin kapsamı dışında.
        await _db.Yazilar
            .Where(y => y.YaziId == yazi.YaziId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                y => y.OkunmaSayisi, y => y.OkunmaSayisi + 1));

        // ── Önceki / sonraki yazı ───────────────────────────
        //
        // Okuyucu yazıyı bitirince ne yapsın? Gezinmesi kolay olsun.
        ViewBag.OncekiYazi = await _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda
                     && y.YayinTarihi < yazi.YayinTarihi)
            .OrderByDescending(y => y.YayinTarihi)
            .Select(y => new { y.Baslik, y.Slug })
            .FirstOrDefaultAsync();

        ViewBag.SonrakiYazi = await _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda
                     && y.YayinTarihi > yazi.YayinTarihi)
            .OrderBy(y => y.YayinTarihi)
            .Select(y => new { y.Baslik, y.Slug })
            .FirstOrDefaultAsync();

        // ── Sayfa başlığı ve açıklaması ─────────────────────
        ViewData["Title"] = yazi.Baslik;
        ViewData["Aciklama"] = MarkdownYardimcisi.DuzMetin(
            yazi.Ozet ?? yazi.Icerik, 160);

        return View(yazi);
    }

    // ══════════════════════════════════════════════════════
    //  ETİKETE GÖRE YAZILAR
    //  GET: /etiket/veritabani
    //
    //  ⭐ Baştaki "/" ile controller'ın "yazi" önekini deliyoruz.
    // ══════════════════════════════════════════════════════
    [HttpGet("/etiket/{slug}")]
    public async Task<IActionResult> Etiket(string slug)
    {
        var etiket = await _db.Etiketler
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Slug == slug);

        if (etiket == null)
            return NotFound();

        var yazilar = await _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda
                     && y.Etiketler.Any(e => e.EtiketId == etiket.EtiketId))
            .Include(y => y.Etiketler)
            .OrderByDescending(y => y.YayinTarihi)
            .ToListAsync();

        ViewData["Title"] = etiket.Ad;
        ViewData["Aciklama"] = $"\"{etiket.Ad}\" etiketli yazılar.";
        ViewBag.EtiketAd = etiket.Ad;

        return View(yazilar);
    }

    // ══════════════════════════════════════════════════════
    //  TÜM ETİKETLER
    //  GET: /etiketler
    // ══════════════════════════════════════════════════════
    [HttpGet("/etiketler")]
    public async Task<IActionResult> Etiketler()
    {
        // ⭐ Sadece yayında yazısı OLAN etiketleri göster.
        //    Boş etiket göstermek okuyucuyu yanıltır.
        var etiketler = await _db.Etiketler
            .AsNoTracking()
            .Select(e => new
            {
                e.Ad,
                e.Slug,
                YaziSayisi = e.Yazilar.Count(y => y.Durum == YaziDurum.Yayinda
                                               && y.AktifMi)
            })
            .Where(x => x.YaziSayisi > 0)
            .OrderByDescending(x => x.YaziSayisi)
            .ThenBy(x => x.Ad)
            .ToListAsync();

        ViewData["Title"] = "Etiketler";
        ViewBag.Etiketler = etiketler;

        return View();
    }
}