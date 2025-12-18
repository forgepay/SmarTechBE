namespace CryptoOnRamp.BLL.Interfaces;

public interface IEmailService
{
    Task SendResetPasswordAsync(string? email, string hash, CancellationToken none);
}
