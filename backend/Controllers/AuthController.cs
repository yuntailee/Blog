using System.Security.Claims;
using BlogApi.Data;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITotpService _totpService;
    private readonly IEncryptionService _encryptionService;
    private readonly BlogDbContext _context;

    public AuthController(
        IAuthService authService,
        ITotpService totpService,
        IEncryptionService encryptionService,
        BlogDbContext context)
    {
        _authService = authService;
        _totpService = totpService;
        _encryptionService = encryptionService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Code))
        {
            return BadRequest(new { message = "Username and code are required" });
        }

        var user = await _authService.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        if (string.IsNullOrEmpty(user.TotpSecret))
        {
            return BadRequest(new { message = "TOTP not set up. Please set up TOTP first." });
        }

        var isValid = await _authService.VerifyTotpAsync(user, request.Code);
        if (!isValid)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = await _authService.GenerateJwtTokenAsync(user);
        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            }
        });
    }

    [HttpGet("setup")]
    [Authorize]
    public async Task<IActionResult> GetTotpSetup()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // If TOTP is already set up, return existing secret info
        if (!string.IsNullOrEmpty(user.TotpSecret))
        {
            return BadRequest(new { message = "TOTP is already set up" });
        }

        // Generate new TOTP secret
        var secret = _totpService.GenerateSecret();
        var qrCodeUri = _totpService.GenerateQrCodeUri(user.Username, secret);

        return Ok(new
        {
            secret,
            qrCodeUri
        });
    }

    [HttpPost("verify-setup")]
    [Authorize]
    public async Task<IActionResult> VerifyTotpSetup([FromBody] VerifySetupRequest request)
    {
        if (string.IsNullOrEmpty(request.Secret) || string.IsNullOrEmpty(request.Code))
        {
            return BadRequest(new { message = "Secret and code are required" });
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Verify the code
        var isValid = _totpService.ValidateTotp(request.Secret, request.Code);
        if (!isValid)
        {
            return BadRequest(new { message = "Invalid verification code" });
        }

        // Encrypt and save the secret
        user.TotpSecret = _encryptionService.Encrypt(request.Secret);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "TOTP setup completed successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // JWT is stateless, so logout is mainly handled on the client side
        // In the future, we could implement token blacklisting here
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// 首次設定 TOTP（不需要認證，僅限於沒有 TOTP Secret 的使用者）
    /// </summary>
    [HttpPost("initial-setup")]
    public async Task<IActionResult> InitialTotpSetup([FromBody] InitialSetupRequest request)
    {
        if (string.IsNullOrEmpty(request.Username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        var user = await _authService.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // 只允許沒有 TOTP Secret 的使用者使用此端點
        if (!string.IsNullOrEmpty(user.TotpSecret))
        {
            return BadRequest(new { message = "TOTP is already set up. Please use /api/auth/setup instead." });
        }

        // Generate new TOTP secret
        var secret = _totpService.GenerateSecret();
        var qrCodeUri = _totpService.GenerateQrCodeUri(user.Username, secret);

        return Ok(new
        {
            secret,
            qrCodeUri,
            username = user.Username
        });
    }

    /// <summary>
    /// 驗證首次 TOTP 設定（不需要認證，完成後會自動登入）
    /// </summary>
    [HttpPost("initial-verify")]
    public async Task<IActionResult> InitialVerifySetup([FromBody] InitialVerifyRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || 
            string.IsNullOrEmpty(request.Secret) || 
            string.IsNullOrEmpty(request.Code))
        {
            return BadRequest(new { message = "Username, secret and code are required" });
        }

        var user = await _authService.GetUserByUsernameAsync(request.Username);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!string.IsNullOrEmpty(user.TotpSecret))
        {
            return BadRequest(new { message = "TOTP is already set up" });
        }

        // Verify the code
        var isValid = _totpService.ValidateTotp(request.Secret, request.Code);
        if (!isValid)
        {
            return BadRequest(new { message = "Invalid verification code" });
        }

        // Encrypt and save the secret
        user.TotpSecret = _encryptionService.Encrypt(request.Secret);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate JWT token for immediate login
        var token = await _authService.GenerateJwtTokenAsync(user);
        
        return Ok(new 
        { 
            message = "TOTP setup completed successfully",
            token,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            }
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class VerifySetupRequest
{
    public string Secret { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class InitialSetupRequest
{
    public string Username { get; set; } = string.Empty;
}

public class InitialVerifyRequest
{
    public string Username { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
