namespace CryptoOnRamp.BLL.Models;

public record GlobalStatsDto(
    int TotalIssued,
    int TotalCompleted,
    int TotalPending,
    int TotalFailed,
    decimal TotalVolume
);
