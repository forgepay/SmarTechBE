using AutoMapper;
using MicPic.Infrastructure.Exceptions;

namespace MicPic.Infrastructure.Mapping;

public static class ResolutionContextExtensions
{
    public static T GetValue<T>(this ResolutionContext context, string key)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.GetValueOrDefault<T>(key) ?? throw new AppException(BusinessErrorCodes.Unexpected);
    }

    public static T? GetValueOrDefault<T>(this ResolutionContext context, string key)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.TryGetItems(out var items) && items.TryGetValue(key, out var _value) && _value is T value ? value : default;
    }

    public static CancellationToken GetCancellationToken(this ResolutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.TryGetItems(out var items) && items.TryGetValue(MappingKey.CancellationToken, out var _cancellationToken) && _cancellationToken is CancellationToken cancellationToken ? cancellationToken : default;
    }
}