using System.Reflection;

namespace MicPic.Infrastructure.Extensions;

public static class ExceptionExtensions
{
    public static TException WithData<TException>(this TException exception, string key, object? value) where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(exception);

        exception.Data[key] = value;

        return exception;
    }
}
