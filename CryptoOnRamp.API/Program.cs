using CryptoOnRamp.API.Controllers.PaymentLinks;
using CryptoOnRamp.API.Middlewares;
using CryptoOnRamp.BLL;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL;
using MicPic.Infrastructure.RateLimit;
using MicPic.Infrastructure.Security;
using MicPic.Infrastructure.Serialization.Converters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoOnRamp.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Console.WriteLine($"ASPNETCORE_ENVIRONMENT={environment}");

        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Add services to the container.

        services.AddMemoryCache();
        services.AddAppRateLimit(configuration);
        services.AddAppSecurity(configuration);

        services.AddCors(options =>
        {
            var cfg = configuration.GetSection("CORS");
            options.AddDefaultPolicy(builder =>
            {
                static string[] format(string? s) =>
                    (s ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                builder
                    .WithOrigins(format(cfg["Origins"]))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        // Services

        services.AddBllServices(configuration);

        // Controllers

        services
            .AddPaymentLinks(configuration);

        // Other

        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            x.JsonSerializerOptions.Converters.Add(new BigIntegerConverter());
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
#pragma warning disable SYSLIB0020 // Type or member is obsolete
            x.JsonSerializerOptions.IgnoreNullValues = true;
#pragma warning restore SYSLIB0020 // Type or member is obsolete
        });

        ConfigureSwagger(services);
        ConfigureAuthentication(services, configuration);

        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Host.UseSerilog();
        ConfigureLogging(configuration);


        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constans.Admin, policy => policy.RequireClaim(AppClaims.Role, Constans.Admin));
            options.AddPolicy(Constans.Agent, policy => policy.RequireClaim(AppClaims.Role, Constans.Agent));
            options.AddPolicy(Constans.SuperAgent, policy => policy.RequireClaim(AppClaims.Role, Constans.SuperAgent));
            options.AddPolicy(Constans.AdminOrSuperAgent, policy => policy.RequireAssertion(ctx =>
                ctx.User.HasClaim(c => c.Type == AppClaims.Role &&(c.Value == Constans.Admin || c.Value == Constans.SuperAgent))));
        });

        services.AddHttpContextAccessor();

        var app = builder.Build();

        if (!app.Environment.IsProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseAuthentication();
        app.UseRouting();
        app.UseAppRateLimit();
        app.UseAuthorization();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var servicesProvider = scope.ServiceProvider;
            var dbContext = servicesProvider.GetRequiredService<ApplicationContext>();

            await dbContext.Database.MigrateAsync(default);
        }

        app.Run();
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(setup =>
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
            };

            setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            setup.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            setup.IncludeXmlComments(xmlPath);                
        });
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var config = configuration.GetSection("AuthenticationSettings");
                var signingKey = config["PrivateKey"] ?? string.Empty;
                var issuer = config["Issuer"];
                var audience = config["Audience"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Response.ContentType = "application/json";

                        if (context.AuthenticateFailure is SecurityTokenExpiredException)
                        {
                            await context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                success = false,
                                data = (object?)null,
                                expandedError = new { errorCode = "JwtTokenExpired", message = "JWT token expired." }
                            }));
                        }
                        else if (context.AuthenticateFailure is Exception ex)
                        {
                            await context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                success = false,
                                data = (object?)null,
                                expandedError = new { errorCode = "AuthenticateFailure", message = context.ErrorDescription, technicalMessage = context.Error }
                            }));
                        }
                    }
                };
            });
    }

    private static void ConfigureLogging(IConfiguration configuration)
    {
        var connectionString = configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
        var tableName = "logs";

        var columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "MessageTemplate", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "TimeStamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
            { "Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "LogEvent", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) }
        };

        Log.Logger = new LoggerConfiguration()
          .Enrich.FromLogContext()
          .MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
          .MinimumLevel.Override("System", LogEventLevel.Warning)
          .WriteTo.Console(outputTemplate:
              "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
          .WriteTo.PostgreSQL(
              connectionString: connectionString,
              tableName: tableName,
              columnOptions: columnWriters,
              needAutoCreateTable: true)
          .CreateLogger();
    }
}
