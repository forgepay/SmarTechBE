namespace CryptoOnRamp.BLL.Models;

public record StatsDto(
    int TotalTransactions,
    decimal TotalVolume,
    decimal TotalProcessorFees,
    decimal TotalSuperAgentFees,
    decimal TotalAgentFees,
    decimal TotalNetPayouts
);
