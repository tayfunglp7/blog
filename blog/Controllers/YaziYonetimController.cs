using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

// ⭐ Taban sınıftaki [Route("yonetim/[controller]")] yerine
//    kendi temiz yolumuzu veriyoruz: /yonetim/yazi
// ⭐ [Authorize] taban sınıftan geliyor — tekrar yazmıyoruz.
[Route("yonetim/yazi")]
public class YaziYonetimController : YonetimControllerTabani
{
    private readonly BlogDbContext _db;
    private readonly GorselYardimcisi _gorsel;

    public YaziYonetimController(BlogDbContext db, GorselYardimcisi gorsel)
    {
        _db = db;
        _gorsel = gorsel;
    }

    // ══════════════════════════════════════════════════════
    //  YARDIMCI: etiket listesi
    // ══════════════════════════════════════════════════════
    private async Task EtiketleriHazirlaAsync()
    {
        ViewBag.TumEtiketler = await _db.Etiketler
            .AsNoTracking()
            .OrderBy(e => e.Ad)
            .ToListAsync();
    }

    // ══════════════════════════════════════════════════════
    //  YARDIMCI: benzersiz slug üret
    //
    //  ⚠️⚠️ IgnoreQueryFilters() ŞART!
    //     Silinmiş (AktifMi = 0) yazılar veritabanında DURUYOR
    //     ve benzersiz indeks onları da sayıyor.
    //     Filtre açık kalsaydı çakışmayı göremez, kayıt
    //     anında DbUpdateException alırdık.
    // ══════════════════════════════════════════════════════
    private string SlugUret(string baslik, long haricTutulanId = 0)
    {
        return SlugYardimcisi.BenzersizUret(baslik, aday =>
            _db.Yazilar
               .IgnoreQueryFilters()
               .Any(y => y.Slug == aday && y.YaziId != haricTutulanId));
    }

    // ══════════════════════════════════════════════════════
    //  1) LİSTE
    //  GET: /yonetim/yazi?durum=Taslak&arama=...
    // ══════════════════════════════════════════════════════
    [HttpGet("")]
    public async Task<IActionResult> Index(YaziDurum? durum, string? arama)
    {
        ViewData["Title"] = "Yazılar";

        IQueryable<Yazi> sorgu = _db.Yazilar
            .AsNoTracking()
            .Include(y => y.Etiketler);

        if (durum.HasValue)
            sorgu = sorgu.Where(y => y.Durum == durum.Value);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            string temiz = arama.Trim();
            sorgu = sorgu.Where(y => y.Baslik.Contains(temiz)
                                  || y.Icerik.Contains(temiz));
        }

        var liste = await sorgu
            // ⭐ Taslaklar üstte dursun — üzerinde çalışılan onlar
            .OrderBy(y => y.Durum == YaziDurum.Yayinda)
            .ThenByDescending(y => y.UpdatedDate ?? y.CreatedDate)
            .ToListAsync();

        ViewBag.SeciliDurum = durum;
        ViewBag.Arama = arama;

