using Markdig;

namespace blog.Data;

/// <summary>
/// Markdown metnini HTML'e çevirir.
/// </summary>
public static class MarkdownYardimcisi
{
    /// <summary>
    /// ⭐ Pipeline bir kez kurulup tekrar tekrar kullanılır.
    ///    Her çağrıda yeniden kurmak israf olurdu.
    ///
    /// static readonly: uygulama ömrü boyunca tek nesne.
    /// </summary>
    private static readonly MarkdownPipeline Boru = new MarkdownPipelineBuilder()

        // ── Markdig'in ek özellikleri ────────────────────────
        //
        // UseAdvancedExtensions şunları açar:
        //   • Tablolar       | a | b |
        //   • Üstü çizili    ~~metin~~
        //   • Görev listesi  - [x] madde
        //   • Otomatik bağlantı (çıplak URL'ler tıklanabilir olur)
        //   • Dipnotlar
        .UseAdvancedExtensions()

        // Başlıklara otomatik id ver → /yazi/slug#bolum-adi
        // çalışsın, içindekiler listesi yapılabilsin
        .UseAutoIdentifiers()

        // ⭐⭐ GÜVENLİK: Markdown içindeki HAM HTML'i devre dışı bırak
        //
        //    Bu olmadan içeriğe <script>alert(1)</script> yazılabilir
        //    ve sayfada ÇALIŞIR.
        //
        //    "Ama içeriği sadece ben yazıyorum" — doğru, bu projede
        //    risk düşük. Ama alışkanlık olarak kapalı tutuyoruz:
        //      • Yarın yorum veya çok yazarlı yapı eklenebilir
        //      • Panel şifresi ele geçerse zarar sınırlı kalır
        //      • Varsayılan güvenli olmalı
        .DisableHtml()

        .Build();

    /// <summary>Markdown → HTML</summary>
    public static string Cevir(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "";

        return Markdown.ToHtml(markdown, Boru);
    }

    /// <summary>
    /// Markdown → düz metin.
    /// Özet üretmek ve paylaşım açıklaması için.
    /// </summary>
    public static string DuzMetin(string? markdown, int enFazla = 160)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return "";

        string duz = Markdown.ToPlainText(markdown, Boru)
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        // Art arda boşlukları teke indir
        while (duz.Contains("  "))
            duz = duz.Replace("  ", " ");

        if (duz.Length <= enFazla)
            return duz;

        // Kelimeyi ortadan bölme — son boşluktan kes
        string kesilmis = duz.Substring(0, enFazla);
        int sonBosluk = kesilmis.LastIndexOf(' ');

        if (sonBosluk > 0)
            kesilmis = kesilmis.Substring(0, sonBosluk);

        return kesilmis + "…";
    }
}