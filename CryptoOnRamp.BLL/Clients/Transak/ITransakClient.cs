using CryptoOnRamp.BLL.Clients.Transak.Dto;

namespace CryptoOnRamp.BLL.Clients.Transak;

public interface ITransakClient
{
    internal Task<CredentialsDto> GetAccessCredentialsAsync(CancellationToken cancellationToken);
}

