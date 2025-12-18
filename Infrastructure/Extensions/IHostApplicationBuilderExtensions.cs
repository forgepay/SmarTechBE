using Microsoft.Extensions.Hosting;

namespace MicPic.Infrastructure.Extensions;

public static class IHostApplicationBuilderExtensions
{
    public static HostApplicationBuilder AsHostBuilder(this IHostApplicationBuilder builder) =>
        builder as HostApplicationBuilder ?? throw new ArgumentOutOfRangeException(nameof(builder), "The provided builder is not a HostApplicationBuilder.");
}
