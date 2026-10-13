using System.ComponentModel.DataAnnotations;

namespace blog.Models;

public class Kullanici
{
    public long KullaniciId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Kullanıcı adı")]
    public string KullaniciAdi { get; set; } = "";

    [Required]
    [StringLength(255)]
    public string SifreHash { get; set; } = "";

    [Required]
    [StringLength(150)]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = "";

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool AktifMi { get; set; } = true;
}


/// <summary>Giriş formunun taşıyıcısı — tablo değil.</summary>
public class GirisViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı gerekli.")]
    [Display(Name = "Kullanıcı adı")]
    public string KullaniciAdi { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = "";

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }
}