using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL.Models;

public record GetFeeSettingsResponse(
    decimal? SuperAgentPercent,                  //
    List<UserFeeInfo> SuperAgentFees, //for Admin
    List<UserFeeInfo> AgentFees            // Admin: all; SuperAgent: only his agents
);

public record UserFeeInfo(int UserId, decimal Percent); 

public record FeeUpdateRequest(
    FeeType Type,
    int? TargetId,       // Processor -> null, SuperAgent -> superAgentId, Agent -> agentId
    decimal Percent
);

public record PayoutRequest(string? ToWallet, decimal? Amount, PayoutType Type );
public record PayoutResponse(string TxHash, decimal NetAmount, decimal FeesDeducted);
