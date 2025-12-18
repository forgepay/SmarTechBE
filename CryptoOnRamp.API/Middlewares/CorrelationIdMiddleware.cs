namespace CryptoOnRamp.API.Middlewares;


public class CorrelationIdMiddleware
{
    private const string CorrelationHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.ContainsKey(CorrelationHeader)
            ? context.Request.Headers[CorrelationHeader].ToString()
            : Guid.NewGuid().ToString("N");

        context.Items[CorrelationHeader] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        // 👇 Передаём как KeyValuePair, чтобы Serilog увидел это как свойство
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationHeader] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
