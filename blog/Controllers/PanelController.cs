using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

// ⭐ Taban sınıf [Authorize]'ı getiriyor. [Route] burada eziliyor:
//    /yonetim/panel değil, sadece /yonetim olsun istiyoruz.
[Route("yonetim")]
public class PanelController : YonetimControllerTabani
{
    private readonly BlogDbContext _db;

    public PanelController(BlogDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Panel";

        // ⭐ Altı sayıyı TEK sorguda al.
        //    GroupBy(x => 1) numarası: hepsini tek gruba toplayıp
        //    grup üzerinde koşullu sayımlar yapıyoruz.
        var ozet = await _db.Yazilar
            .GroupBy(y => 1)
            .Select(g => new
            {
                Toplam  = g.Count(),
                Yayinda = g.Count(y => y.Durum == YaziDurum.Yayinda),
                Taslak  = g.Count(y => y.Durum == YaziDurum.Taslak),
                Okunma  = g.Sum(y => y.OkunmaSayisi)
            })
            .FirstOrDefaultAsync();

        // ⚠️ Hiç yazı yoksa GroupBy hiç grup üretmez → null döner
        ViewBag.ToplamYazi   = ozet?.Toplam  ?? 0;
        ViewBag.YayindaYazi  = ozet?.Yayinda ?? 0;
        ViewBag.TaslakYazi   = ozet?.Taslak  ?? 0;
        ViewBag.ToplamOkunma = ozet?.Okunma  ?? 0;

        ViewBag.OkunmamisMesaj = await _db.Mesajlar.CountAsync(m => !m.OkunduMu);
        ViewBag.ToplamSayfa    = await _db.Sayfalar.CountAsync();

        // Son düzenlenen 5 yazı
        ViewBag.SonYazilar = await _db.Yazilar
            .AsNoTracking()
            .OrderByDescending(y => y.UpdatedDate ?? y.CreatedDate)
            .Take(5)
            .Select(y => new
            {
                y.YaziId,
                y.Baslik,
                y.Slug,
                y.Durum,
                Tarih = y.UpdatedDate ?? y.CreatedDate
            })
            .ToListAsync();

        return View();
    }
}