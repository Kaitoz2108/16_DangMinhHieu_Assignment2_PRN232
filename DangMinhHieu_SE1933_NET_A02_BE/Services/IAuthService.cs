using FUNewsManagement_BLL.DTOs;

namespace FUNewsManagement_BLL.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
    SystemAccountDto? GetProfile(short accountId, string role);
    (bool Success, string Message) UpdateProfile(short accountId, string role, ProfileUpdateRequest request);
    string GenerateJwtToken(short accountId, string name, string email, string role);
}
