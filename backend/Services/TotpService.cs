using OtpNet;

namespace BlogApi.Services;

public class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        var secret = KeyGeneration.GenerateRandomKey(20); // 160 bits
        return Base32Encoding.ToString(secret);
    }

    public string GenerateQrCodeUri(string username, string secret, string issuer = "Blog")
    {
        // Format: otpauth://totp/{issuer}:{username}?secret={secret}&issuer={issuer}
        // Frontend will generate QR code from this URI
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(username)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
    }

    public bool ValidateTotp(string secret, string code)
    {
        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);
            
            // Validate with time window tolerance (±1 step = ±30 seconds)
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }
}
