using System.Text.RegularExpressions;

namespace HotelManagement.Core.Services;

/// <summary>
/// Input validation logic derived from VB.NET source code KeyPress events and validation checks.
/// </summary>
public class ValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z][\w\.\-]{2,28}[a-zA-Z0-9]@[a-zA-Z0-9][\w\.\-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$",
        RegexOptions.Compiled);

    public bool IsNumericOnly(string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(c => char.IsDigit(c));
    }

    public bool IsAlphaOnly(string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(c => char.IsLetter(c) || c == ' ');
    }

    public bool IsDecimalInput(string input)
    {
        return decimal.TryParse(input, out _);
    }

    public bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && EmailRegex.IsMatch(email);
    }

    public bool IsNonEmpty(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public bool IsValidDateRange(DateTime dateIn, DateTime dateOut)
    {
        return dateOut.Date >= dateIn.Date;
    }

    public bool IsPaymentValid(decimal totalPaid, decimal grandTotal)
    {
        return totalPaid <= grandTotal;
    }

    public bool IsDeductionValid(decimal deduction, decimal advance)
    {
        return deduction <= advance;
    }

    public bool IsNetPayPositive(decimal netPay)
    {
        return netPay >= 0;
    }

    public bool ContainsSqlInjection(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        string lower = input.ToLowerInvariant();
        string[] patterns = { "'", "--", ";", "drop ", "delete ", "insert ", "update ", "exec ", "execute ", "xp_", "sp_", "/*", "*/" };
        return patterns.Any(p => lower.Contains(p));
    }

    public (bool IsValid, List<string> Errors) ValidateGuest(string guestName, string address, string city, string contactNo, string idType, string idNumber)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(guestName)) errors.Add("Guest Name is required");
        if (!IsNonEmpty(address)) errors.Add("Address is required");
        if (!IsNonEmpty(city)) errors.Add("City is required");
        if (!IsNonEmpty(contactNo)) errors.Add("Contact No is required");
        else if (!IsNumericOnly(contactNo)) errors.Add("Contact No must be numeric");
        if (!IsNonEmpty(idType)) errors.Add("ID Type is required");
        if (!IsNonEmpty(idNumber)) errors.Add("ID Number is required");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateRoom(string roomNo, string roomType, string roomCharges)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(roomNo)) errors.Add("Room No is required");
        if (!IsNonEmpty(roomType)) errors.Add("Room Type is required");
        if (!IsNonEmpty(roomCharges)) errors.Add("Room Charges is required");
        else if (!IsNumericOnly(roomCharges)) errors.Add("Room Charges must be numeric");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateEmployee(string name, string address, string mobileNo, string email, string department, string designation, string salary)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(name)) errors.Add("Employee Name is required");
        else if (!IsAlphaOnly(name)) errors.Add("Employee Name must contain only letters");
        if (!IsNonEmpty(address)) errors.Add("Address is required");
        if (!IsNonEmpty(mobileNo)) errors.Add("Mobile No is required");
        if (!IsNonEmpty(email)) errors.Add("Email is required");
        else if (!IsValidEmail(email)) errors.Add("Invalid email format");
        if (!IsNonEmpty(department)) errors.Add("Department is required");
        else if (!IsAlphaOnly(department)) errors.Add("Department must contain only letters");
        if (!IsNonEmpty(designation)) errors.Add("Designation is required");
        else if (!IsAlphaOnly(designation)) errors.Add("Designation must contain only letters");
        if (!IsNonEmpty(salary)) errors.Add("Salary is required");
        else if (!IsDecimalInput(salary)) errors.Add("Salary must be a valid number");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateHotelInfo(string hotelName, string address, string contactNo)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(hotelName)) errors.Add("Hotel Name is required");
        if (!IsNonEmpty(address)) errors.Add("Address is required");
        if (!IsNonEmpty(contactNo)) errors.Add("Contact No is required");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateCheckIn(string roomNo, string guestId, DateTime dateIn, DateTime dateOut, decimal totalPaid, decimal grandTotal)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(roomNo)) errors.Add("Room must be selected");
        if (!IsNonEmpty(guestId)) errors.Add("Guest must be selected");
        if (!IsValidDateRange(dateIn, dateOut)) errors.Add("Check-out date must be on or after check-in date");
        if (!IsPaymentValid(totalPaid, grandTotal)) errors.Add("Total Paid cannot exceed Grand Total");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateCheckOut(string currencyId, string guestId, decimal totalPaid, decimal grandTotal)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(currencyId)) errors.Add("Currency must be selected");
        if (!IsNonEmpty(guestId)) errors.Add("Guest must be selected");
        if (!IsPaymentValid(totalPaid, grandTotal)) errors.Add("Total Paid cannot exceed Grand Total");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateAttendance(string employeeId, string status)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(employeeId)) errors.Add("Employee ID is required");
        if (!IsNonEmpty(status)) errors.Add("Status is required");
        else if (status != "P" && status != "A") errors.Add("Status must be P or A");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidatePayment(string employeeId, string overtimeRate, string modeOfPayment, decimal deduction, decimal advance, decimal netPay)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(employeeId)) errors.Add("Employee ID is required");
        if (!IsNonEmpty(overtimeRate)) errors.Add("Overtime Rate is required");
        if (!IsNonEmpty(modeOfPayment)) errors.Add("Mode of Payment is required");
        if (!IsDeductionValid(deduction, advance)) errors.Add("Deduction cannot exceed Advance");
        if (!IsNetPayPositive(netPay)) errors.Add("Net Pay must be greater than or equal to 0");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidatePurchaseInventory(string productName, string category, string transactionType, string partyName, string quantity, string unit)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(productName)) errors.Add("Product Name is required");
        if (!IsNonEmpty(category)) errors.Add("Category is required");
        if (!IsNonEmpty(transactionType)) errors.Add("Transaction Type is required");
        if (!IsNonEmpty(partyName)) errors.Add("Party Name is required");
        if (!IsNonEmpty(quantity)) errors.Add("Quantity is required");
        else if (!IsNumericOnly(quantity)) errors.Add("Quantity must be numeric");
        if (!IsNonEmpty(unit)) errors.Add("Unit is required");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateStock(string liquorName, string noOfBottles, string volume)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(liquorName)) errors.Add("Liquor Name is required");
        if (!IsNonEmpty(noOfBottles)) errors.Add("No of Bottles is required");
        else if (!IsNumericOnly(noOfBottles)) errors.Add("No of Bottles must be numeric");
        if (!IsNonEmpty(volume)) errors.Add("Volume is required");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateStockBeer(string beerId, string noOfBottles)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(beerId)) errors.Add("Beer must be selected");
        if (!IsNonEmpty(noOfBottles)) errors.Add("No of Bottles is required");
        else if (!IsNumericOnly(noOfBottles)) errors.Add("No of Bottles must be numeric");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateCurrency(string currencyName)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(currencyName)) errors.Add("Currency Name is required");
        return (errors.Count == 0, errors);
    }

    public (bool IsValid, List<string> Errors) ValidateTaxInfo(string salary, string taxPercentage)
    {
        var errors = new List<string>();
        if (!IsNonEmpty(salary)) errors.Add("Salary range is required");
        if (!IsNonEmpty(taxPercentage)) errors.Add("Tax Percentage is required");
        else if (!IsDecimalInput(taxPercentage)) errors.Add("Tax Percentage must be a valid number");
        return (errors.Count == 0, errors);
    }
}
