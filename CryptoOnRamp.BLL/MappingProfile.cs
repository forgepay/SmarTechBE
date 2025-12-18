using AutoMapper;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;

namespace CryptoOnRamp.BLL;

public class BllMappingProfile : Profile
{
    public BllMappingProfile()
    {
        CreateMap<User, UserDb>().ReverseMap();
        CreateMap<TelegramUser, TelegramUserDb>().ReverseMap();
        CreateMap<Session, SessionDb>().ReverseMap();
        CreateMap<TransactionStatus, TransactionStatusDb>().ReverseMap();

        CreateMap<PayoutDb, PayoutDto>().ReverseMap();

        CreateMap<CheckoutSessionDb, CheckoutSessionDTO>().ReverseMap();
        CreateMap<TransactionDb, TransactionDto>()
            .ForMember(d => d.Id, m => m.MapFrom(s => s.Id))
            .ForMember(d => d.Fees, m => m.MapFrom(s => new FeeDto
            {
                Agent = s.AgentFee,
                AgentPercent = s.AgentPercent,
                SuperAgentPercent = s.SuperAgentPercent,
                SuperAgent = s.SuperAgentFee,
                NetPayout = s.NetPayout
            }));
    }
}
