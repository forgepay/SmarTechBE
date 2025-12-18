using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IFeeService
{
    Task<GetFeeSettingsResponse> GetSettingsAsync();
    Task<bool> UpdateAsync(FeeUpdateRequest req);
    Task<(decimal sa, decimal ag)> ResolvePercentsForAgentAsync(int? agentUserId);
}
