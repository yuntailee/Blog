using System.Security.Cryptography;
using System.Text;

namespace BlogApi.Services;

public class EncryptionService : IEncryptionService
{
    private readonly string _encryptionKey;
    private readonly byte[] _keyBytes;

    public EncryptionService(IConfiguration configuration)
    {
        _encryptionKey = configuration["Totp:EncryptionKey"] 
            ?? throw new InvalidOperationException("TOTP Encryption Key not configured");
        
        // Derive a 32-byte key from the encryption key
        using var sha256 = SHA256.Create();
        _keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV and cipher text
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract IV
        var iv = new byte[aes.IV.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        // Extract cipher text
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
