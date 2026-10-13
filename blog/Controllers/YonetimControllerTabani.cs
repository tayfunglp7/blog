using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog.Controllers;

/// <summary>
/// Yönetim panelinin tüm controller'larının tabanı.
///
/// İki iş yapıyor:
///   1. [Authorize]  → giriş zorunlu
///   2. [Route]      → ortak yol öneki
///
/// ⭐ [controller] yer tutucusu, sınıf adından "Controller"
///    ekini atıp kalanı koyar:
///       YaziYonetimController  →  /yonetim/yaziyonetim
/// </summary>
[Authorize]
[Route("yonetim/[controller]")]
public abstract class YonetimControllerTabani : Controller
{
}