using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.BLL.Interfaces;

public interface IOnramperService
{
    Task<OnramperCheckoutResponse> CreateCheckoutIntentAsync(
       string onramp, string fiat, string crypto, decimal amount,
       string type, string paymentMethod, string network,
       string? walletAddress, string? walletMemo,
       string partnerContext,
       CancellationToken ct = default);

        Task<List<QuoteItem>> GetBuyQuotesAsync(
        string fiat, string crypto, decimal amount,
        string? country = null, string? network = null, string? paymentMethod = null,
        CancellationToken ct = default);

    (string signContent, string signature) BuildSignature(string? walletAddress, string? walletMemo);

    Task<List<OnramperCheckoutResponse>> CreateBestCheckoutIntentsAsync(
           string fiat, string crypto, decimal amount,
           string? walletAddress, string? walletMemo,
           int transactionId,
           CancellationToken ct = default);
}
