namespace MicPic.Infrastructure.Exceptions;

public static class BusinessErrorMessages
{
    private const string SomethingWentWrong = "Something went wrong.";

    private readonly static Dictionary<BusinessErrorCodes, string> _messages =
        new()
        {
            { BusinessErrorCodes.Unexpected, SomethingWentWrong },
            { BusinessErrorCodes.GeneralError, SomethingWentWrong},
            { BusinessErrorCodes.Unauthorized, "Unauthorized" },
            { BusinessErrorCodes.ValidationError, "One or more validation errors occurred." },
            { BusinessErrorCodes.NotFound, "Resource not found." },
            { BusinessErrorCodes.RateLimitExceeded, "Rate limit exceeded. Try again after {0}." },
        };

    public static string GetErrorMessage(BusinessErrorCodes code) =>
            _messages.GetValueOrDefault(code) ?? SomethingWentWrong;
}