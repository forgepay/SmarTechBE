using CryptoOnRamp.BLL.Clients.ExchangeRateApi.HttpHandlers;
using CryptoOnRamp.BLL.Clients.ExchangeRateApi.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.ExchangeRateApi;

public static class Configure
{
    public static IServiceCollection AddExchangeRateApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ExchangeRateApiClientOptions>(
            configuration.GetSection(ExchangeRateApiClientOptions.Position));

        services.AddHttpClient<IExchangeRateApiClient, ExchangeRateApiClient>(ConfigureHttpClient)
            .AddHttpMessageHandler<ErrorHttpHandler>();

        services.AddTransient<ErrorHttpHandler>();

        return services;
    }


    #region Private

    private static void ConfigureHttpClient(IServiceProvider services, HttpClient client)
    {
        var options = services
            .GetRequiredService<IOptionsMonitor<ExchangeRateApiClientOptions>>()
            .CurrentValue;
        client.BaseAddress = new Uri(options.Endpoint);
    }

    #endregion
}