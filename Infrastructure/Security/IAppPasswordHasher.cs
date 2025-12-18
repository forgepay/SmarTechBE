namespace MicPic.Infrastructure.Security;

public interface IAppPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