        return View(liste);
    }

    // ══════════════════════════════════════════════════════
    //  2) OLUŞTURMA FORMU
    //  GET: /yonetim/yazi/olustur
    // ══════════════════════════════════════════════════════
    [HttpGet("olustur")]
    public async Task<IActionResult> Olustur()
    {
        ViewData["Title"] = "Yeni yazı";
        await EtiketleriHazirlaAsync();
        return View(new Yazi());
    }

    // ══════════════════════════════════════════════════════
    //  3) OLUŞTUR
    //  POST: /yonetim/yazi/olustur
    // ══════════════════════════════════════════════════════
    [HttpPost("olustur")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Olustur(Yazi yazi)
    {
        ViewData["Title"] = "Yeni yazı";

        if (!ModelState.IsValid)
        {
            await EtiketleriHazirlaAsync();
            return View(yazi);
        }

        // ⭐ Slug'ı BİZ üretiyoruz — kullanıcı formda görmüyor
        yazi.Slug = SlugUret(yazi.Baslik);

        yazi.CreatedDate = DateTime.Now;
        yazi.UpdatedDate = null;
        yazi.AktifMi = true;
        yazi.OkunmaSayisi = 0;

        // ⭐ Yayın tarihi: sadece yayınlanıyorsa dolsun
        yazi.YayinTarihi = yazi.Durum == YaziDurum.Yayinda
            ? DateTime.Now
            : null;

        await EtiketleriBaglaAsync(yazi, yazi.SeciliEtiketIdler);

        // ── Kapak görseli ───────────────────────────────────
        if (yazi.KapakDosya != null && yazi.KapakDosya.Length > 0)
        {
            var sonuc = await _gorsel.KaydetAsync(yazi.KapakDosya);

            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(nameof(Yazi.KapakDosya), sonuc.Hata!);
                await EtiketleriHazirlaAsync();
                return View(yazi);
            }

            yazi.KapakGorsel = sonuc.Yol;
        }

        _db.Yazilar.Add(yazi);
        await _db.SaveChangesAsync();

        TempData["Basarili"] = yazi.Durum == YaziDurum.Yayinda
            ? "Yazı yayınlandı."
            : "Taslak kaydedildi.";

        return RedirectToAction(nameof(Duzenle), new { id = yazi.YaziId });
    }

    // ══════════════════════════════════════════════════════
    //  4) DÜZENLEME FORMU
    //  GET: /yonetim/yazi/duzenle/5
    // ══════════════════════════════════════════════════════
    [HttpGet("duzenle/{id:long}")]
    public async Task<IActionResult> Duzenle(long id)
    {
        // ⭐ Include ŞART — etiketleri de çekmeliyiz ki
        //    formda hangilerinin işaretli geleceğini bilelim
        var yazi = await _db.Yazilar
            .Include(y => y.Etiketler)
            .FirstOrDefaultAsync(y => y.YaziId == id);

        if (yazi == null)
            return NotFound();

        yazi.SeciliEtiketIdler = yazi.Etiketler.Select(e => e.EtiketId).ToList();

        ViewData["Title"] = "Yazıyı düzenle";
        await EtiketleriHazirlaAsync();
        return View(yazi);
    }

    // ══════════════════════════════════════════════════════
    //  5) DÜZENLE
    //  POST: /yonetim/yazi/duzenle/5
    // ══════════════════════════════════════════════════════
    [HttpPost("duzenle/{id:long}")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(4 * 1024 * 1024)]      // ⭐ 4 MB (3 MB dosya + form verisi) 
    public async Task<IActionResult> Duzenle(long id, Yazi gelen)
    {
        ViewData["Title"] = "Yazıyı düzenle";

        if (!ModelState.IsValid)
        {
            await EtiketleriHazirlaAsync();
            return View(gelen);
        }

        var mevcut = await _db.Yazilar
            .Include(y => y.Etiketler)
            .FirstOrDefaultAsync(y => y.YaziId == id);

        if (mevcut == null)
            return NotFound();

        // ⭐ Taslaktan yayına mı geçiyor? Tarihi o zaman doldur.
        bool oncedenYayindaydi = mevcut.Durum == YaziDurum.Yayinda;
        bool simdiYayinda = gelen.Durum == YaziDurum.Yayinda;

        mevcut.Baslik = gelen.Baslik;
        mevcut.Ozet = gelen.Ozet;
        mevcut.Icerik = gelen.Icerik;
        mevcut.Durum = gelen.Durum;
        mevcut.UpdatedDate = DateTime.Now;


        // ── Kapak görseli ───────────────────────────────────
        if (gelen.KapakKaldir)
        {
            // Kullanıcı "kaldır" dedi
            _gorsel.Sil(mevcut.KapakGorsel);
            mevcut.KapakGorsel = null;
        }
        else if (gelen.KapakDosya != null && gelen.KapakDosya.Length > 0)
        {
            var sonuc = await _gorsel.KaydetAsync(gelen.KapakDosya);

            if (!sonuc.Basarili)
            {
                ModelState.AddModelError(nameof(Yazi.KapakDosya), sonuc.Hata!);
                await EtiketleriHazirlaAsync();
                return View(gelen);
            }

            // ⭐ Yeni görsel kaydedildikten SONRA eskisini sil.
            //    Sıra önemli: önce silseydik ve yükleme başarısız
            //    olsaydı, hem eski hem yeni görsel kaybolurdu.
            string? eski = mevcut.KapakGorsel;
            mevcut.KapakGorsel = sonuc.Yol;
            _gorsel.Sil(eski);
        }



        if (simdiYayinda && !oncedenYayindaydi)
        {
            // İlk kez yayınlanıyor
            mevcut.YayinTarihi = DateTime.Now;
        }
        else if (!simdiYayinda)
        {
            // Yayından çekildi
            mevcut.YayinTarihi = null;
        }
        // Zaten yayındaydı ve hâlâ yayında → ESKİ TARİHİ KORU
        // (else bloğu yok — bilinçli)

        // ⚠️ SLUG'A DOKUNMUYORUZ
        //    Başlık değişse bile adres sabit kalır.
        //    Değiştirseydik paylaşılmış tüm bağlantılar kırılırdı.
        //    (Elle değiştirme seçeneği Alıştırma 4'te.)

        await EtiketleriBaglaAsync(mevcut, gelen.SeciliEtiketIdler);

        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Yazı güncellendi.";
        return RedirectToAction(nameof(Duzenle), new { id });
    }

    // ══════════════════════════════════════════════════════
    //  6) HIZLI DURUM DEĞİŞTİR
    //  POST: /yonetim/yazi/durum/5
    // ══════════════════════════════════════════════════════
    [HttpPost("durum/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Durum(long id, YaziDurum yeniDurum)
    {
        var yazi = await _db.Yazilar.FindAsync(id);

        if (yazi == null)
            return NotFound();

        yazi.Durum = yeniDurum;
        yazi.UpdatedDate = DateTime.Now;

        if (yeniDurum == YaziDurum.Yayinda && yazi.YayinTarihi == null)
            yazi.YayinTarihi = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Basarili"] = yeniDurum == YaziDurum.Yayinda
            ? "Yazı yayınlandı."
            : "Yazı taslağa alındı.";

        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  7) SİLME ONAYI
    //  GET: /yonetim/yazi/sil/5
    // ══════════════════════════════════════════════════════
    [HttpGet("sil/{id:long}")]
    public async Task<IActionResult> Sil(long id)
    {
        var yazi = await _db.Yazilar
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.YaziId == id);

        if (yazi == null)
            return NotFound();

        ViewData["Title"] = "Yazıyı sil";
        return View(yazi);
    }

    // ══════════════════════════════════════════════════════
    //  8) SİL (soft delete)
    //  POST: /yonetim/yazi/sil/5
    // ══════════════════════════════════════════════════════
    [HttpPost("sil/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnayla(long id)
    {
        var yazi = await _db.Yazilar.FindAsync(id);

        if (yazi == null)
            return NotFound();

        yazi.AktifMi = false;
        yazi.UpdatedDate = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Basarili"] = "Yazı silindi.";
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════
    //  YARDIMCI: etiketleri bağla
    //
    //  ⭐ Clear() + Add() → EF farkı kendisi hesaplar.
    //     Sadece gerçekten değişen bağlantılar için SQL üretir.
    //
    //  ⚠️ yazi nesnesi TAKİP EDİLİYOR olmalı ve Etiketler
    //     koleksiyonu YÜKLENMİŞ olmalı (Include).
    //     Yeni kayıtta ikisi de zaten sağlanıyor.
    // ══════════════════════════════════════════════════════
    private async Task EtiketleriBaglaAsync(Yazi yazi, List<long> seciliIdler)
    {
        yazi.Etiketler.Clear();

        if (seciliIdler == null || seciliIdler.Count == 0)
            return;

        var etiketler = await _db.Etiketler
            .Where(e => seciliIdler.Contains(e.EtiketId))
            .ToListAsync();

        foreach (var etiket in etiketler)
            yazi.Etiketler.Add(etiket);
    }
}