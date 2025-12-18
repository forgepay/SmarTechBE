using AutoMapper;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using CryptoOnRamp.BLL.Helpers;
using Microsoft.EntityFrameworkCore;
using MicPic.Infrastructure.Exceptions;

namespace CryptoOnRamp.BLL.Services;

public class IdentityService(
    IPasswordHashier passwordHashier,
    IUserRepository userRepository,
    IMapper mapper,
    IAuthenticationService authenticationService,
    IOptions<IdentityServiceOptions> options,
    IEmailService emailService,
    IUserService userService) : IIdentityService
{
    private readonly IPasswordHashier _passwordHashier = passwordHashier;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IdentityServiceOptions _options = options.Value;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailService _emailService = emailService;
    private readonly IUserService _userService = userService;

    public async Task<AuthenticationContext> LoginAsync(string name, string passcode)
    {
        var userDb = await GetAndValidateVerifiedUserAsync(name, passcode);
        var user = _mapper.Map<User>(userDb);


        return await GenerateTokenAsync(user.Id.ToString(), user);
    }

    public async Task<AuthenticationContext> GenerateTokenAsync(string id, User userClientInfo)
    {

        var context = await _authenticationService.GenerateAuthenticationContextAsync(
            userId: id,
            claims:
              new Claim[]
              {
                  new Claim(AppClaims.Role, userClientInfo.Role switch
                  {
                      UserRole.Admin      => Constans.Admin,
                      UserRole.SuperAgent => Constans.SuperAgent,
                      UserRole.Agent      => Constans.Agent,
                      _                   => throw new InvalidOperationException("Unknown role")
                  })
              },
            accessTokenExpiry: _options.AccessTokenLifeTime,
            refreshTokenExpiry: _options.RefreshTokenLifeTime,
            userClientInfo: userClientInfo,
            cancellationToken: CancellationToken.None);

        return context;
    }

    private async Task<UserDb> GetAndValidateVerifiedUserAsync(string name, string password)
    {
        UserDb userDb = await GetUserByNameAsync(name);

        if (userDb is null)
            throw new Exception("No user with name");

        if (string.IsNullOrWhiteSpace(userDb.PasswordHash))
            throw new ApplicationException("User has no password.");

        if (!_passwordHashier.VerifyPassword(password, userDb.PasswordHash))
            throw new Exception("Password Is Incorrect");

        return userDb;
    }

    private async Task<UserDb> GetUserByNameAsync(string name)
    {
        var userDb = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Name != null && x.Name.ToLower() == name.ToLower())
            .SingleOrDefaultAsync()
            ?? throw new AppException(BusinessErrorCodes.Unauthorized);

        return userDb;
    }

    public async Task<AuthenticationContext> RefreshTokenAsync(string name, string refreshToken, CancellationToken cancellationToken)
    {
        var userDb = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Name != null && x.Name.ToLower() == name.ToLower())
            .SingleOrDefaultAsync()
            ?? throw new AppException(BusinessErrorCodes.Unauthorized);

        var user = _mapper
            .Map<User>(userDb);

        var context = await _authenticationService
            .RefreshAuthenticationContextAsync(
                refreshToken: refreshToken,
                accessTokenExpiry: _options.AccessTokenLifeTime,
                refreshTokenExpiry: _options.RefreshTokenLifeTime,
                userClientInfo: user,
                cancellationToken: cancellationToken);

        return context;
    }

    public async Task GeneratePasswordResetTokenAsync(string email)
    {
        await PrivateGeneratePasswordResetTokenAsync(email);
    }

    public async Task<string> PrivateGeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Email != null && x.Email.ToLower() == email.ToLower())
            .SingleOrDefaultAsync()
            ?? throw new AppException(BusinessErrorCodes.Unauthorized);

        var resetInfo = new PasswordResetInfo()
        {
            UserId = user.Id,
            ExpiredAt = DateTime.UtcNow.AddMinutes(10),
        };

        var key = Guid.NewGuid().ToString();

        user.PasswordResetKey = key;

        var hash = AesEncryption.Encrypt(key, resetInfo);

        _userRepository
            .Update(user);
        
        await _userRepository
            .SaveAsync();

        await _emailService
            .SendResetPasswordAsync(user.Email, hash, CancellationToken.None);

        return hash;
    }

    public async Task ResetPasswordAsync(string email, string hash, string newPassword)
    {
        var user = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Email != null && x.Email.ToLower() == email.ToLower())
            .SingleOrDefaultAsync()
            ?? throw new AppException(BusinessErrorCodes.Unauthorized);

        if (string.IsNullOrWhiteSpace(user.PasswordResetKey))
        {
            throw new ApplicationException("Password reset key is not set.");
        }

        var model = AesEncryption.Decrypt<PasswordResetInfo>(user.PasswordResetKey, hash);

        if (model.ExpiredAt < DateTime.UtcNow)
        {
            throw new Exception("Token for reset password is expired.");
        }

        user.PasswordResetKey = string.Empty;

        await _userService.ChangePasswordAsync(user.Id, string.Empty, newPassword, false);

        _userRepository.Update(user);
        await _userRepository.SaveAsync();
    }
}
