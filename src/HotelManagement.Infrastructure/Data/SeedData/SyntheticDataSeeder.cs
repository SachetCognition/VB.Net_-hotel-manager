using HotelManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Data.SeedData;

public static class SyntheticDataSeeder
{
    public static async Task SeedAsync(HotelDbContext context)
    {
        if (await context.Users.AnyAsync())
            return; // Already seeded

        // Seed default Admin user
        context.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            UserType = "Admin"
        });

        // Seed default regular user
        context.Users.Add(new User
        {
            Username = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
            UserType = "User"
        });

        // Seed currencies
        var currencies = new[]
        {
            new CurrencySet { CurrencyName = "INR", Symbol = "₹" },
            new CurrencySet { CurrencyName = "USD", Symbol = "$" },
            new CurrencySet { CurrencyName = "EUR", Symbol = "€" },
            new CurrencySet { CurrencyName = "GBP", Symbol = "£" }
        };
        context.CurrencySets.AddRange(currencies);

        // Seed rooms
        var rooms = new[]
        {
            new Room { RoomNo = "101", RoomType = "AC", RoomCharges = 2000m },
            new Room { RoomNo = "102", RoomType = "AC", RoomCharges = 2000m },
            new Room { RoomNo = "103", RoomType = "Non-AC", RoomCharges = 1200m },
            new Room { RoomNo = "104", RoomType = "Non-AC", RoomCharges = 1200m },
            new Room { RoomNo = "201", RoomType = "Deluxe", RoomCharges = 3500m },
            new Room { RoomNo = "202", RoomType = "Deluxe", RoomCharges = 3500m },
            new Room { RoomNo = "301", RoomType = "Suite", RoomCharges = 5000m },
            new Room { RoomNo = "302", RoomType = "Suite", RoomCharges = 5000m }
        };
        context.Rooms.AddRange(rooms);

        // Seed hotel info
        context.HotelInfos.Add(new HotelInfo
        {
            HotelName = "Grand Hotel",
            Address = "123 Main Street",
            City = "Mumbai",
            State = "Maharashtra",
            ZipCode = "400001",
            Phone = "022-12345678",
            Email = "info@grandhotel.com",
            Website = "www.grandhotel.com",
            TIN = "TIN123456",
            ServiceTaxNo = "ST123456"
        });

        // Seed halls
        context.Halls.Add(new Hall
        {
            HallName = "Grand Ballroom",
            Charges = 25000m,
            Description = "Main event hall with 500 capacity"
        });

        // Seed gardens
        context.Gardens.Add(new Garden
        {
            GardenName = "Rose Garden",
            Charges = 15000m,
            Description = "Outdoor garden venue with 200 capacity"
        });

        // Seed extra bed
        context.ExtraBeds.Add(new ExtraBed
        {
            BedType = "Standard",
            Charges = 500m
        });

        // Seed tax information
        context.TaxInformations.AddRange(
            new TaxInformation { TaxName = "Service Tax", TaxPercentage = 14.5m, Description = "Service tax on room charges" },
            new TaxInformation { TaxName = "Luxury Tax", TaxPercentage = 12.0m, Description = "Luxury tax on room charges" }
        );

        // Seed sample dishes
        context.Dishes.AddRange(
            new Dish { DishName = "Butter Chicken", Category = "Main Course", Rate = 350m },
            new Dish { DishName = "Paneer Tikka", Category = "Starter", Rate = 250m },
            new Dish { DishName = "Dal Makhani", Category = "Main Course", Rate = 200m },
            new Dish { DishName = "Biryani", Category = "Main Course", Rate = 300m },
            new Dish { DishName = "Gulab Jamun", Category = "Dessert", Rate = 100m }
        );

        // Seed sample guests
        context.Guests.AddRange(
            new Guest { GuestID = "G-100001", GuestName = "John Smith", Address = "456 Park Avenue", City = "Delhi", ContactNo = "9876543210", IDType = "Passport", IDNumber = "P1234567" },
            new Guest { GuestID = "G-100002", GuestName = "Jane Doe", Address = "789 Lake Road", City = "Mumbai", ContactNo = "9876543211", IDType = "Aadhar", IDNumber = "1234-5678-9012" }
        );

        // Seed sample employees
        context.Employees.AddRange(
            new Employee { EmployeeID = "E-100001", EmployeeName = "Raj Kumar", Address = "123 Worker Lane", MobileNo = "9876543220", Email = "raj@hotel.com", BloodGroup = "O+", Gender = "Male", Department = "Front Desk", Designation = "Receptionist", DateOfJoining = new DateTime(2024, 1, 15), Salary = 25000m, BasicWorkingTime = "08:00:00" },
            new Employee { EmployeeID = "E-100002", EmployeeName = "Priya Sharma", Address = "456 Staff Colony", MobileNo = "9876543221", Email = "priya@hotel.com", BloodGroup = "A+", Gender = "Female", Department = "Housekeeping", Designation = "Supervisor", DateOfJoining = new DateTime(2024, 3, 1), Salary = 20000m, BasicWorkingTime = "08:00:00" }
        );

        await context.SaveChangesAsync();
    }
}
