namespace CryptoOnRamp.BLL.Interfaces;

public interface IEncryptionHelper
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
