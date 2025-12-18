using AutoMapper;
using CryptoOnRamp.BLL.Clients.Transak.Models;
using CryptoOnRamp.BLL.Services.LinkGenerator.Models;

namespace CryptoOnRamp.BLL.Services.Transak.Mapping;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LinkRequest, WidgetRequest>()
            .ConvertUsing<ITypeConverter<LinkRequest, WidgetRequest>>();

        CreateMap<WidgetResponse, LinkResponse>()
            .ConvertUsing<ITypeConverter<WidgetResponse, LinkResponse>>();
    }
}
