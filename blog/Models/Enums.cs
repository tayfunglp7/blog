namespace blog.Models;

/// <summary>
/// Yazının yayın durumu.
///
/// Veritabanına METİN olarak yazılacak (DbContext'te ayarlanıyor).
/// SSMS'te tabloya bakınca "Yayinda" görmek "1" görmekten iyidir.
/// </summary>
public enum YaziDurum
{
    Taslak,
    Yayinda
}