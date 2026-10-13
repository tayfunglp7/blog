namespace blog.Models;

/// <summary>
/// appsettings.json'daki "Site" bölümünün C# karşılığı.
/// Özellik adları JSON'daki anahtarlarla aynı olmalı.
/// </summary>
public class SiteAyarlari
{
    public string Baslik { get; set; } = "Blog";
    public string AdSoyad { get; set; } = "";
    public string Unvan { get; set; } = "";
    public string Hakkimda { get; set; } = "";
    public string ProfilFoto { get; set; } = "";
    public string Aciklama { get; set; } = "";

    /// <summary>Sosyal medya bağlantıları — ikon adı : adres</summary>
    public List<SosyalBaglanti> Sosyal { get; set; } = new();
}

public class SosyalBaglanti
{
    /// <summary>Bootstrap Icons adı: "github", "linkedin", "twitter-x"…</summary>
    public string Ikon { get; set; } = "";
    public string Adres { get; set; } = "";
    public string Etiket { get; set; } = "";
}