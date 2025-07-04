using JWT_AuthApi.Models.Auth;

namespace JWT_AuthApi.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
        int? ValidateJwtToken(string token);
        RefreshToken GenerateRefreshToken(string ipAddress);
    }
}
