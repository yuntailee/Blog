namespace BlogApi.Services;

public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string username, string secret, string issuer = "Blog");
    bool ValidateTotp(string secret, string code);
}
