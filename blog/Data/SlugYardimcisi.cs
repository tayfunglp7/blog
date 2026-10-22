using System.Text;
using System.Text.RegularExpressions;

namespace blog.Data;

/// <summary>
/// Başlıktan adres dostu slug üretir.
/// "Merhaba Dünya" → "merhaba-dunya"
/// </summary>
public static class SlugYardimcisi
{
    // Türkçe harfler ve karşılıkları — sıraları BİREBİR eşleşmeli
    private const string TurkceHarfler = "çğıöşüÇĞİÖŞÜ";
    private const string LatinKarsilik = "cgiosucgiosu";

    /// <summary>
    /// Metni slug'a çevirir.
    ///
    /// Sıra önemli:
    ///   1. Türkçe harfleri çevir   (küçültmeden ÖNCE!)
    ///   2. Küçük harfe çevir       (ToLowerInvariant — ToLower DEĞİL)
    ///   3. Harf/rakam dışını tire yap
    ///   4. Baştaki ve sondaki tireleri at
    /// </summary>
    public static string Uret(string metin)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return "";

        // ── 1) Türkçe harf çevirimi ──────────────────────────
        //
        // ⚠️ Bu adım küçük harfe çevirmeden ÖNCE yapılmalı.
        //    Sonra yapsaydık 'İ' karakteri ToLowerInvariant ile
        //    "i̇" (i + ayrı nokta) olurdu ve eşleşmezdi.
        var tampon = new StringBuilder(metin.Length);

        foreach (char harf in metin)
        {
            int sira = TurkceHarfler.IndexOf(harf);
            tampon.Append(sira >= 0 ? LatinKarsilik[sira] : harf);
        }

        // ── 2) Küçük harfe çevir ─────────────────────────────
        //
        // ⚠️⚠️ ToLowerInvariant — ToLower DEĞİL!
        //    ToLower(), bilgisayarın diline göre çalışır.
        //    Türkçe sistemde 'I' harfini 'ı' yapar ve
        //    bir sonraki adımda o harf tireye dönüşür, kaybolur.
        string slug = tampon.ToString().ToLowerInvariant();

        // ── 3) Harf ve rakam dışındaki her şeyi tire yap ────
        //
        // Art arda gelen işaretler tek tireye düşsün:
        //   "C# -- 12'de"  →  "c-12-de"   (üç tire değil)
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");

        // ── 4) Baştaki ve sondaki tireleri at ────────────────
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Benzersiz slug üretir. Slug zaten varsa sonuna sayı ekler.
    ///
    /// slugVarMi: "bu slug kullanımda mı?" sorusunu cevaplayan fonksiyon.
    ///            Veritabanı erişimini dışarıda bırakıyoruz — bu sınıf
    ///            EF'i tanımasın. Böylece test edilebilir kalıyor.
    /// </summary>
    public static string BenzersizUret(string metin, Func<string, bool> slugVarMi)
    {
        string temelSlug = Uret(metin);

        // Başlık tamamen işaretlerden oluşuyorsa slug boş kalabilir
        if (string.IsNullOrEmpty(temelSlug))
            temelSlug = "yazi";

        if (!slugVarMi(temelSlug))
            return temelSlug;

        // Çakışma var — sona sayı ekle
        int sayac = 2;
        while (slugVarMi($"{temelSlug}-{sayac}"))
            sayac++;

        return $"{temelSlug}-{sayac}";
    }

        /// <summary>
    /// Sistem tarafından kullanılan, sayfa slug'ı olamayacak adresler.
    ///
    /// ⚠️ Bu listeye yeni rota eklediğinizde buraya da ekleyin.
    ///    Unutulursa o sayfaya hiç erişilemez — ve sebebi
    ///    bulunması zor bir hata olur.
    /// </summary>
    private static readonly HashSet<string> YasakliSluglar = new(StringComparer.Ordinal)
    {
        "yazi", "etiket", "etiketler", "iletisim", "giris", "cikis",
        "yonetim", "hata", "rss", "arsiv", "ara", "css", "js",
        "lib", "yuklenen", "favicon"
    };

    public static bool SlugYasakliMi(string slug)
    {
        return YasakliSluglar.Contains(slug);
    }
}