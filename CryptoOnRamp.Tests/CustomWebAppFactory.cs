using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CryptoOnRamp.Tests;

public class CustomWebAppFactory : WebApplicationFactory<CryptoOnRamp.API.Program>
{
    private static CustomWebAppFactory? _instance;
    private static readonly object _obj = "obj";

    private readonly string[] env = ["ASPNETCORE_ENVIRONMENT", "AWS_ACCESS_KEY_ID", "AWS_SECRET_ACCESS_KEY"];

    public static CustomWebAppFactory GetInstance()
    {
        lock (_obj)
        {
            if (_instance == null)
            {
                _instance = new CustomWebAppFactory();
            }

            return _instance;
        }
    }

    private CustomWebAppFactory()
    {
        Environment.SetEnvironmentVariable("RetryCount_For_Test", "5");

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets("OTCDasboard")
        .Build();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("IS_TEST_ENVIRONMENT", "true");
    }
}