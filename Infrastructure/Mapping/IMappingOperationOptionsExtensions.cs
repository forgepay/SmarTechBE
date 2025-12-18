using AutoMapper;

namespace MicPic.Infrastructure.Mapping;

public static class IMappingOperationOptionsExtensions
{
    public static IMappingOperationOptions<TSource, TTarget> WithValue<TSource, TTarget>(this IMappingOperationOptions<TSource, TTarget> options, string name, object value)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Items[name] = value;

        return options;
    }


    public static IMappingOperationOptions<TSource, TTarget> WithCancellationToken<TSource, TTarget>(this IMappingOperationOptions<TSource, TTarget> options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Items[MappingKey.CancellationToken] = cancellationToken;

        return options;
    }
}