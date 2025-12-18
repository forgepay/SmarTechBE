namespace CryptoOnRamp.BLL.Models;

public sealed class PaybisWidgetPetition
{
    public string PartnerUserId { get; set; } = ""; //  userId/agentId
    public PaybisTransactionFlow Flow { get; set; } = PaybisTransactionFlow.buyCrypto;

    //mine wallet
    public string CryptoAddress { get; set; } = "";

    public string CurrencyCodeFrom { get; set; } = "USD";
    public string CurrencyCodeTo { get; set; } = "USDC-ETHEREUM";

    public decimal AmountFrom { get; set; } = 0m;
    public decimal AmountTo { get; set; } = 0m;

    public string? Locale { get; set; } = "en";
    public string? Email { get; set; }
}

public enum PaybisTransactionFlow { buyCrypto, SellCrypto }
