using CryptoOnRamp.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace CryptoOnRamp.BLL.Services;

public class EncryptionHelper : IEncryptionHelper
{
    private readonly byte[] _key; // 32 bytes for AES-256
    private readonly byte[] _iv;  // 16 bytes initialization vector

    public EncryptionHelper(IConfiguration config)
    {
        // Get from appsettings.json or secrets store
        var secretKey = config["Encryption:Key"] ?? throw new ApplicationException("Encryption:Key is not set"); // base64 encoded key
        var iv = config["Encryption:IV"] ?? throw new ApplicationException("Encryption:IV is not set");         // base64 encoded IV

        _key = Convert.FromBase64String(secretKey);
        _iv = Convert.FromBase64String(iv);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        var cipherBytes = Convert.FromBase64String(cipherText);
        var decrypted = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}
