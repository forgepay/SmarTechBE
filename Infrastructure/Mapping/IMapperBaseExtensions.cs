using AutoMapper;

namespace MicPic.Infrastructure.Mapping;

public static class IMapperBaseExtensions
{
    public static TTarget? MapOrDefault<TTarget>(this IMapperBase mapper, object? source, TTarget? defaultValue = default(TTarget?))
    {
        ArgumentNullException.ThrowIfNull(mapper);

        if (source is null)
            return defaultValue;

        return mapper.Map<TTarget>(source);
    }
}
