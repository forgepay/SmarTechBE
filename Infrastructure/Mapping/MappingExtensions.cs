using AutoMapper;

namespace MicPic.Infrastructure.Mapping;

public static class MappingExtensions
{
    public static async Task<TTarget> MapAsync<TSource, TTarget>(this Task<TSource> source, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map<TTarget>(await source);
    }

    public static async Task<TTarget> MapAsync<TSource, TTarget>(this Task<TSource> source, IMapper mapper, Action<IMappingOperationOptions<TSource, TTarget>> opts)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        return mapper.Map(await source, opts);
    }

    public static async IAsyncEnumerable<TTarget> MapAsync<TSource, TTarget>(this IAsyncEnumerable<TSource> source, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(mapper);

        await foreach (var item in source)
        {
            yield return mapper.Map<TTarget>(item);
        }
    }
}
