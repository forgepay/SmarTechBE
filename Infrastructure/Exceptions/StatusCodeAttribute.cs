namespace MicPic.Infrastructure.Exceptions;

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class StatusCodeAttribute(int code) : Attribute
{
    public int Code { get; } = code;
}
