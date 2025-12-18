using CryptoOnRamp.BLL.Clients.Transak.HttpHandlers;
using CryptoOnRamp.BLL.Clients.Transak.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.Transak;

public static class Configure
{
    public static IServiceCollection AddTransakClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TransakClientOptions>(
            configuration.GetSection(TransakClientOptions.Position));

        services.AddHttpClient<ITransakClient, TransakClient>(ConfigureHttpClient)
            .AddHttpMessageHandler<AuthorizationHttpHandler>()
            .AddHttpMessageHandler<ErrorHttpHandler>();

        services.AddHttpClient<ITransakGatewayClient, TransakGatewayClient>(ConfigureGatewayHttpClient)
            .AddHttpMessageHandler<AuthorizationHttpHandler>()
            .AddHttpMessageHandler<ErrorHttpHandler>();

        services.AddTransient<AuthorizationHttpHandler>();
        services.AddTransient<ErrorHttpHandler>();

        return services;
    }


    #region Private

    private static void ConfigureHttpClient(IServiceProvider services, HttpClient client)
    {
        var options = services
            .GetRequiredService<IOptionsMonitor<TransakClientOptions>>()
            .CurrentValue;
        client.BaseAddress = new Uri(options.Endpoint);
    }

    private static void ConfigureGatewayHttpClient(IServiceProvider services, HttpClient client)
    {
        var options = services
            .GetRequiredService<IOptionsMonitor<TransakClientOptions>>()
            .CurrentValue;
        client.BaseAddress = new Uri(options.GatewayEndpoint);
    }

    #endregion
}