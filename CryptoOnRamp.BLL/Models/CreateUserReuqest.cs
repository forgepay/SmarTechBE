namespace CryptoOnRamp.BLL.Models;

public class CreateUserReuqest
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public int? CreatedById { get; set; }
    public UserRole Role { get; set; } = default!;

    public string UsdcWallet { get; set; } = "";
}

public class CreateSelfUserReuqest
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string UsdcWallet { get; set; } = "";
}


