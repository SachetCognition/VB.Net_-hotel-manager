namespace HotelManager.Domain.Entities;

public class HotelInfo
{
    public int ID { get; set; }
    public string? HotelName { get; set; }
    public string? Address { get; set; }
    public string? ContactNo { get; set; }
    public string? ContactNo1 { get; set; }
    public string? Email { get; set; }
    public string? TIN { get; set; }
    public string? STNo { get; set; }
    public byte[]? Logo { get; set; }
}

public class CurrencySet
{
    public int ID { get; set; }
    public string? CS_Currency { get; set; }
}

public class Room
{
    public string RoomNo { get; set; } = null!;
    public string? RoomType { get; set; }
    public int? RoomCharges { get; set; }
}

public class Hall
{
    public int ID { get; set; }
    public int? Charges { get; set; }
    public string? HallName { get; set; }
}

public class Garden
{
    public int ID { get; set; }
    public int? Charges { get; set; }
}

public class ExtraBed
{
    public int ID { get; set; }
    public int? Charges { get; set; }
}

public class Guest
{
    public string GuestID { get; set; } = null!;
    public string? GuestName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ContactNo { get; set; }
    public string? IDType { get; set; }
    public string? IDNumber { get; set; }
    public string? Notes { get; set; }
}

public class Registration
{
    public string UserName { get; set; } = null!;
    public string? UserType { get; set; }
    public string? User_Password { get; set; }
    public string? NameOfuser { get; set; }
    public string? ContactNo { get; set; }
    public string? Email { get; set; }
    public DateTime? JoiningDate { get; set; }
}

public class User
{
    public string Username { get; set; } = null!;
    public Registration? Registration { get; set; }
}
