using System.Security.Cryptography;
using System.Text;

namespace blog.Data;

/// <summary>
/// ⚠️ ÖĞRETİM AMAÇLIDIR. Gerçek projede BCrypt/Argon2 kullanın.
/// SHA256 çok hızlıdır ve tuz içermez.
/// </summary>
public static class SifreYardimcisi
{
    public static string Hashle(string sifre)
    {
        byte[] bayt = Encoding.UTF8.GetBytes(sifre);
        byte[] hash = SHA256.HashData(bayt);
        return Convert.ToHexString(hash);
    }
}