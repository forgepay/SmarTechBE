using Microsoft.AspNetCore.Mvc;

namespace MicPic.Infrastructure.Exceptions;

public class AppValidationProblemDetails(ValidationProblemDetails validationProblemDetails) 
    : Dictionary<string, string[]>(validationProblemDetails?.Errors ?? new Dictionary<string, string[]>())
{
}
