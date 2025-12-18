using CryptoOnRamp.BLL.Clients.ExchangeRateApi;
using CryptoOnRamp.BLL.Clients.TgRampSmart;
using CryptoOnRamp.BLL.Clients.Transak;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.BLL.Services;
using CryptoOnRamp.BLL.Services.ExchangeRate;
using CryptoOnRamp.BLL.Services.TransactionViaContract;
using CryptoOnRamp.BLL.Services.Transak;
using CryptoOnRamp.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace CryptoOnRamp.BLL;

public static class Configure
{
    public static IServiceCollection AddBllServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDalServices(configuration);
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHashier, PasswordHashier>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITelegramUserService, TelegramUserService>();
        services.AddScoped<ITelegramBotDialogService, TelegramBotDialogService>();
        services.AddScoped<IWebHookService, WebHookService>();
        services.AddScoped<IWebhookTransactionNotificationService, WebhookTransactionNotificationService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        services.AddAutoMapper(config => config.AddProfile<BllMappingProfile>());
        services.Configure<AuthenticationSettings>(configuration.GetSection(AuthenticationSettings.Position));

        services.AddHttpClient("tg_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var token = configuration["Telegram:BotToken"]
                             ?? throw new InvalidOperationException("Telegram Bot Token is missing");
                return new TelegramBotClient(token, httpClient);
            });

        services.AddHostedService<TelegramBotService>();

        services.AddHttpClient("onramper", (sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri("https://api.onramper.com/");
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {config["Onramper:ApiKey"]}");
        });

        services.AddScoped<IOnramperService, OnramperService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IEncryptionHelper, EncryptionHelper>();
        services.Configure<OnramperOptions>(configuration.GetSection(OnramperOptions.Position));
        services.AddScoped<ITransactionService, TransactionService>();

        services.Configure<BlockchainOptions>(configuration.GetSection("Blockchain"));
        services.AddScoped<IFeeService, FeeService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IOnramperWebhookService, OnramperWebhookService>();
        services.AddScoped<IGenerateLinkService, GenerateLinkService>();
        services.Configure<PaybisOptions>(configuration.GetSection("Paybis"));
        services.AddScoped<ILinkGenerationOrchestrator, LinkGenerationOrchestrator>();
        services.AddScoped<IPaybisWidgetLinkBuilder, PaybisWidgetLinkBuilder>();
        services.AddScoped<IPaybisWebhookService, PaybisWebhookService>();
        services.Configure<AppilcationSettings>(configuration.GetSection(AppilcationSettings.Position));
        services.Configure<EmailServiceOptions>(configuration.GetSection(EmailServiceOptions.Position));
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<ITelegramBotClient>(sp => new TelegramBotClient(configuration["Telegram:BotToken"] ?? throw new ApplicationException("Telegram bot token is missing.")));

        services.AddScoped<ITelegramTransactionNotificationService, TelegramTransactionNotificationService>();


        services.AddScoped<IGenerateLinkService, GenerateLinkService>();

        // Clients

        services
            .AddExchangeRateApiClient(configuration)
            .AddTgRampSmartClient(configuration)
            .AddTransakClient(configuration)
            ;

        // Services

        services
            .AddExchangeRateService(configuration)
            .AddTransactionViaContractService(configuration)
            .AddTransakService(configuration)
            ;

        return services;
    }
}
