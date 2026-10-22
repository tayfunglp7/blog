using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Models;
using blog.Data;


namespace blog.Controllers;

public class HomeController : GenelController
{
   private readonly BlogDbContext _db;

    // Sayfa başına yazı sayısı
    private const int SayfaBoyutu = 5;

    public HomeController(BlogDbContext db)
    {
        _db = db;
    }

    // GET: /  veya  /?sayfa=2
    public async Task<IActionResult> Index(int sayfa = 1)
    {
        if (sayfa < 1) sayfa = 1;

        // ⭐ GENEL SİTENİN ALTIN KURALI:
        //    Sadece YAYINDA olanlar görünür.
        //
        //    Query filter AktifMi'yi hallediyor ama durum
        //    filtresini her genel sorguda ELLE yazmamız gerekiyor.
        //
        //    ⚠️ Bunu unutmak = taslakların siteye sızması.
        IQueryable<Yazi> sorgu = _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda)
            .Include(y => y.Etiketler);

        int toplam = await sorgu.CountAsync();

        var yazilar = await sorgu
            .OrderByDescending(y => y.YayinTarihi)
            .Skip((sayfa - 1) * SayfaBoyutu)
            .Take(SayfaBoyutu)
            .ToListAsync();

        ViewBag.Sayfa = sayfa;
        ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / SayfaBoyutu);
        ViewBag.ToplamYazi = toplam;

        return View(yazilar);
    }
}
