using Microsoft.Extensions.DependencyInjection;

namespace CryptoOnRamp.Tests;

public class BaseIntegrationTest
{
    protected IServiceScope Scope;
    protected CancellationToken _token;
    private CustomWebAppFactory _factory;
    protected readonly HttpClient _client;

    public BaseIntegrationTest()
    {
        _factory = CustomWebAppFactory.GetInstance();
        Scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _client = _factory.CreateClient();

        _token = new CancellationToken();
    }
}
