using System.ComponentModel.DataAnnotations;

namespace blog.Models;

public class Etiket
{
    public long EtiketId { get; set; }

    [Required(ErrorMessage = "Etiket adı zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Etiket adı")]
    public string Ad { get; set; } = "";

    // Etiketin de slug'ı var: /etiket/veritabani
    [StringLength(60)]
    public string Slug { get; set; } = "";

    public bool AktifMi { get; set; } = true;

    // Skip navigation
    public List<Yazi> Yazilar { get; set; } = new();
}