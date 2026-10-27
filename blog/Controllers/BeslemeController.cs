using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;
using blog.Models;

namespace blog.Controllers;

public class BeslemeController : GenelController
{
    private readonly BlogDbContext _db;
    private readonly SiteAyarlari _site;      // ⭐ appsettings'ten (Modül 3)

    public BeslemeController(BlogDbContext db, SiteAyarlari site)
    {
        _db = db;
        _site = site;
    }

    // GET: /rss
    [Route("/rss")]
    public async Task<IActionResult> Rss()
    {
        var yazilar = await _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda)
            .OrderByDescending(y => y.YayinTarihi)
            .Take(20)
            .ToListAsync();

        string temelAdres = $"{Request.Scheme}://{Request.Host}";

        var sb = new StringBuilder();

        // ⭐ XmlWriter kullanıyoruz, elle string birleştirmiyoruz.
        //    Sebep: başlıkta & veya < geçerse XML bozulur.
        //    XmlWriter kaçırmayı (escaping) kendisi yapar.
        var ayarlar = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using (var yazici = XmlWriter.Create(sb, ayarlar))
        {
            yazici.WriteStartDocument();
            yazici.WriteStartElement("rss");
            yazici.WriteAttributeString("version", "2.0");

            // atom: ad alanı — self link için gerekli
            yazici.WriteAttributeString("xmlns", "atom", null,
                "http://www.w3.org/2005/Atom");

            yazici.WriteStartElement("channel");

            yazici.WriteElementString("title", _site.Baslik);
            yazici.WriteElementString("link", temelAdres);
            yazici.WriteElementString("description", _site.Aciklama);
            yazici.WriteElementString("language", "tr");

            yazici.WriteElementString("lastBuildDate",
                DateTime.Now.ToString("R"));     // RFC 1123 biçimi

            // Beslemenin kendi adresi
            yazici.WriteStartElement("atom", "link",
                "http://www.w3.org/2005/Atom");
            yazici.WriteAttributeString("href", $"{temelAdres}/rss");
            yazici.WriteAttributeString("rel", "self");
            yazici.WriteAttributeString("type", "application/rss+xml");
            yazici.WriteEndElement();

            foreach (var yazi in yazilar)
            {
                string adres = $"{temelAdres}/yazi/{yazi.Slug}";

                yazici.WriteStartElement("item");

                yazici.WriteElementString("title", yazi.Baslik);
                yazici.WriteElementString("link", adres);

                // guid: bu öğenin benzersiz kimliği.
                // Okuyucu uygulamalar "bunu gördüm mü?" için kullanır.
                yazici.WriteStartElement("guid");
                yazici.WriteAttributeString("isPermaLink", "true");
                yazici.WriteString(adres);
                yazici.WriteEndElement();

                yazici.WriteElementString("description",
                    MarkdownYardimcisi.DuzMetin(yazi.Ozet ?? yazi.Icerik, 300));

                if (yazi.YayinTarihi.HasValue)
                {
                    yazici.WriteElementString("pubDate",
                        yazi.YayinTarihi.Value.ToString("R"));
                }

                yazici.WriteEndElement();   // item
            }

            yazici.WriteEndElement();       // channel
            yazici.WriteEndElement();       // rss
            yazici.WriteEndDocument();
        }

        return Content(sb.ToString(), "application/rss+xml", Encoding.UTF8);
    }

    // GET: /sitemap.xml
    [Route("/sitemap.xml")]
    public async Task<IActionResult> SiteHaritasi()
    {
        string temelAdres = $"{Request.Scheme}://{Request.Host}";

        var yazilar = await _db.Yazilar
            .AsNoTracking()
            .Where(y => y.Durum == YaziDurum.Yayinda)
            .Select(y => new { y.Slug, y.YayinTarihi, y.UpdatedDate })
            .ToListAsync();

        var sayfalar = await _db.Sayfalar
            .AsNoTracking()
            .Select(s => new { s.Slug, s.UpdatedDate, s.CreatedDate })
            .ToListAsync();

        var sb = new StringBuilder();
        var ayarlar = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 };

        using (var yazici = XmlWriter.Create(sb, ayarlar))
        {
            yazici.WriteStartDocument();
            yazici.WriteStartElement("urlset",
                "http://www.sitemaps.org/schemas/sitemap/0.9");

            // Ana sayfa
            YazAdres(yazici, temelAdres, DateTime.Now, "daily", "1.0");

            foreach (var s in sayfalar)
            {
                YazAdres(yazici, $"{temelAdres}/{s.Slug}",
                    s.UpdatedDate ?? s.CreatedDate, "monthly", "0.8");
            }

            foreach (var y in yazilar)
            {
                YazAdres(yazici, $"{temelAdres}/yazi/{y.Slug}",
                    y.UpdatedDate ?? y.YayinTarihi ?? DateTime.Now, "monthly", "0.6");
            }

            yazici.WriteEndElement();
            yazici.WriteEndDocument();
        }

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    private static void YazAdres(XmlWriter yazici, string adres,
                                 DateTime tarih, string siklik, string oncelik)
    {
        yazici.WriteStartElement("url");
        yazici.WriteElementString("loc", adres);
        yazici.WriteElementString("lastmod", tarih.ToString("yyyy-MM-dd"));
        yazici.WriteElementString("changefreq", siklik);
        yazici.WriteElementString("priority", oncelik);
        yazici.WriteEndElement();
    }

    // GET: /robots.txt
    [Route("/robots.txt")]
    public IActionResult Robots()
    {
        string temelAdres = $"{Request.Scheme}://{Request.Host}";

        // ⭐ Yönetim panelini arama motorlarına kapat
        string icerik = $"""
            User-agent: *
            Allow: /
            Disallow: /yonetim
            Disallow: /giris

            Sitemap: {temelAdres}/sitemap.xml
            """;

        return Content(icerik, "text/plain", Encoding.UTF8);
    }
}