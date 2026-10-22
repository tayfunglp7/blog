using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blog.Data;

namespace blog.Controllers;

public class SayfaController : GenelController
{
    private readonly BlogDbContext _db;

    public SayfaController(BlogDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Kök seviyesindeki sayfalar: /hakkimda, /projelerim ...
    /// Rota Program.cs'te tanımlı (en sondaki yakalayıcı).
    /// </summary>
    public async Task<IActionResult> Goster(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var sayfa = await _db.Sayfalar
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug);

        if (sayfa == null)
            return NotFound();

        ViewData["Title"] = sayfa.Baslik;
        ViewData["Aciklama"] = MarkdownYardimcisi.DuzMetin(sayfa.Icerik, 160);

        return View(sayfa);
    }
}