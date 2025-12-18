using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL.Helpers;

public static class FeeCalculator
{
    public static void ApplyFees(TransactionDb tx)
    {
        if (tx == null)
            throw new ArgumentNullException(nameof(tx));
        if (tx.CryptoAmount == null /*&& tx.FiatAmount == null*/)
            throw new InvalidOperationException("Transaction has no amount to calculate fees from.");

        decimal amount = tx.CryptoAmount.Value;
        decimal saPct = tx.SuperAgentPercent;
        decimal agPct = tx.AgentPercent;
        var role = tx.User.Role;

        static decimal R(decimal v) => Math.Floor(v * 100m) / 100m;

        switch (role)
        {
            case UserRoleDb.Agent:
                {
                    var superAgentFee = R(amount * saPct / 100m);
                    var agentFee = R(amount * agPct / 100m);
                    var netPayout = R(amount - superAgentFee - agentFee);

                    tx.SuperAgentFee = superAgentFee;
                    tx.AgentFee = agentFee;
                    tx.NetPayout = netPayout;
                    break;
                }

            case UserRoleDb.SuperAgent:
                {
                    var netPayout = R(amount * saPct / 100m);
                    var superAgentFee = R(amount - netPayout);

                    tx.SuperAgentFee = superAgentFee;
                    tx.AgentFee = 0;
                    tx.NetPayout = netPayout;
                    break;
                }

            default:
                {
                    tx.SuperAgentFee = 0m;
                    tx.AgentFee = 0;
                    tx.NetPayout = R(amount);
                    break;
                }
        }
    }
}
