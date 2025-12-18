using MicPic.Infrastructure.Serialization;

namespace MicPic.Infrastructure.Security.Internal;

internal sealed class AppPasswordHasher : IAppPasswordHasher
{
    public string HashPassword(string password)
    {
        return AppHashier.Hash(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return string.Equals(hashedPassword, HashPassword(password), StringComparison.Ordinal);
    }
}
