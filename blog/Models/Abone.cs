using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace blog.Models;

/// <summary>
/// E-posta bülteni aboneleri.
///
/// ⚠️ Bu tabloya da GİRİŞ YAPMAMIŞ kişiler yazıyor — Mesaj gibi.
///    Modül 9'da ikisini birlikte güvene alacağız.
/// </summary>
[Index(nameof(Eposta), IsUnique = true)]   // ⭐ aynı e-posta iki kere kaydolamaz
public class Abone
{
    public long AboneId { get; set; }

    [Required(ErrorMessage = "E-posta adresi gerekli.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(255)]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = "";

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool AktifMi { get; set; } = true;
}