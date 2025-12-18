using AutoMapper;

namespace MicPic.Infrastructure.Mapping;

public static class IMapperExtensions
{

    public static Task<TTarget> MapAsync<TTarget>(this IMapper mapper, object source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map<Task<TTarget>>(source, opts => opts.WithCancellationToken(cancellationToken));
    }
}
