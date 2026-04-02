namespace HotelManagement.Core.Services;

/// <summary>
/// BCrypt password hashing replacing Base64 encoding from ModFunc.vb.
/// </summary>
public class PasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    /// <summary>
    /// Legacy Base64 decode for migration purposes only.
    /// </summary>
    public static string LegacyDecrypt(string encoded)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Legacy Base64 encode (for reference/migration only - do NOT use for new passwords).
    /// </summary>
    public static string LegacyEncrypt(string plain)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plain);
        return Convert.ToBase64String(bytes);
    }
}
