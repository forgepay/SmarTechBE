using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IStatsService
{
    Task<GlobalStatsDto> GetGlobalAsync();
    Task<UserStatsDto> GetUserAsync(int userId);
}
