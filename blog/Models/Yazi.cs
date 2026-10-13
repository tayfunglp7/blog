using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace blog.Models;

public class Yazi
{
    public long YaziId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(200, ErrorMessage = "En fazla 200 karakter.")]
    [Display(Name = "Başlık")]
    public string Baslik { get; set; } = "";

    // ⭐ SLUG
    // Kullanıcı formda GÖRMEZ ve DOLDURMAZ.
    // Controller, başlıktan üretir. Bir kez üretilir, sabit kalır.
    [StringLength(220)]
    public string Slug { get; set; } = "";

    /// <summary>
    /// Liste sayfalarında ve paylaşım önizlemelerinde görünen kısa tanıtım.
    /// Boş bırakılırsa içerikten otomatik üretilecek (Modül 5).
    /// </summary>
    [StringLength(300, ErrorMessage = "Özet en fazla 300 karakter.")]
    [Display(Name = "Özet")]
    public string? Ozet { get; set; }

    /// <summary>
    /// Yazının gövdesi — MARKDOWN olarak saklanır.
    /// HTML'e çevrim okuma anında yapılır (Modül 5).
    ///
    /// [StringLength] YAZMIYORUZ → NVARCHAR(MAX).
    /// Uzun yazılar için bilinçli tercih.
    /// </summary>
    [Required(ErrorMessage = "İçerik boş olamaz.")]
    [Display(Name = "İçerik")]
    public string Icerik { get; set; } = "";

    /// <summary>Kapak görseli yolu. Modül 10'da dolacak.</summary>
    [StringLength(300)]
    [Display(Name = "Kapak görseli")]
    public string? KapakGorsel { get; set; }

    [Required]
    [Display(Name = "Durum")]
    public YaziDurum Durum { get; set; } = YaziDurum.Taslak;

    /// <summary>
    /// İlk yayınlandığı an. Taslakta NULL.
    /// ⭐ CreatedDate'ten FARKLI: yazı bugün oluşturulup
    ///    gelecek hafta yayınlanabilir.
    /// </summary>
    [Display(Name = "Yayın tarihi")]
    public DateTime? YayinTarihi { get; set; }

    /// <summary>Kaç kez okundu. Modül 6'da artacak.</summary>
    public int OkunmaSayisi { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool AktifMi { get; set; } = true;

    // ── Navigasyon ───────────────────────────────────────────
    // ⭐ Skip navigation — ara tablo sınıfı yazmıyoruz (Modül 8)
    public List<Etiket> Etiketler { get; set; } = new();

    /// <summary>Formdan gelen seçili etiket id'leri.</summary>
    [NotMapped]
    public List<long> SeciliEtiketIdler { get; set; } = new();

    // ── Hesaplanan ───────────────────────────────────────────
    //
    // ⚠️ Hepsi [NotMapped]. Where/OrderBy İÇİNDE KULLANILAMAZ —
    //    EF bunları SQL'e çeviremez. Sadece view'da, veri
    //    belleğe geldikten sonra kullanılır.

    [NotMapped]
    public bool YayindaMi => Durum == YaziDurum.Yayinda;

    /// <summary>
    /// Tahmini okuma süresi (dakika).
    /// Ortalama okuma hızı dakikada ~200 kelime kabul edilir.
    /// </summary>
    [NotMapped]
    public int OkumaSuresi
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Icerik))
                return 1;

            int kelimeSayisi = Icerik.Split(
                new[] { ' ', '\n', '\r', '\t' },
                StringSplitOptions.RemoveEmptyEntries).Length;

            int dakika = (int)Math.Ceiling(kelimeSayisi / 200.0);
            return Math.Max(1, dakika);   // en az 1 dakika yaz
        }
    }

    /// <summary>Listede gösterilecek tanıtım metni.</summary>
    [NotMapped]
    public string GorunenOzet
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Ozet))
                return Ozet;

            // Özet girilmemişse içerikten üret.
            // Markdown işaretlerini kabaca temizle.
            string duz = Icerik
                .Replace("#", "").Replace("*", "")
                .Replace("`", "").Replace(">", "")
                .Replace("\r", " ").Replace("\n", " ");

            duz = duz.Trim();

            return duz.Length <= 160 ? duz : duz.Substring(0, 157) + "...";
        }
    }
}