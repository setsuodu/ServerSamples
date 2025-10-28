using EFCore.DTOs.Auth;

namespace EFCore.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}