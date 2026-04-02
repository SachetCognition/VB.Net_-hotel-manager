using System.Security.Cryptography;

namespace HotelManagement.Core.Services;

/// <summary>
/// ID generation matching VB.NET GetUniqueKey() using RNGCryptoServiceProvider.
/// </summary>
public class IdGenerationService
{
    private static readonly char[] Digits = "0123456789".ToCharArray();

    /// <summary>
    /// Generate Guest ID: "G-" + 6 random digits (from frmGuest.vb).
    /// </summary>
    public string GenerateGuestId()
    {
        return "G-" + GenerateRandomDigits(6);
    }

    /// <summary>
    /// Generate Employee ID: "E-" + 6 random digits (from frmEmployee_registration.vb).
    /// </summary>
    public string GenerateEmployeeId()
    {
        return "E-" + GenerateRandomDigits(6);
    }

    /// <summary>
    /// Generate Payment ID: "SP-" + 9 random digits (from frmEmployeePayment.vb).
    /// </summary>
    public string GeneratePaymentId()
    {
        return "SP-" + GenerateRandomDigits(9);
    }

    /// <summary>
    /// Generate Stock ID with prefix and random digits.
    /// </summary>
    public string GenerateStockId()
    {
        return "ST-" + GenerateRandomDigits(6);
    }

    /// <summary>
    /// Generate Reservation ID with prefix and random digits.
    /// </summary>
    public string GenerateReservationId()
    {
        return "R-" + GenerateRandomDigits(6);
    }

    /// <summary>
    /// Generate Bill Number with prefix and random digits.
    /// </summary>
    public string GenerateBillNo()
    {
        return "B-" + GenerateRandomDigits(8);
    }

    private static string GenerateRandomDigits(int length)
    {
        byte[] data = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = Digits[data[i] % 10];
        }
        return new string(result);
    }
}
