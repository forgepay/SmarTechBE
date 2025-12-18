using System.Globalization;

namespace MicPic.Infrastructure.Exceptions;

#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable CA1058 // Types should not extend certain base types

public class AppException : ApplicationException
{
    public BusinessErrorCodes Code { get; }

    public int StatusCode { get; }

    public string? TechnicalMessage { get; init; }

    public AppException()
        : this(BusinessErrorCodes.GeneralError)
    {

    }

    public AppException(BusinessErrorCodes code, params object?[] args)
        : this(GetStatusCode(code), GetErrorMessage(code), code, args)
    {

    }
    
    public AppException(string message, BusinessErrorCodes code, params object?[] args)
        : this(GetStatusCode(code), message, code, args)
    {

    }

    public AppException(int statusCode, BusinessErrorCodes code, params object?[] args)
        : this(statusCode, GetErrorMessage(code), code, args)
    {

    }

    public AppException(int statusCode, string message, BusinessErrorCodes code, params object?[] args)
        : base(string.Format(CultureInfo.InvariantCulture, message, args))
    {
        Code = code;
        StatusCode = statusCode;
    }

    public AppException(AppException e, BusinessErrorCodes code, params object?[] args)
        : this(e, GetStatusCode(code), GetErrorMessage(code), code, args)
    {

    }
    
    public AppException(AppException e, string message, BusinessErrorCodes code, params object?[] args)
        : this(e, GetStatusCode(code), message, code, args)
    {

    }

    public AppException(AppException e, int statusCode, BusinessErrorCodes code, params object?[] args)
        : this(e, statusCode, GetErrorMessage(code), code, args)
    {

    }

    public AppException(AppException e, int statusCode, string message, BusinessErrorCodes code, params object?[] args)
        : base(string.Format(CultureInfo.InvariantCulture, message, args), e)
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));

        Code = code;
        StatusCode = statusCode;
        TechnicalMessage = e.Message;
    }

    private static int GetStatusCode(BusinessErrorCodes code) 
        => typeof(BusinessErrorCodes)
            .GetMember(code.ToString())
            .SelectMany(x => x.GetCustomAttributes(typeof(StatusCodeAttribute), false))
            .Cast<StatusCodeAttribute>()
            .Select(x => x.Code)
            .FirstOrDefault(500);

    private static string GetErrorMessage(BusinessErrorCodes code)
        => BusinessErrorMessages.GetErrorMessage(code);
}
