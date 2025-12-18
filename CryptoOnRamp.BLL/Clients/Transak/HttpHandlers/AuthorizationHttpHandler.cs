using CryptoOnRamp.BLL.Clients.Transak.Enums;
using CryptoOnRamp.BLL.Clients.Transak.Internal;
using CryptoOnRamp.BLL.Clients.Transak.Dto;
using MicPic.Infrastructure.Exceptions;
using MicPic.Infrastructure.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CryptoOnRamp.BLL.Clients.Transak.HttpHandlers;

internal sealed class AuthorizationHttpHandler(
    IServiceProvider serviceProvider,
    IMemoryCache memoryCache,
    IOptionsSnapshot<TransakClientOptions> optionsSnapshot) : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly TransakClientOptions _options = optionsSnapshot.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorizationType = GetAuthorizationType(request);

        switch (authorizationType)
        {
            case AuthorizationTypes.ApiSecret:
                var apiSecret = _options.ApiSecret;
                request.Headers.Add("Api-Secret", apiSecret);
                break;

            case AuthorizationTypes.AccessToken:
                var accessToken = await GetAccessTokenAsync(cancellationToken);
                request.Headers.Add("Access-Token", accessToken);
                break;
        }

        return await base.SendAsync(request, cancellationToken);
    }


    #region Private Methods

    private static AuthorizationTypes GetAuthorizationType(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(new HttpRequestOptionsKey<AuthorizationTypes>("AuthorizationType"), out AuthorizationTypes authorizationType))
        {
            return authorizationType;
        }

        return AuthorizationTypes.AccessToken;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var credentials = await _memoryCache
            .GetOrCreateAsync("Transak_AccessToken", entry => BuildCredentialsCacheEntryAsync(entry, cancellationToken));

        return credentials?.AccessToken.NullIfWhiteSpace() ?? throw new AppException("Failed to get access token from Transak.", BusinessErrorCodes.NoData);
    }

    private async Task<CredentialsDto> BuildCredentialsCacheEntryAsync(ICacheEntry cacheEntry, CancellationToken cancellationToken)
    {
        var credentials = await _serviceProvider
            .GetRequiredService<ITransakClient>()
            .GetAccessCredentialsAsync(cancellationToken);

        cacheEntry.AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(credentials.ExpiresAt).AddMinutes(-1);
        
        return credentials;
    }

    #endregion
}
