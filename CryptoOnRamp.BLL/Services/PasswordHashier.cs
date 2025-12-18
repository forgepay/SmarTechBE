using CryptoOnRamp.BLL.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace CryptoOnRamp.BLL.Services;

public class PasswordHashier : IPasswordHashier
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 11);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }

    public string GenerateSecurePassword(int length = 8)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}";
        var res = new StringBuilder();
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (res.Length < length)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }
        }

        return res.ToString();
    }
}