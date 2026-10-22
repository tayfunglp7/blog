using System.ComponentModel.DataAnnotations;

namespace blog.Models;

/// <summary>
/// İletişim formundan gelen mesajlar.
///
/// ⚠️ Bu tabloya GİRİŞ YAPMAMIŞ kişiler yazacak.
///    Projedeki tek anonim yazma noktası. Modül 9'da
///    doğrulama ve spam önlemlerini ele alacağız.
/// </summary>
public class Mesaj
{
    public long MesajId { get; set; }

    [Required(ErrorMessage = "Adınızı yazın.")]
    [StringLength(100)]
    [Display(Name = "Adınız")]
    public string Ad { get; set; } = "";

    [Required(ErrorMessage = "E-posta adresinizi yazın.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(255)]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [Required(ErrorMessage = "Konu yazın.")]
    [StringLength(150)]
    [Display(Name = "Konu")]
    public string Konu { get; set; } = "";

    [Required(ErrorMessage = "Mesajınızı yazın.")]
    [StringLength(2000, MinimumLength = 10,
        ErrorMessage = "Mesaj 10-2000 karakter arasında olmalı.")]
    [Display(Name = "Mesajınız")]
    public string Icerik { get; set; } = "";

    public bool OkunduMu { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool AktifMi { get; set; } = true;
}

/// <summary>
/// İletişim formunun taşıyıcısı.
///
/// ⭐ Neden Mesaj sınıfını doğrudan kullanmıyoruz?
///    Mesaj'da OkunduMu, AktifMi, CreatedDate var.
///    Forma koysaydık kötü niyetli biri F12 ile
///    OkunduMu=true gönderip mesajını gizleyebilirdi.
///
///    KURAL: Kullanıcıdan gelen modele SADECE gereken alanları koy.
/// </summary>
public class IletisimViewModel
{
    [Required(ErrorMessage = "Adınızı yazın.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad 2-100 karakter olmalı.")]
    [Display(Name = "Adınız")]
    public string Ad { get; set; } = "";

    [Required(ErrorMessage = "E-posta adresinizi yazın.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(255)]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    [Required(ErrorMessage = "Konu yazın.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Konu 3-150 karakter olmalı.")]
    [Display(Name = "Konu")]
    public string Konu { get; set; } = "";

    [Required(ErrorMessage = "Mesajınızı yazın.")]
    [StringLength(2000, MinimumLength = 10,
        ErrorMessage = "Mesaj 10-2000 karakter arasında olmalı.")]
    [Display(Name = "Mesajınız")]
    public string Icerik { get; set; } = "";

    // ══════════════════════════════════════════════════════
    //  ⭐ BAL KÜPÜ
    //  Gerçek kullanıcı bu alanı göremez ve doldurmaz.
    //  Doluysa gönderen bir bottur.
    //
    //  Adı bilerek masum: "Website". Botlar alan adlarına
    //  bakarak doldurur; "honeypot" yazsaydık anlarlardı.
    // ══════════════════════════════════════════════════════
    public string? Website { get; set; }

    /// <summary>
    /// Form sayfasının açıldığı an (Unix saniye).
    /// Çok hızlı gönderim bot işaretidir.
    /// </summary>
    public long AcilisZamani { get; set; }
}