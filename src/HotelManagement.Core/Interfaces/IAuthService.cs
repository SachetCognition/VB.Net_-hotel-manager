using HotelManagement.Core.DTOs;

namespace HotelManagement.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);
    Task<IEnumerable<object>> GetLoginActivityAsync();
}
