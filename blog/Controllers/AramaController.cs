using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

public class AramaController : GenelController
{
    private readonly BlogDbContext _db;

    public AramaController(BlogDbContext db)
    {
        _db = db;
    }

    // GET: /ara?q=veritabani
    [Route("/ara")]
    public async Task<IActionResult> Index(string? q)
    {
        ViewData["Title"] = string.IsNullOrWhiteSpace(q) ? "Ara" : $"\"{q}\" araması";
        ViewBag.Sorgu = q;

        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            ViewBag.Uyari = q != null ? "En az 2 karakter yazın." : null;
            return View(new List<Yazi>());
        }

        string temiz = q.Trim();

        var sonuclar = await _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda
                     && (y.Baslik.Contains(temiz)
                      || y.Icerik.Contains(temiz)
                      || (y.Ozet != null && y.Ozet.Contains(temiz))))
            .Include(y => y.Etiketler)
            // ⭐ Başlıkta geçenler önce — daha alakalı
            .OrderByDescending(y => y.Baslik.Contains(temiz))
            .ThenByDescending(y => y.YayinTarihi)
            .Take(50)
            .ToListAsync();

        return View(sonuclar);
    }
}