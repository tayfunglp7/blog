using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;

namespace blog.Controllers;

[Route("yonetim/mesaj")]
public class MesajYonetimController : YonetimControllerTabani
{
    private readonly BlogDbContext _db;

    public MesajYonetimController(BlogDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════════════
    //  LİSTE
    // ══════════════════════════════════════════════════════
    [HttpGet("")]
    public async Task<IActionResult> Index(bool sadeceOkunmamis = false)
    {
        ViewData["Title"] = "Mesajlar";

        IQueryable<Models.Mesaj> sorgu = _db.Mesajlar.AsNoTracking();

        if (sadeceOkunmamis)
            sorgu = sorgu.Where(m => !m.OkunduMu);

        var mesajlar = await sorgu
            // ⭐ Okunmamışlar üstte
            .OrderBy(m => m.OkunduMu)
            .ThenByDescending(m => m.CreatedDate)
            .ToListAsync();

        ViewBag.SadeceOkunmamis = sadeceOkunmamis;
        ViewBag.OkunmamisSayi = await _db.Mesajlar.CountAsync(m => !m.OkunduMu);

        return View(mesajlar);
    }

    // ══════════════════════════════════════════════════════
    //  DETAY  — açınca okundu say
    // ══════════════════════════════════════════════════════
    [HttpGet("{id:long}")]
    public async Task<IActionResult> Detay(long id)
    {
        var mesaj = await _db.Mesajlar.FindAsync(id);

        if (mesaj == null)
            return NotFound();

        // ⭐ Açıldıysa okunmuş say
        if (!mesaj.OkunduMu)
        {
            mesaj.OkunduMu = true;
            await _db.SaveChangesAsync();
        }

        ViewData["Title"] = mesaj.Konu;
        return View(mesaj);
    }

    // ══════════════════════════════════════════════════════
    //  OKUNMADI İŞARETLE
    // ══════════════════════════════════════════════════════
    [HttpPost("okunmadi/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Okunmadi(long id)
    {
        await _db.Mesajlar
            .Where(m => m.MesajId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.OkunduMu, false));

        TempData["Basarili"] = "Mesaj okunmadı olarak işaretlendi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  SİL
    // ══════════════════════════════════════════════════════
    [HttpPost("sil/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sil(long id)
    {
        var mesaj = await _db.Mesajlar.FindAsync(id);

        if (mesaj == null)
            return NotFound();

        mesaj.AktifMi = false;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Mesaj silindi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  TÜMÜNÜ OKUNDU YAP
    // ══════════════════════════════════════════════════════
    [HttpPost("tumunu-okundu")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TumunuOkundu()
    {
        // ⭐ ExecuteUpdate: tek sorguda hepsini güncelle.
        //    Klasik yolla 50 mesaj = 1 SELECT + 50 UPDATE olurdu.
        int adet = await _db.Mesajlar
            .Where(m => !m.OkunduMu)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.OkunduMu, true));

        TempData["Basarili"] = $"{adet} mesaj okundu olarak işaretlendi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /yonetim/mesaj/aboneler
    [HttpGet("aboneler")]
    public async Task<IActionResult> Aboneler()
    {
        ViewData["Title"] = "Aboneler";

        var liste = await _db.Aboneler
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();

        return View(liste);
    }

    // POST: /yonetim/mesaj/abone-cikar/5
    [HttpPost("abone-cikar/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AboneCikar(long id)
    {
        // Soft delete — kayıt duruyor, AktifMi false oluyor.
        // Böylece aynı kişi tekrar kaydolursa geçmişi kaybolmuyor.
        var abone = await _db.Aboneler.FindAsync(id);
        if (abone == null) return NotFound();

        abone.AktifMi = false;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = $"{abone.Eposta} listeden çıkarıldı.";
        return RedirectToAction(nameof(Aboneler));
    }
}