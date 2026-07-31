namespace HotelManagement.Core.DTOs;

public record RegisterRequest(string Username, string Password, string UserType);
public record LoginRequest(string Username, string Password, string UserType);
public record LoginResponse(int Id, string Username, string UserType, string Token);
public record ChangePasswordRequest(string Username, string OldPassword, string NewPassword);
