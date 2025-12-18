using CryptoOnRamp.BLL.Clients.TgRampSmart.HttpHandlers;
using CryptoOnRamp.BLL.Clients.TgRampSmart.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.TgRampSmart;

public static partial class Configure
{
    public static IServiceCollection AddTgRampSmartClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TgRampSmartClientOptions>(
            configuration.GetSection(TgRampSmartClientOptions.Position));

        services.AddHttpClient<ITgRampSmartClient, TgRampSmartClient>(ConfigureHttpClient)
            .AddHttpMessageHandler<ErrorHttpHandler>();

        services.AddTransient<ErrorHttpHandler>();

        return services;
    }


    #region Private

    private static void ConfigureHttpClient(IServiceProvider services, HttpClient client)
    {
        var options = services
            .GetRequiredService<IOptionsMonitor<TgRampSmartClientOptions>>()
            .CurrentValue;
        client.BaseAddress = new Uri(options.Endpoint);
    }

    #endregion
}