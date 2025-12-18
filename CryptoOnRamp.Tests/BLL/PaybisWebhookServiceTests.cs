using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace CryptoOnRamp.Tests.BLL;

public class PaybisWebhookServiceTests : BaseIntegrationTest
{
    [Fact]
    public async Task ProcessAsync_SetsExternalId_And_AdvancesStatus()
    {
        // Arrange
        var svc = Scope.ServiceProvider.GetRequiredService<IPaybisWebhookService>();

        var txId = 142; // в примере PartnerUserId = "142"
        var requestId = "dfeed716-a4ba-4217-8675-fa8ea91b0e41";

        // ⚠️ Важно: в тестовой БД должна быть
        // TransactionDb с Id = 142
        // CheckoutSessionDb c TransactionId=142, Ramp="paybis",
        // PartnerContext="142:pb", ExternalId=NULL

        var rawBody = @"
            {
              ""timestamp"": 1759146909,
              ""event"": ""TRANSACTION_STATUS_CHANGED"",
              ""data"": {
                ""requestId"": ""dfeed716-a4ba-4217-8675-fa8ea91b0e41"",
                ""partnerUserId"": ""142"",
                ""userEmail"": ""realtimeinfo.ro@gmail.com"",
                ""userIp"": ""194.42.157.65"",
                ""quote"": {
                  ""quoteId"": ""defc8da1-db35-4e0b-b464-0ebf0828a303"",
                  ""amountTo"": { ""amount"": ""26.576486"", ""currency"": ""USDC-SEPOLIA"" },
                  ""amountFrom"": { ""amount"": ""25.00000000"", ""currency"": ""EUR"" },
                  ""amountReceived"": { ""amount"": ""22.70"", ""currency"": ""EUR"" },
                  ""currencyCodeTo"": ""USDC-SEPOLIA"",
                  ""currencyCodeFrom"": ""EUR"",
                  ""fees"": {
                    ""currency"": ""EUR"",
                    ""network_fee"": ""0.37"",
                    ""service_fee"": ""1.93"",
                    ""partner_fee"": ""0.13"",
                    ""total_fee"": ""2.30""
                  },
                  ""feesInUsd"": {
                    ""network_fee"": ""0.43"",
                    ""service_fee"": ""2.26"",
                    ""partner_fee"": ""0.15"",
                    ""total_fee"": ""2.70""
                  },
                  ""directionChange"": ""from"",
                  ""expiresAt"": ""2025-09-29T12:10:10+0000""
                },
                ""transaction"": {
                  ""status"": ""completed"",
                  ""rejectReason"": null,
                  ""invoice"": ""PBQA25092920848TX2"",
                  ""flow"": ""buyCrypto"",
                  ""createdAt"": ""2025-09-29T11:53:33+0000"",
                  ""statusUpdatedAt"": ""2025-09-29T11:55:08+0000""
                },
                ""payment"": {
                  ""id"": ""media-square-credit-card"",
                  ""name"": ""Credit/Debit Card"",
                  ""card"": {
                    ""source"": ""direct"",
                    ""cardholderName"": ""CARDHOLDER APPROVED LOCAL"",
                    ""maskedCardNumber"": ""400293******7895"",
                    ""expirationDate"": ""12/2025"",
                    ""billingAddress"": {
                      ""country"": { ""name"": ""Spain"", ""code"": ""ES"" },
                      ""state"": null,
                      ""zip"": ""1231"",
                      ""city"": ""test"",
                      ""address"": ""test""
                    }
                  },
                  ""errorCode"": null
                },
                ""payout"": {
                  ""id"": ""usd-coin"",
                  ""name"": ""USD Coin"",
                  ""transaction_hash"": ""0x84581c8e32d89ede6eb7955be8f1355efea5059ea0136f5696b870f83af50c14"",
                  ""explorer_link"": ""https://sepolia.etherscan.io/tx/0x84581c8e32d89ede6eb7955be8f1355efea5059ea0136f5696b870f83af50c14"",
                  ""destinationWalletAddress"": ""0x9751ee3dBDa308A95059F5888ec3841CBF4a50c4""
                },
                ""amountFrom"": { ""amount"": ""25.00"", ""currency"": ""EUR"" },
                ""amountTo"": { ""amount"": ""26.576486"", ""currency"": ""USDC"" },
                ""promoCode"": null
              },
              ""meta"": {
                ""assets"": [
                  {
                    ""currency"": ""USDC"",
                    ""currencyCode"": ""USDC-SEPOLIA"",
                    ""displayName"": ""USD Coin (Sepolia)"",
                    ""blockchain"": ""ethereum"",
                    ""network"": ""sepolia"",
                    ""decimals"": 6,
                    ""tokenContract"": ""0x1c7D4B196Cb0C7B01d743Fbc6116a902379C7238"",
                    ""hasDestinationTag"": false
                  }
                ]
              }
            }";

        // Act
        await svc.ProcessAsync(rawBody, correlationId: Guid.NewGuid().ToString("N"));

        // Assert
        var sessions = Scope.ServiceProvider.GetRequiredService<ICheckoutSessionRepository>();
        var txRepo = Scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

        var session = await sessions.GetFirstOrDefaultAsync(
            s => s.TransactionId == txId && s.Ramp == "paybis" && s.PartnerContext == $"{txId}:pb");

        Assert.NotNull(session);
        Assert.Equal(requestId, session!.ExternalId); // ExternalId должен обновиться
        Assert.True(IsForward(session.Status, TransactionStatusDb.Completed));

        var tx = await txRepo.GetByIdAsync(txId);
        Assert.NotNull(tx);
        Assert.True(IsForward(tx!.Status, TransactionStatusDb.Completed));
    }

    // ——— helpers ———

    /// <summary>
    /// Считает Base64(HMACSHA256(rawBody)) при условии, что keyBase64 — это Base64-кодированный HMAC-ключ.
    /// </summary>
    private static string ComputeB64HmacSha256_WithBase64Key(string body, string keyBase64)
    {
        var key = Convert.FromBase64String(keyBase64);
        var data = Encoding.UTF8.GetBytes(body);
        var hash = HMACSHA256.HashData(key, data);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Монотонное сравнение статусов (тот же порядок, что и в сервисе).
    /// </summary>
    private static bool IsForward(TransactionStatusDb current, TransactionStatusDb expectAtLeast)
        => Order(current) <= Order(expectAtLeast);

    private static int Order(TransactionStatusDb s) => s switch
    {
        TransactionStatusDb.Issued => 0,
        TransactionStatusDb.Pending => 1,
        TransactionStatusDb.Completed => 2,
        TransactionStatusDb.PayoutCompleted => 3,
        TransactionStatusDb.Failed => 99,
        _ => 0
    };
}
