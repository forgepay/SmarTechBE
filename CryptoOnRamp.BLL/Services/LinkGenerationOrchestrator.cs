using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.BLL.Services.LinkGenerator;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;
using CryptoOnRamp.BLL.Services.TransactionViaContract;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using MicPic.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Services;

public sealed class LinkGenerationOrchestrator(
    IServiceProvider serviceProvider,
    IUserRepository users,
    ITransactionViaContractService transactionViaContractService,
    ICheckoutSessionRepository sessionRepo,
    IOnramperService onramper,
    IPaybisWidgetLinkBuilder paybis,
    IAppPasswordGenerator appPasswordGenerator,
    IOptions<AppilcationSettings> appilcationSettings,
    IUserService currentUser) : ILinkGenerationOrchestrator
{
    private static readonly string[] Providers = [/*"Transak"*/];
    

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IUserRepository _users = users;
    private readonly ITransactionViaContractService _transactionViaContractService = transactionViaContractService;
    private readonly ICheckoutSessionRepository _sessionRepo = sessionRepo;
    private readonly IOnramperService _onramper = onramper;
    private readonly IPaybisWidgetLinkBuilder _paybis = paybis;
    private readonly IUserService _currentUser = currentUser;
    private readonly IAppPasswordGenerator _appPasswordGenerator = appPasswordGenerator;
    private readonly AppilcationSettings _appilcationSettings = appilcationSettings.Value;

    public async Task<GenerateLinkResult> GenerateAsync(LinkGenerationInput input, CancellationToken ct)
    {
        if (input.Amount <= 0) throw new ArgumentOutOfRangeException(nameof(input.Amount));

        // 1) Проверка доступа (Admin/SuperAgent/Agent)
        var me = await _users.GetFirstOrDefaultAsync(u => u.Id == input.RequestorUserId)
                 ?? throw new UnauthorizedAccessException();
        var target = await _users.GetFirstOrDefaultAsync(u => u.Id == input.TargetUserId)
                     ?? throw new ArgumentException("User not found");

        if (string.IsNullOrWhiteSpace(input.Email))
        {
            throw new ApplicationException("Email is not set.");
        }
        
        if (string.IsNullOrWhiteSpace(me.UsdcWallet))
        {
            throw new ApplicationException("User wallet address is not set.");
        }

        if (string.IsNullOrWhiteSpace(target.UsdcWallet))
        {
            throw new ApplicationException("Target user wallet address is not set.");
        }

        switch (me.Role)
            {
                case UserRoleDb.Admin:
                    break;
                case UserRoleDb.SuperAgent:
                    var myAgents = await _users.GetAgentsBySuperAgentIdAsync(me.Id);
                    if (target.Id != me.Id && !myAgents.Any(a => a.Id == target.Id))
                        throw new UnauthorizedAccessException("Not your agent");
                    break;
                case UserRoleDb.Agent:
                    if (target.Id != me.Id) throw new UnauthorizedAccessException("Agents can only generate for themselves");
                    break;
                default: throw new UnauthorizedAccessException("Invalid role");
            }

        var tx = await _transactionViaContractService
            .CreateTransactionAsync(
                superAgentId: me.Id,
                superAgentWallet: me.UsdcWallet,
                agentId: target.Id,
                agentWallet: target.UsdcWallet,
                fiatCurrency: input.FiatCurrency,
                amount: input.Amount,
                cancellationToken: ct);

        var capacity = 6;
        var sessions = new List<CheckoutSessionDb>(capacity: capacity);

        #region Modern Way

        var lingRequest = new LinkRequest
        {
            UserId = me.Id,
            TransactionId = tx.Id,
            CryptoCurrency = "USDT_POLYGON",
            FiatCurrency = input.FiatCurrency.ToUpperInvariant(),
            FiatAmount = input.Amount,
            WalletAddress = tx.UniqueWalletAddress,
        };

        foreach (var provider in Providers)
        {
            var providerService = _serviceProvider
                .GetRequiredKeyedService<ILinkSource>(provider);

            var links = providerService
                .GenerateLinksAsync(lingRequest, ct);

            await foreach (var link in links)
            {
                var session = new CheckoutSessionDb
                {
                    Id = _appPasswordGenerator.GeneratePassword(),
                    TransactionId = tx.Id,
                    Ramp = link.Ramp,
                    PaymentMethod = link.PaymentMethod,
                    ExternalId = link.ExternalId,
                    PartnerContext = link.OrderId,
                    Url = link.Link,
                    Status = TransactionStatusDb.Issued
                };

                sessions
                    .Add(session);

                if (sessions.Count >= capacity)
                    break;
            }

            if (sessions.Count >= capacity)
                break;
        }

        #endregion


        // 3.1 Paybis (первым)
        try
        {
            //dont generatelinl for paybis
            var paybisUri = await _paybis.GetWidgetUrlAsync(new PaybisWidgetPetition
            {
               PartnerUserId = tx.Id.ToString(), //not user, it is need for webhook
               Flow = PaybisTransactionFlow.buyCrypto,
               CryptoAddress = tx.UniqueWalletAddress,
               CurrencyCodeFrom = input.FiatCurrency.ToUpperInvariant(),
               CurrencyCodeTo = "USDT-POLYGON",
               AmountFrom = input.Amount,
               AmountTo = 0m,
               Locale = "en",
               //Email = input.Email // removed email
            });

            
            sessions.Add(new CheckoutSessionDb
            {
               Id = _appPasswordGenerator.GeneratePassword(),
               TransactionId = tx.Id,
               Ramp = "paybis",
               PaymentMethod = "widget",
               ExternalId = null,
               PartnerContext = $"{tx.Id}:pb",
               Url = paybisUri.ToString(),
               Status = TransactionStatusDb.Issued
            });
        }
        catch { /* Paybis не обязателен */ }

        // 3.2 Onramper (до 5 после Paybis)
        try
        {
            var intents = await _onramper.CreateBestCheckoutIntentsAsync(
                fiat: input.FiatCurrency,
                crypto: "usdt_polygon",
                amount: input.Amount,
                walletAddress: tx.UniqueWalletAddress,
                walletMemo: null,
                //email: input.Email,
                transactionId: tx.Id,
                ct: ct
            );

            int added = 0;
            for (int i = 0; i < intents.Count && added < 5; i++)
            {
                var it = intents[i];
                var url = it?.Message?.TransactionInformation?.Url;

                
                if (string.IsNullOrWhiteSpace(url)) continue;

                //links.Add(url);
                sessions.Add(new CheckoutSessionDb
                {
                    Id = _appPasswordGenerator.GeneratePassword(),
                    TransactionId = tx.Id,
                    Ramp = it?.Message?.SessionInformation.Onramp ?? "",
                    PaymentMethod = it?.Message?.SessionInformation.PaymentMethod ?? "",
                    ExternalId = it?.Message?.TransactionInformation?.TransactionId,
                    PartnerContext = $"{tx.Id}:{i + 1}",
                    Url = url,
                    Status = TransactionStatusDb.Issued
                });
                added++;
            }
        }
        catch { /* нет ссылок — вернём хотя бы Paybis */ }

        if (sessions.Count > 0)
        {
            await _sessionRepo.InsertRangeAsync(sessions);
            await _sessionRepo.SaveAsync();

        }

        var paymentLinks = sessions
            .Select(session => $"{_appilcationSettings.UrlUi.TrimEnd('/')}/pay/{session.Id}")
            .ToList();

        return new GenerateLinkResult
        {
            TransactionId = tx.Id,
            PaymentLinks = paymentLinks,
            UniqueWalletAddress = tx.UniqueWalletAddress,
            EncryptedPrivateKey = tx.UniquePrivateKey
        };
    }
}
