using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogApi.Data;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services;

public class AuthService : IAuthService
{
    private readonly BlogDbContext _context;
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;

    public AuthService(
        BlogDbContext context,
        ITotpService totpService,
        IEncryptionService encryptionService,
        IConfiguration configuration)
    {
        _context = context;
        _totpService = totpService;
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expirationHours),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> VerifyTotpAsync(User user, string code)
    {
        if (string.IsNullOrEmpty(user.TotpSecret))
            return false;

        try
        {
            var decryptedSecret = _encryptionService.Decrypt(user.TotpSecret);
            return _totpService.ValidateTotp(decryptedSecret, code);
        }
        catch
        {
            return false;
        }
    }
}
