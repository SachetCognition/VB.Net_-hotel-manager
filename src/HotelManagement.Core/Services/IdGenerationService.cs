using System.Security.Cryptography;

namespace HotelManagement.Core.Services;

/// <summary>
/// ID generation matching VB.NET GetUniqueKey() using RNGCryptoServiceProvider.
/// Original VB.NET uses chars "123456789" (no zero) with GetNonZeroBytes.
/// </summary>
public class IdGenerationService
{
    private static readonly char[] Digits = "123456789".ToCharArray();

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
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            // RandomNumberGenerator.GetInt32 uses rejection sampling for uniform distribution,
            // avoiding the modulo bias of byte % 9. Range [0, 9) maps to Digits indices 0-8
            // which are chars '1'-'9', matching the original VB.NET behavior (no zeros).
            result[i] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];
        }
        return new string(result);
    }
}
