using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CryptoOnRamp.Tests.BLL;

public class OnramperWebhookServiceTests : BaseIntegrationTest
{
    [Fact]
    public async Task ProcessAsync_Ok()
    {
        // Arrange
        var historyService = Scope.ServiceProvider.GetRequiredService<IOnramperWebhookService>();
        var options = Scope.ServiceProvider.GetRequiredService<IOptions<OnramperWebhookOptions>>().Value;

        // partnerContext "95:3" => txId=95, sessionIdx=3
        // Если у тебя в БД ещё нет транзакции с Id=95, заранее посей её в фикстуре/сидере.
        var txId = 95;
        var sessionIdx = 3;

        // apiKey кладём такой же, как в AllowedApiKey (если он задан).
        // Если AllowedApiKey пуст, можно оставить любое значение — проверка не активируется.
        var apiKey = string.IsNullOrWhiteSpace(options.AllowedApiKey)
            ? "ignored-in-test"
            : options.AllowedApiKey;

        var payload = new
        {
            transactionId = "01K60JHADPGB7A9TM164BFF39G",
            statusDate = "2025-09-25T13:59:51.140Z",
            status = "paid",
            transactionType = "buy",
            onramp = "banxa",
            onrampTransactionId = "529dcd31c715911bba560fe4402fd9b6",
            sourceCurrency = "eur",
            targetCurrency = "usdc_polygon",
            inAmount = 25,
            outAmount = 26.79,
            apiKey,
            country = "ie",
            paymentMethod = "creditcard",
            transactionHash = (string?)null,
            walletAddress = "0x37fe265C16dAa86493B716712f55eC2da259A8CD",
            partnerContext = $"{txId}:{sessionIdx}",
            isRecurringPayment = false,
            partnerFee = 0
        };

        var rawBody = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });


        var correlationId = Guid.NewGuid().ToString("N");

        // Act
        await historyService.ProcessAsync(rawBody,  correlationId);

        // Assert — по желанию: можно ничего не проверять,
        // либо достать ITransactionRepository и проверить статус/ExternalId.
    }

    private static string ComputeHexHmacSha256(string body, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = Encoding.UTF8.GetBytes(body);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
