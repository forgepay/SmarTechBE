using System.Text;

namespace MicPic.Infrastructure.Serialization;

public static class AppHashier
{
    public static byte[] SHA256<T>(T value) =>
        System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(AppJsonSerializer.Serialize(value)));

    public static string Hash<T>(T value) =>
        Convert.ToHexString(SHA256(value));
}