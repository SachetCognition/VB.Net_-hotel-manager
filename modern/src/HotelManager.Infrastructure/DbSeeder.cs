using HotelManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManager.Infrastructure;

public static class DbSeeder
{
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminPassword = "admin@123";

    public static async Task SeedAsync(HotelDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Registrations.AnyAsync())
        {
            var hasher = new PasswordHasher<Registration>();
            var admin = new Registration
            {
                UserName = DefaultAdminUserName,
                UserType = "Admin",
                NameOfuser = "Administrator",
                Email = "admin@hotel.local",
                JoiningDate = DateTime.Today
            };
            admin.User_Password = hasher.HashPassword(admin, DefaultAdminPassword);
            db.Registrations.Add(admin);
            db.Users.Add(new User { Username = admin.UserName });
        }

        if (!await db.HotelInfos.AnyAsync())
        {
            db.HotelInfos.Add(new HotelInfo
            {
                HotelName = "Grand Palace Hotel",
                Address = "1 Seaside Boulevard",
                ContactNo = "+1-555-0100",
                ContactNo1 = "+1-555-0101",
                Email = "info@grandpalace.local",
                TIN = "TIN-000001",
                STNo = "ST-000001"
            });
        }

        if (!await db.Currencies.AnyAsync())
        {
            db.Currencies.AddRange(
                new CurrencySet { CS_Currency = "USD" },
                new CurrencySet { CS_Currency = "EUR" },
                new CurrencySet { CS_Currency = "INR" });
        }

        if (!await db.Rooms.AnyAsync())
        {
            db.Rooms.AddRange(
                new Room { RoomNo = "101", RoomType = "Standard", RoomCharges = 100 },
                new Room { RoomNo = "102", RoomType = "Standard", RoomCharges = 100 },
                new Room { RoomNo = "201", RoomType = "Deluxe", RoomCharges = 180 },
                new Room { RoomNo = "202", RoomType = "Deluxe", RoomCharges = 180 },
                new Room { RoomNo = "301", RoomType = "Suite", RoomCharges = 300 });
        }

        if (!await db.Halls.AnyAsync())
        {
            db.Halls.AddRange(
                new Hall { HallName = "Crystal Hall", Charges = 500 },
                new Hall { HallName = "Emerald Hall", Charges = 750 });
        }

        if (!await db.Gardens.AnyAsync())
            db.Gardens.Add(new Garden { Charges = 400 });

        if (!await db.ExtraBeds.AnyAsync())
            db.ExtraBeds.Add(new ExtraBed { Charges = 25 });

        if (!await db.Dishes.AnyAsync())
        {
            db.Dishes.AddRange(
                new Dish { DishName = "Margherita Pizza", Category = "Main Course", Rate = 12 },
                new Dish { DishName = "Caesar Salad", Category = "Starter", Rate = 8 },
                new Dish { DishName = "Grilled Salmon", Category = "Main Course", Rate = 22 },
                new Dish { DishName = "Tiramisu", Category = "Dessert", Rate = 7 });
        }

        if (!await db.Guests.AnyAsync())
        {
            db.Guests.AddRange(
                new Guest { GuestID = "G-0001", GuestName = "John Smith", Address = "12 Oak Street", City = "Springfield", ContactNo = "555-1234", IDType = "Passport", IDNumber = "P1234567" },
                new Guest { GuestID = "G-0002", GuestName = "Maria Garcia", Address = "44 Elm Avenue", City = "Riverside", ContactNo = "555-5678", IDType = "Driving License", IDNumber = "DL998877" });
        }

        await db.SaveChangesAsync();
    }
}
