namespace blog.Data;

/// <summary>
/// Görsel yükleme ve silme.
///
/// Dört katmanlı kontrol:
///   1. Boyut sınırı
///   2. Uzantı beyaz listesi
///   3. MIME tipi kontrolü
///   4. Dosya imzası (magic bytes)
/// </summary>
public class GorselYardimcisi
{
    private readonly IWebHostEnvironment _ortam;
    private readonly ILogger<GorselYardimcisi> _gunluk;

    // En fazla 3 MB
    private const long EnFazlaBoyut = 3 * 1024 * 1024;

    // ⭐ BEYAZ LİSTE — "şunlar yasak" değil, "sadece şunlar serbest"
    //    Kara liste yaklaşımı her zaman eksik kalır.
    private static readonly string[] IzinliUzantilar =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private static readonly string[] IzinliMimeTipleri =
        { "image/jpeg", "image/png", "image/webp", "image/gif" };

    public GorselYardimcisi(IWebHostEnvironment ortam, ILogger<GorselYardimcisi> gunluk)
    {
        _ortam = ortam;
        _gunluk = gunluk;
    }

    /// <summary>Yükleme sonucunu taşır: başarı, yol ve hata birlikte.</summary>
    public record YuklemeSonucu(bool Basarili, string? Yol, string? Hata);

    /// <summary>
    /// Görseli doğrular ve kaydeder.
    /// Başarılıysa web yolunu döner ("/yuklenen/2026-10/abc.jpg").
    /// </summary>
    public async Task<YuklemeSonucu> KaydetAsync(IFormFile? dosya)
    {
        if (dosya == null || dosya.Length == 0)
            return new YuklemeSonucu(false, null, "Dosya seçilmedi.");

        // ── 1) BOYUT ────────────────────────────────────────
        if (dosya.Length > EnFazlaBoyut)
            return new YuklemeSonucu(false, null,
                $"Dosya çok büyük. En fazla {EnFazlaBoyut / 1024 / 1024} MB olabilir.");

        // ── 2) UZANTI ───────────────────────────────────────
        //
        // ⚠️ ToLowerInvariant kullanıyoruz. Bu örnekte ToLower() de
        //    aynı sonucu verirdi (".JPG" içinde 'I' harfi yok), ama
        //    uzantı karşılaştırması PROGRAMATİK bir karşılaştırmadır.
        //    Modül 2'deki kural: makine için invariant, insan için kültür.
        string uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();

        if (!IzinliUzantilar.Contains(uzanti))
            return new YuklemeSonucu(false, null,
                $"Sadece şu biçimler yüklenebilir: {string.Join(", ", IzinliUzantilar)}");

        // ── 3) MIME TİPİ ────────────────────────────────────
        //
        // Tarayıcının bildirdiği tip. Kolayca sahte olabilir
        // ama bedava bir kontrol katmanı.
        if (!IzinliMimeTipleri.Contains(dosya.ContentType.ToLowerInvariant()))
            return new YuklemeSonucu(false, null, "Dosya türü desteklenmiyor.");

        // ── 4) ⭐ DOSYA İMZASI ──────────────────────────────
        //
        // Asıl kontrol bu. Uzantı ve MIME yalan söyleyebilir,
        // dosyanın ilk baytları söyleyemez.
        if (!await ImzaGecerliMiAsync(dosya))
        {
            _gunluk.LogWarning("Geçersiz dosya imzası: {ad}", dosya.FileName);
            return new YuklemeSonucu(false, null, "Dosya geçerli bir görsel değil.");
        }

        // ── 5) KAYDET ───────────────────────────────────────
        //
        // ⭐ Dosya adını BİZ üretiyoruz.
        //    Kullanıcının verdiği adın hiçbir parçası kullanılmıyor
        //    → path traversal imkânsız.
        string yeniAd = $"{Guid.NewGuid():N}{uzanti}";

        // Tarihe göre klasörle — tek klasörde 10.000 dosya olmasın
        string altKlasor = DateTime.Now.ToString("yyyy-MM");
        string klasorYolu = Path.Combine(_ortam.WebRootPath, "yuklenen", altKlasor);

        Directory.CreateDirectory(klasorYolu);   // varsa dokunmaz

        string tamYol = Path.Combine(klasorYolu, yeniAd);

        using (var akis = new FileStream(tamYol, FileMode.Create))
        {
            await dosya.CopyToAsync(akis);
        }

        // Web'den erişilecek yol — ters eğik çizgi DEĞİL
        return new YuklemeSonucu(true, $"/yuklenen/{altKlasor}/{yeniAd}", null);
    }

    /// <summary>
    /// Dosyanın ilk baytlarını okuyup gerçekten görsel olup
    /// olmadığını kontrol eder.
    /// </summary>
    private static async Task<bool> ImzaGecerliMiAsync(IFormFile dosya)
    {
        // İlk 12 bayt tüm biçimleri ayırt etmeye yeter
        byte[] tampon = new byte[12];

        using (var akis = dosya.OpenReadStream())
        {
            int okunan = await akis.ReadAsync(tampon, 0, tampon.Length);

            if (okunan < 4)
                return false;      // dosya çok küçük, görsel olamaz
        }

        // JPEG:  FF D8 FF
        if (tampon[0] == 0xFF && tampon[1] == 0xD8 && tampon[2] == 0xFF)
            return true;

        // PNG:   89 50 4E 47
        if (tampon[0] == 0x89 && tampon[1] == 0x50 &&
            tampon[2] == 0x4E && tampon[3] == 0x47)
            return true;

        // GIF:   47 49 46 38   ("GIF8")
        if (tampon[0] == 0x47 && tampon[1] == 0x49 &&
            tampon[2] == 0x46 && tampon[3] == 0x38)
            return true;

        // WEBP:  52 49 46 46 ("RIFF") ... 57 45 42 50 ("WEBP")
        if (tampon.Length >= 12 &&
            tampon[0] == 0x52 && tampon[1] == 0x49 &&
            tampon[2] == 0x46 && tampon[3] == 0x46 &&
            tampon[8] == 0x57 && tampon[9] == 0x45 &&
            tampon[10] == 0x42 && tampon[11] == 0x50)
            return true;

        return false;
    }

    /// <summary>
    /// Görseli diskten siler.
    ///
    /// ⚠️ Yol kontrolü yapıyoruz — "/yuklenen/" ile başlamayan
    ///    hiçbir şeyi silmiyoruz. Bu kontrol olmasaydı
    ///    veritabanındaki bozuk bir yol sistem dosyası sildirebilirdi.
    /// </summary>
    public void Sil(string? webYolu)
    {
        if (string.IsNullOrWhiteSpace(webYolu))
            return;

        if (!webYolu.StartsWith("/yuklenen/", StringComparison.Ordinal))
        {
            _gunluk.LogWarning("Şüpheli silme yolu reddedildi: {yol}", webYolu);
            return;
        }

        try
        {
            // Web yolunu disk yoluna çevir: /yuklenen/x/y.jpg → wwwroot\yuklenen\x\y.jpg
            string gorece = webYolu.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string tamYol = Path.Combine(_ortam.WebRootPath, gorece);

            if (System.IO.File.Exists(tamYol))
                System.IO.File.Delete(tamYol);
        }
        catch (Exception ex)
        {
            // Silme başarısız olursa uygulamayı çökertme —
            // yetim dosya kalır, o kadar.
            _gunluk.LogError(ex, "Görsel silinemedi: {yol}", webYolu);
        }
    }
}