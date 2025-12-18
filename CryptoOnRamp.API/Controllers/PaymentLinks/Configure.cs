using AutoMapper;
using CryptoOnRamp.API.Controllers.PaymentLinks.Controllers;
using CryptoOnRamp.API.Controllers.PaymentLinks.Dto;
using CryptoOnRamp.API.Controllers.PaymentLinks.Mapping;
using CryptoOnRamp.API.Controllers.PaymentLinks.Mapping.Converters;
using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.API.Controllers.PaymentLinks;

public static partial class Configure
{
    public static IServiceCollection AddPaymentLinks(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Options
        
        services.Configure<PaymentLinksOptions>(configuration
            .GetSection(PaymentLinksOptions.Position));

        // Mapping

        services.AddAutoMapper(config =>
            config.AddProfile<MappingProfile>());

        // Services

        services.AddScoped<ITypeConverter<GenerateLinkRequestDto, Task<GenerateLinkRequest>>, GenerateLinkRequestFromDtoConverter>();

        return services;
    }
}
