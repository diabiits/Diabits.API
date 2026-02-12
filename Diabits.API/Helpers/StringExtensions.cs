using System.Security.Cryptography;
using System.Text;

namespace Diabits.API.Configuration;

public static class StringExtensions
{
    public static string HashToken(this string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }
}
