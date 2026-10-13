using Microsoft.EntityFrameworkCore;
using blog.Models;

namespace blog.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Yazi> Yazilar { get; set; }
    public DbSet<Etiket> Etiketler { get; set; }
    public DbSet<Sayfa> Sayfalar { get; set; }
    public DbSet<Mesaj> Mesajlar { get; set; }
    public DbSet<Abone> Aboneler { get; set; }
    public DbSet<Kullanici> Kullanicilar { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ══════════════════════════════════════════════════════
        //  ENUM → METİN
        //  SSMS'te "Yayinda" görmek "1" görmekten iyi.
        //  ⚠️ Migration üretmeden ÖNCE yazın.
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Yazi>()
            .Property(y => y.Durum)
            .HasConversion<string>()
            .HasMaxLength(20);

        // ══════════════════════════════════════════════════════
        //  BENZERSİZ SLUG'LAR
        //
        //  ⭐ Bu indeksler sadece veri bütünlüğü için değil,
        //     PERFORMANS için de kritik. Her yazı sayfası
        //     açılışında "slug'ı bu olan yazı" sorgusu çalışacak.
        //     İndeks olmadan tam tablo taraması yapılır.
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Yazi>().HasIndex(y => y.Slug).IsUnique();
        modelBuilder.Entity<Etiket>().HasIndex(e => e.Slug).IsUnique();
        modelBuilder.Entity<Sayfa>().HasIndex(s => s.Slug).IsUnique();

        modelBuilder.Entity<Etiket>().HasIndex(e => e.Ad).IsUnique();
        modelBuilder.Entity<Kullanici>().HasIndex(k => k.KullaniciAdi).IsUnique();

        // Genel site "yayında olanları, tarihe göre" listeleyecek
        modelBuilder.Entity<Yazi>().HasIndex(y => new { y.Durum, y.YayinTarihi });

        // ══════════════════════════════════════════════════════
        //  ÇOKTAN ÇOĞA — ara tablo sınıfı YAZMIYORUZ
        //  Sadece adını veriyoruz (varsayılan "EtiketYazi" olurdu)
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Yazi>()
            .HasMany(y => y.Etiketler)
            .WithMany(e => e.Yazilar)
            .UsingEntity(ara => ara.ToTable("YaziEtiketleri"));

        // ══════════════════════════════════════════════════════
        //  SOFT DELETE
        // ══════════════════════════════════════════════════════
        modelBuilder.Entity<Yazi>().HasQueryFilter(y => y.AktifMi);
        modelBuilder.Entity<Etiket>().HasQueryFilter(e => e.AktifMi);
        modelBuilder.Entity<Sayfa>().HasQueryFilter(s => s.AktifMi);
        modelBuilder.Entity<Mesaj>().HasQueryFilter(m => m.AktifMi);
        modelBuilder.Entity<Abone>().HasQueryFilter(a => a.AktifMi);
        modelBuilder.Entity<Kullanici>().HasQueryFilter(k => k.AktifMi);

        // ══════════════════════════════════════════════════════
        //  BAŞLANGIÇ VERİSİ
        //  ⚠️ DateTime.Now KULLANILAMAZ — sabit tarih zorunlu.
        //     (Her migrations add'de değişirse EF boş migration üretir.)
        // ══════════════════════════════════════════════════════
        var sabitTarih = new DateTime(2026, 1, 15, 10, 0, 0);

        modelBuilder.Entity<Kullanici>().HasData(
            new Kullanici
            {
                KullaniciId = 1,
                KullaniciAdi = "admin",
                SifreHash = SifreYardimcisi.Hashle("blog2026"),
                AdSoyad = "Site Sahibi",
                CreatedDate = sabitTarih,
                AktifMi = true
            }
        );

        modelBuilder.Entity<Etiket>().HasData(
            new Etiket { EtiketId = 1, Ad = "Yazılım", Slug = "yazilim", AktifMi = true },
            new Etiket { EtiketId = 2, Ad = "Veritabanı", Slug = "veritabani", AktifMi = true },
            new Etiket { EtiketId = 3, Ad = "Tasarım", Slug = "tasarim", AktifMi = true },
            new Etiket { EtiketId = 4, Ad = "Günlük", Slug = "gunluk", AktifMi = true },
            new Etiket { EtiketId = 5, Ad = "Kitap", Slug = "kitap", AktifMi = true }
        );

        modelBuilder.Entity<Sayfa>().HasData(
            new Sayfa
            {
                SayfaId = 1,
                Baslik = "Hakkımda",
                Slug = "hakkimda",
                MenudeGoster = true,
                Sira = 1,
                CreatedDate = sabitTarih,
                AktifMi = true,
                Icerik = @"Merhaba. Ben bir yazılım geliştiricisiyim.

Bu siteyi öğrendiklerimi not etmek ve paylaşmak için kurdum. Burada
çoğunlukla **web geliştirme**, **veritabanı tasarımı** ve ara sıra
okuduğum kitaplar üzerine yazıyorum.

## Neler yapıyorum

- ASP.NET Core ile web uygulamaları geliştiriyorum
- Veritabanı tasarımı ve optimizasyonu ile ilgileniyorum
- Arayüz tasarımına ve tipografiye meraklıyım

## İletişim

Bana [iletişim sayfasından](/iletisim) ulaşabilirsiniz."
            }
        );
    }
}