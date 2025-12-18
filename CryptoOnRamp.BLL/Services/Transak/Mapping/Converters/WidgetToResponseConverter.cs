using AutoMapper;
using CryptoOnRamp.BLL.Clients.Transak.Models;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;

namespace CryptoOnRamp.BLL.Services.Transak.Mapping.Converters;

internal sealed class WidgetToResponseConverter : ITypeConverter<WidgetResponse, LinkResponse>
{
    public LinkResponse Convert(WidgetResponse source, LinkResponse destination, ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new()
        {
            OrderId = source.PartnerOrderId,
            Link = source.WidgetUrl,
            Ramp = "transak",
            PaymentMethod = "widget",
        };
    }
}