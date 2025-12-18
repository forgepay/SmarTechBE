using AutoMapper;
using CryptoOnRamp.BLL.Extensions;
using CryptoOnRamp.BLL.Interfaces;
using CryptoOnRamp.BLL.Models;
using CryptoOnRamp.DAL.Models;
using CryptoOnRamp.DAL.Repositories.Interfaces;
using MicPic.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CryptoOnRamp.BLL.Services;

public class UserService(IUserRepository userRepository, IMapper mapper, IPasswordHashier passwordHashier, IHttpContextAccessor httpContextAccessor) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHashier _passwordHashier = passwordHashier;
    private readonly IMapper _mapper = mapper;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<User> CreateAsync(CreateUserReuqest createUser)
    {
        if (string.IsNullOrWhiteSpace(createUser.Username))
            throw new ArgumentException("Username is required");

        if (string.IsNullOrWhiteSpace(createUser.Password))
            throw new ArgumentException("Password is required");

        var currentUserId = GetCurrentUserId();

        var creator = await _userRepository
            .GetActiveUserAsync(currentUserId, default);

        ValidateRolePermissions(creator.Role, createUser, 0);

        var existingUser = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Name != null && x.Name.ToLower() == createUser.Username.ToLower())
            .FirstOrDefaultAsync();

        if (existingUser != null)
            throw new AppException($"User with username '{createUser.Username}' already exists.", BusinessErrorCodes.ValidationError);

        var existingUserEmail = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Email != null && x.Email.ToLower() == createUser.Email.ToLower())
            .FirstOrDefaultAsync();

        if (existingUserEmail != null)
            throw new AppException("User with this email is already exists", BusinessErrorCodes.ValidationError);

        var newUser = new UserDb
        {
            Name = createUser.Username,
            Email = createUser.Email,
            Role = (UserRoleDb)createUser.Role,
            CreatedById = createUser.CreatedById.HasValue ? createUser.CreatedById.Value : GetCurrentUserId(),
            PasswordHash = _passwordHashier.HashPassword(createUser.Password),
            CreatedAt = DateTime.UtcNow,
            UsdcWallet = createUser.UsdcWallet
        };

        await _userRepository
            .InsertAsync(newUser);

        await _userRepository
            .SaveAsync();

        return _mapper
            .Map<User>(newUser);
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetActiveUserAsync(userId, cancellationToken);

        if (user.Role == UserRoleDb.Admin)
            throw new AppException("Admin cannot be deleted", BusinessErrorCodes.ValidationError);

        user.DeletedAt = DateTime.UtcNow;
        
        _userRepository
            .Update(user);

        await _userRepository
            .SaveAsync();
    }

    public int GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            throw new AppException("User is not authenticated.", BusinessErrorCodes.Unauthorized);

        var idClaim = user.Claims
            .SingleOrDefault(c =>
                string.Equals(AppClaims.Id, c.Type, StringComparison.InvariantCultureIgnoreCase))
            ?.Value;

        if (string.IsNullOrEmpty(idClaim))
            throw new UnauthorizedAccessException("User ID not found in claims.");

        if (!int.TryParse(idClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid user ID format in claims.");

        return userId;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(UserRole? role, int? parentId)
    {
        var currentUserId = GetCurrentUserId();

        var currentUser = await _userRepository
            .GetActiveUserAsync(currentUserId, default);

        if (currentUser.Role == UserRoleDb.Admin)
        {
            var query = _userRepository
                .Query()
                .Where(x => x.DeletedAt == null);

            if (role.HasValue)
                query = query.Where(x => x.Role == (UserRoleDb)role.Value);

            if (parentId.HasValue)
                query = query.Where(x => x.CreatedById == parentId.Value);

            var users = await query
                .ToListAsync();

            return _mapper
                .Map<List<User>>(users);
        }

        if (currentUser.Role == UserRoleDb.SuperAgent)
        {
            var users = await _userRepository
                .Query()
                .Where(x => x.DeletedAt == null)
                .Where(x => x.CreatedById == currentUser.Id)
                .ToListAsync();

            return _mapper
                .Map<List<User>>(users);
        }

        if (currentUser.Role == UserRoleDb.Agent)
        {
            return [];
        }

        throw new AppException("Invalid role", BusinessErrorCodes.ValidationError);
    }

    private static void ValidateRolePermissions(UserRoleDb currentUserRole, CreateUserReuqest createUser, int currentUserId)
    {
        switch (currentUserRole)
        {
            case UserRoleDb.Admin:
                if (createUser.Role != UserRole.Agent && createUser.Role != UserRole.SuperAgent)
                    throw new AppException("Admin can only create Agents or SuperAgents", BusinessErrorCodes.Unauthorized);
                break;

            case UserRoleDb.SuperAgent:
                if (createUser.Role != UserRole.Agent)
                    throw new AppException("SuperAgent can only create Agents", BusinessErrorCodes.Unauthorized);
                break;

            case UserRoleDb.Agent:
                throw new AppException("Agents cannot create users", BusinessErrorCodes.Unauthorized);

            default:
                throw new AppException("Unknown role", BusinessErrorCodes.ValidationError);
        }
    }

    public async Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, bool checkOldPasswordVerification = true)
    {
        var userDb = await _userRepository
            .GetActiveUserAsync(userId, default);

        if (userDb.Email == newPassword)
        {
            throw new Exception("Email is equal to password.");
        }

        if (checkOldPasswordVerification)
        {
            if (string.IsNullOrWhiteSpace(userDb.PasswordHash))
                throw new AppException("User has no password.", BusinessErrorCodes.ValidationError);

            if (!_passwordHashier.VerifyPassword(oldPassword, userDb.PasswordHash))
                throw new AppException("Old Password Is Incorrect", BusinessErrorCodes.Unauthorized);
        }


        if (string.IsNullOrEmpty(newPassword))
        {
            throw new AppException("Password is required", BusinessErrorCodes.ValidationError);
        }

        if (!IsValidPassword(newPassword))
        {
            throw new AppException("Password does not meet the required criteria: Minimum 8 characters, at least one uppercase letter, one lowercase letter, one digit, and one special character.", BusinessErrorCodes.ValidationError);
        }

        userDb.PasswordHash = _passwordHashier
            .HashPassword(newPassword);

        _userRepository
            .Update(userDb);
        
        await _userRepository
            .SaveAsync();
    }

    public async Task CreateSelfRegisteredAsync(CreateSelfUserReuqest createUser)
    {
        if (string.IsNullOrWhiteSpace(createUser.Username))
            throw new AppException("Username is required", BusinessErrorCodes.ValidationError);

        if (string.IsNullOrWhiteSpace(createUser.Password))
            throw new AppException("Password is required", BusinessErrorCodes.ValidationError);

        var existingUser = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Name != null && x.Name.ToLower() == createUser.Username.ToLower())
            .FirstOrDefaultAsync();

        if (existingUser != null)
            throw new AppException($"User with username '{createUser.Username}' already exists.", BusinessErrorCodes.ValidationError);

        var existingUserEmail = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Email != null && x.Email.ToLower() == createUser.Email.ToLower())
            .FirstOrDefaultAsync();

        if (existingUserEmail != null)
            throw new AppException("User with this email is already exists", BusinessErrorCodes.ValidationError);

        var newUser = new UserDb
        {
            Name = createUser.Username,
            Email = createUser.Email,
            Role = UserRoleDb.SuperAgent,
            PasswordHash = _passwordHashier.HashPassword(createUser.Password),
            CreatedAt = DateTime.UtcNow,
            UsdcWallet = createUser.UsdcWallet
        };

        await _userRepository
            .InsertAsync(newUser);

        await _userRepository
            .SaveAsync();
    }

    public bool IsValidPassword(string newPassword)
    {
        var passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._])[A-Za-z\d@$!%*?&._]{8,}$";

        var regex = new Regex(passwordPattern);

        return regex.IsMatch(newPassword);
    }

    public async Task UpdateUserWalletAsync(int userId, string newWalletAddress)
    {
        if (string.IsNullOrWhiteSpace(newWalletAddress))
            throw new AppException("Wallet address is required.", BusinessErrorCodes.ValidationError);

        if (!WalletService.IsEthAddress(newWalletAddress))
            throw new AppException("Invalid wallet address format.", BusinessErrorCodes.ValidationError);

        var user = await _userRepository
            .Query()
            .Where(x => x.DeletedAt == null)
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync()
            ?? throw new AppException("User not found", BusinessErrorCodes.NotFound);

        user.UsdcWallet = newWalletAddress.Trim();
        
        _userRepository
            .Update(user);

        await _userRepository
            .SaveAsync();
    }
}
