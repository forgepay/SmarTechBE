using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOnRamp.BLL.Helpers;

public static class AesEncryption
{
    public static string Encrypt<T>(string privatekey, T model)
    {
        string plaintext = JsonSerializer.Serialize(model);
        Console.WriteLine(plaintext);

        var rt32 = new String(privatekey.Take(32).ToArray());
        var rt16 = new String(privatekey.Take(16).ToArray());

        var key = Encoding.UTF8.GetBytes(rt32);
        var iv = Encoding.UTF8.GetBytes(rt16);

        // Encrypt
        byte[] ciphertext = AesEncryption.Encrypt(plaintext, key, iv);
        string encryptedText = Convert.ToBase64String(ciphertext);

        return encryptedText;
    }

    public static T Decrypt<T>(string privatekey, string encryptedText)
    {
        var rt32 = new String(privatekey.Take(32).ToArray());
        var rt16 = new String(privatekey.Take(16).ToArray());

        var key = Encoding.UTF8.GetBytes(rt32);
        var iv = Encoding.UTF8.GetBytes(rt16);

        byte[] bytes = Convert.FromBase64String(encryptedText);
        string decryptedText = AesEncryption.Decrypt(bytes, key, iv);

        return JsonSerializer.Deserialize<T>(decryptedText) ?? throw new ArgumentOutOfRangeException(nameof(encryptedText), "Decrypted content can't be deserialized.");
    }

    private static byte[] Encrypt(string plaintext, byte[] key, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            byte[] encryptedBytes;
            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                }
                encryptedBytes = msEncrypt.ToArray();
            }
            return encryptedBytes;
        }
    }
    private static string Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            byte[] decryptedBytes;
            using (var msDecrypt = new System.IO.MemoryStream(ciphertext))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var msPlain = new System.IO.MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlain);
                        decryptedBytes = msPlain.ToArray();
                    }
                }
            }
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}

