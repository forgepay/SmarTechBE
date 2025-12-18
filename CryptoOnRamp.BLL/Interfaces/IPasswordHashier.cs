namespace CryptoOnRamp.BLL.Interfaces;

public interface IPasswordHashier
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    string GenerateSecurePassword(int length = 32);
}
