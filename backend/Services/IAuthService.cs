using BlogApi.Models;

namespace BlogApi.Services;

public interface IAuthService
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<string> GenerateJwtTokenAsync(User user);
    Task<bool> VerifyTotpAsync(User user, string code);
}
