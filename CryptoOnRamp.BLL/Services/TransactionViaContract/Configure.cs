using CryptoOnRamp.BLL.Services.TransactionViaContract.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoOnRamp.BLL.Services.TransactionViaContract;

public static class Configure
{
    public static IServiceCollection AddTransactionViaContractService(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TransactionViaContractServiceOptions>(
            configuration.GetSection(TransactionViaContractServiceOptions.Position));

        services.AddScoped<ITransactionViaContractService, TransactionViaContractService>();

        return services;
    }
}