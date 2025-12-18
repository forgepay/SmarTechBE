using AutoMapper;
using CryptoOnRamp.API.Controllers.PaymentLinks.Dto;
using CryptoOnRamp.BLL.Models;

namespace CryptoOnRamp.API.Controllers.PaymentLinks.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GenerateLinkRequestDto, Task<GenerateLinkRequest>>()
            .ConvertUsing<ITypeConverter<GenerateLinkRequestDto, Task<GenerateLinkRequest>>>();
    }
}
