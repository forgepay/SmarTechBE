using CryptoOnRamp.DAL.Repositories.Implementations;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CryptoOnRamp.DAL;

public static class Configure
{
    public static IServiceCollection AddDalServices(this IServiceCollection services, IConfiguration configuration)
    {
        // database 
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IFeeSchemeRepository, FeeSchemeRepository>();
        services.AddScoped<ICheckoutSessionRepository, CheckoutSessionRepository>();
        services.AddScoped<IPayoutRepository, PayoutRepository>();
        services.AddScoped<ITelegramUserRepository, TelegramUserRepository>();

        try
        {
            // Retrieve the connection string from configuration
            var connectionString = configuration.GetSection("ConnectionStrings:DefaultConnection").Value;

            // Test the database connection
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to RDS successfully.");
            }


            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(dataSource));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to RDS: {ex.Message}");
            throw;
        }

        return services;
    }
}
