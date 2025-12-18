using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace MicPic.Infrastructure.Security.Internal;

internal sealed class AppPasswordGenerator(IOptionsSnapshot<SecurityOptions> optionsSnapshot) : IAppPasswordGenerator
{
    private readonly SecurityOptions _options = optionsSnapshot.Value;

    public string GeneratePassword()
    {
        return RandomNumberGenerator
            .GetString(
                choices: _options.PasswordChars,
                length: _options.PasswordLength);
    }
}
