using AutoMapper;
using CryptoOnRamp.BLL.Clients.Transak.Models;
using CryptoOnRamp.BLL.Services.LinkGenerator;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;
using CryptoOnRamp.BLL.Services.Transak.Internal;
using CryptoOnRamp.BLL.Services.Transak.Mapping;
using CryptoOnRamp.BLL.Services.Transak.Mapping.Converters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoOnRamp.BLL.Services.Transak;

public static class Configure
{
    public static IServiceCollection AddTransakService(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        
        // Options
        
        services.Configure<TransakServiceOptions>(configuration
            .GetSection(TransakServiceOptions.Position));

        // Mapping
        
        services.AddAutoMapper(config =>
            config.AddProfile<MappingProfile>());

        services.AddScoped<ITypeConverter<LinkRequest, WidgetRequest>, WidgetFromRequestConverter>();
        services.AddScoped<ITypeConverter<WidgetResponse, LinkResponse>, WidgetToResponseConverter>();

        // Services

        services.AddScoped<ITransakService, TransakService>();
        services.AddScoped<ITransakWebhookService, TransakWebhookService>();
        
        services.AddKeyedScoped<ILinkSource>(nameof(Transak), (sp, _) => sp.GetRequiredService<ITransakService>());

        return services;
    }
}
