using MicPic.Infrastructure.Security.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicPic.Infrastructure.Security;

public static partial class Configure
{
    public static IServiceCollection AddAppSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        // Options
        
        services.Configure<SecurityOptions>(configuration
            .GetSection(SecurityOptions.Position));

        // Services

        services.AddScoped<IAppPasswordGenerator, AppPasswordGenerator>();
        services.AddSingleton<IAppPasswordHasher, AppPasswordHasher>();

        return services;
    }
}
