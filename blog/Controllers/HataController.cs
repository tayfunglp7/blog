using Microsoft.AspNetCore.Mvc;

namespace blog.Controllers;

public class HataController : GenelController
{
    [Route("/hata/{kod:int}")]
    public IActionResult Durum(int kod)
    {
        ViewData["Title"] = kod == 404 ? "Sayfa bulunamadı" : "Bir şeyler ters gitti";
        ViewBag.Kod = kod;
        return View("Durum");
    }

    [Route("/hata")]
    public IActionResult Genel()
    {
        ViewData["Title"] = "Bir şeyler ters gitti";
        ViewBag.Kod = 500;
        return View("Durum");
    }
}