using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace CryptoOnRamp.Tests.BLL;

public class LinkGenerationOrchestratorIntegrationTests : BaseIntegrationTest
{
    [Fact]
    public async Task GenerateAsync_CreatesParentTx_PutsPaybisFirst_SavesSessions_Max6()
    {
        // ---------- Arrange ----------
        var orchestrator = Scope.ServiceProvider.GetRequiredService<ILinkGenerationOrchestrator>();
        var users = Scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var telegramUsers = Scope.ServiceProvider.GetRequiredService<ITelegramUserRepository>();
        var txRepo = Scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var sessionsRepo = Scope.ServiceProvider.GetRequiredService<ICheckoutSessionRepository>();
        var paybisOptions = Scope.ServiceProvider.GetRequiredService<IOptions<PaybisOptions>>().Value;

        // Сидируем (если надо) юзера-агента, для которого генерим ссылки
        // Используем фиксированный Id, чтобы потом проще проверять
        var agentId = 777_001;
        var tgId = 777001;

        var existing = await users.GetFirstOrDefaultAsync(u => u.Id == agentId);
        if (existing is null)
        {
            await users.InsertAsync(new UserDb
            {
                Id = agentId,
                Name = "tg_777001",
                Email = "agent@test.local",
                Phone = "+3530000000",
                UsdcWallet = "0x1111111111111111111111111111111111111111",
                Role = UserRoleDb.Agent,
                CreatedAt = DateTime.UtcNow,
                RegistrationStep = RegistrationStep.Completed,
                // TelegramId = 777001,
                Country = "EUR" // у тебя тут хранится валюта
            });

            await users.SaveAsync();
        }

        var tgExisting = await telegramUsers.GetFirstOrDefaultAsync(tgu => tgu.TelegramId == tgId && tgu.UserId == agentId);
        if (tgExisting is null)
        {
            await telegramUsers.InsertAsync(new TelegramUserDb
            {
                TelegramId = tgId,
                UserId = agentId,
            });

            await telegramUsers.SaveAsync();
        }

        var input = new LinkGenerationInput
        {
            RequestorUserId = agentId,
            TargetUserId = agentId,
            FiatCurrency = "EUR",
            Amount = 25m,
            Email = "agent@test.local"
        };

        // ---------- Act ----------
        var result = await orchestrator.GenerateAsync(input, CancellationToken.None);

        // ---------- Assert ----------
        // 1) Вернулся валидный результат
        Assert.True(result.TransactionId > 0);
        Assert.False(string.IsNullOrWhiteSpace(result.UniqueWalletAddress));
        Assert.False(string.IsNullOrWhiteSpace(result.EncryptedPrivateKey));
        Assert.NotNull(result.PaymentLinks);

        // 2) В БД есть parent-транзакция
        var tx = await txRepo.GetByIdAsync(result.TransactionId);
        Assert.NotNull(tx);
        Assert.Equal(agentId, tx!.UserId);
        Assert.Equal(TransactionStatusDb.Issued, tx.Status);

        // 3) Сохранены сессии (>=1), максимум 6
        var sessions = await sessionsRepo.GetAsync(x => x.TransactionId == result.TransactionId);
        Assert.NotNull(sessions);
        Assert.NotEmpty(sessions);
        Assert.True(sessions.Count <= 6, "должно быть не больше 6 ссылок: 1 Paybis + до 5 Onramper");

        // 4) Первая сессия — Paybis, PartnerContext = "{txId}:pb"
        var first = sessions[0];
        Assert.Equal("paybis", first.Ramp);
        Assert.Equal($"{result.TransactionId}:pb", first.PartnerContext);
        Assert.Equal(TransactionStatusDb.Issued, first.Status);

        // 5) Первая ссылка в ответе — действительно Paybis (по домену из конфигов)
        Assert.NotEmpty(result.PaymentLinks);
        Assert.StartsWith(paybisOptions.Endpoint, result.PaymentLinks[0], StringComparison.OrdinalIgnoreCase);

        // 6) Проверим формат PartnerContext для Onramper-сессий: "{txId}:{index}"
        // (если Onramper отключён/фейк не подставлен и ссылок нет — это ок)
        var onrampers = sessions.Skip(1).ToList();
        var re = new Regex($@"^{result.TransactionId}:\d+$");
        foreach (var s in onrampers)
        {
            Assert.Matches(re, s.PartnerContext ?? "");
            Assert.False(string.IsNullOrWhiteSpace(s.Url));
            Assert.Equal(TransactionStatusDb.Issued, s.Status);
        }

        // 7) Кросс-проверка: число сохранённых сессий == числу ссылок в ответе
        // (если оркестратор что-то отфильтровал/не сохранил, этот ассерт подсветит рассинхрон)
        Assert.Equal(sessions.Count, result.PaymentLinks.Count);
    }
}
