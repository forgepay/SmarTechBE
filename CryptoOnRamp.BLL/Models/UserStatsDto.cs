namespace CryptoOnRamp.BLL.Models;

public record UserStatsDto(
    int UserId,
    int TotalIssued,
    int TotalCompleted,
    int TotalPending,
    int TotalFailed,
    decimal TotalVolume,
    List<UserStatsItemDto> SubUsers
);

public record UserStatsItemDto(int UserId, GlobalStatsDto Stats);
