using System.ComponentModel.DataAnnotations;

namespace blog.Models;

/// <summary>
/// "Hakkımda", "Projelerim" gibi sabit sayfalar.
///
/// ⭐ Neden veritabanında? Koda gömseydik her yeni sayfa için
///    kod yazıp yeniden yayınlamak gerekirdi. Böyle, panelden
///    eklenebiliyor.
/// </summary>
public class Sayfa
{
    public long SayfaId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Sayfa başlığı")]
    public string Baslik { get; set; } = "";

    [StringLength(120)]
    public string Slug { get; set; } = "";

    [Required(ErrorMessage = "İçerik boş olamaz.")]
    [Display(Name = "İçerik")]
    public string Icerik { get; set; } = "";       // markdown

    [Display(Name = "Menüde göster")]
    public bool MenudeGoster { get; set; } = true;

    /// <summary>Menüdeki sırası. Küçük olan solda.</summary>
    [Display(Name = "Sıra")]
    [Range(0, 99, ErrorMessage = "Sıra 0-99 arasında olmalı.")]
    public int Sira { get; set; } = 10;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool AktifMi { get; set; } = true;
}