using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace blog.Migrations
{
    /// <inheritdoc />
    public partial class IlkOlusturma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aboneler",
                columns: table => new
                {
                    AboneId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Eposta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aboneler", x => x.AboneId);
                });

            migrationBuilder.CreateTable(
                name: "Etiketler",
                columns: table => new
                {
                    EtiketId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etiketler", x => x.EtiketId);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.KullaniciId);
                });

            migrationBuilder.CreateTable(
                name: "Mesajlar",
                columns: table => new
                {
                    MesajId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Eposta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Konu = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OkunduMu = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesajlar", x => x.MesajId);
                });

            migrationBuilder.CreateTable(
                name: "Sayfalar",
                columns: table => new
                {
                    SayfaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenudeGoster = table.Column<bool>(type: "bit", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sayfalar", x => x.SayfaId);
                });

            migrationBuilder.CreateTable(
                name: "Yazilar",
                columns: table => new
                {
                    YaziId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    Ozet = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KapakGorsel = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OkunmaSayisi = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yazilar", x => x.YaziId);
                });

            migrationBuilder.CreateTable(
                name: "YaziEtiketleri",
                columns: table => new
                {
                    EtiketlerEtiketId = table.Column<long>(type: "bigint", nullable: false),
                    YazilarYaziId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YaziEtiketleri", x => new { x.EtiketlerEtiketId, x.YazilarYaziId });
                    table.ForeignKey(
                        name: "FK_YaziEtiketleri_Etiketler_EtiketlerEtiketId",
                        column: x => x.EtiketlerEtiketId,
                        principalTable: "Etiketler",
                        principalColumn: "EtiketId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YaziEtiketleri_Yazilar_YazilarYaziId",
                        column: x => x.YazilarYaziId,
                        principalTable: "Yazilar",
                        principalColumn: "YaziId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Etiketler",
                columns: new[] { "EtiketId", "Ad", "AktifMi", "Slug" },
                values: new object[,]
                {
                    { 1L, "Yazılım", true, "yazilim" },
                    { 2L, "Veritabanı", true, "veritabani" },
                    { 3L, "Tasarım", true, "tasarim" },
                    { 4L, "Günlük", true, "gunluk" },
                    { 5L, "Kitap", true, "kitap" }
                });

            migrationBuilder.InsertData(
                table: "Kullanicilar",
                columns: new[] { "KullaniciId", "AdSoyad", "AktifMi", "CreatedDate", "KullaniciAdi", "SifreHash" },
                values: new object[] { 1L, "Site Sahibi", true, new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), "admin", "36139B67115FF01F55FE490D297FDD79E1F82068C1FCD684CECDF3D399689A6F" });

            migrationBuilder.InsertData(
                table: "Sayfalar",
                columns: new[] { "SayfaId", "AktifMi", "Baslik", "CreatedDate", "Icerik", "MenudeGoster", "Sira", "Slug", "UpdatedDate" },
                values: new object[] { 1L, true, "Hakkımda", new DateTime(2026, 1, 15, 10, 0, 0, 0, DateTimeKind.Unspecified), "Merhaba. Ben bir yazılım geliştiricisiyim.\r\n\r\nBu siteyi öğrendiklerimi not etmek ve paylaşmak için kurdum. Burada\r\nçoğunlukla **web geliştirme**, **veritabanı tasarımı** ve ara sıra\r\nokuduğum kitaplar üzerine yazıyorum.\r\n\r\n## Neler yapıyorum\r\n\r\n- ASP.NET Core ile web uygulamaları geliştiriyorum\r\n- Veritabanı tasarımı ve optimizasyonu ile ilgileniyorum\r\n- Arayüz tasarımına ve tipografiye meraklıyım\r\n\r\n## İletişim\r\n\r\nBana [iletişim sayfasından](/iletisim) ulaşabilirsiniz.", true, 1, "hakkimda", null });

            migrationBuilder.CreateIndex(
                name: "IX_Aboneler_Eposta",
                table: "Aboneler",
                column: "Eposta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Etiketler_Ad",
                table: "Etiketler",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Etiketler_Slug",
                table: "Etiketler",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sayfalar_Slug",
                table: "Sayfalar",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YaziEtiketleri_YazilarYaziId",
                table: "YaziEtiketleri",
                column: "YazilarYaziId");

            migrationBuilder.CreateIndex(
                name: "IX_Yazilar_Durum_YayinTarihi",
                table: "Yazilar",
                columns: new[] { "Durum", "YayinTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Yazilar_Slug",
                table: "Yazilar",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aboneler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Mesajlar");

            migrationBuilder.DropTable(
                name: "Sayfalar");

            migrationBuilder.DropTable(
                name: "YaziEtiketleri");

            migrationBuilder.DropTable(
                name: "Etiketler");

            migrationBuilder.DropTable(
                name: "Yazilar");
        }
    }
}
