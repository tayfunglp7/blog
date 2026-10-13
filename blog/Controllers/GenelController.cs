using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace blog.Controllers;

/// <summary>
/// Genel sitenin tüm controller'larının tabanı.
///
/// ⭐ [AllowAnonymous] burada yazıldı, miras alan herkeste geçerli.
///    Program.cs'teki global "giriş zorunlu" kuralını deler.
///
/// abstract: bu sınıftan doğrudan nesne üretilmez,
///           sadece miras alınır.
/// </summary>
[AllowAnonymous]
public abstract class GenelController : Controller
{
}