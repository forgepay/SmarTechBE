using System.Security.Claims;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CryptoOnRamp.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;

        var token = httpContext.Request.Headers["X-COR-ApiKey"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var cancellationToken = httpContext.RequestAborted;
        var serviceProvider = httpContext.RequestServices;

        var userId = await serviceProvider
            .GetRequiredService<IApiKeyService>()
            .GetUserIdByApiKeyOrDefaultAsync(token, cancellationToken);

        if (!userId.HasValue)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Create claims for the user

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(AppClaims.Id, userId.Value.ToString()),
        };

        // Create identity and principal

        var identity = new ClaimsIdentity(claims, "ApiKey");
        var principal = new ClaimsPrincipal(identity);

        // Set the user on HttpContext

        context.HttpContext.User = principal;
    }
}