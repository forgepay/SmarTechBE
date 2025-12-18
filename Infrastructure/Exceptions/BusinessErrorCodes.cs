using Microsoft.AspNetCore.Http;

namespace MicPic.Infrastructure.Exceptions;

public enum BusinessErrorCodes
{
    [StatusCode(StatusCodes.Status200OK)]
    NoError = 0,

    [StatusCode(StatusCodes.Status400BadRequest)]
    GeneralError = 1,

    [StatusCode(StatusCodes.Status500InternalServerError)]
    Unexpected = 2,

    [StatusCode(StatusCodes.Status404NotFound)]
    NotFound = 3,

    [StatusCode(StatusCodes.Status409Conflict)]
    AlreadyExists = 4,

    [StatusCode(StatusCodes.Status401Unauthorized)]
    Unauthorized = 5,

    [StatusCode(StatusCodes.Status419AuthenticationTimeout)]
    Expired = 6,

    [StatusCode(StatusCodes.Status400BadRequest)]
    ValidationError = 7,

    [StatusCode(StatusCodes.Status500InternalServerError)]
    Misconfigured = 8,

    [StatusCode(StatusCodes.Status423Locked)]
    Locked = 10,

    [StatusCode(StatusCodes.Status429TooManyRequests)]
    RateLimitExceeded = 11,

    [StatusCode(StatusCodes.Status400BadRequest)]
    Unsupported = 12,

    [StatusCode(StatusCodes.Status500InternalServerError)]
    NoData = 100,

    [StatusCode(StatusCodes.Status500InternalServerError)]
    Internal3rdPartyError = 999,
}