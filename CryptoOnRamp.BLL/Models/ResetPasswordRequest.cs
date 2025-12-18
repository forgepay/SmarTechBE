using System.ComponentModel.DataAnnotations;

namespace CryptoOnRamp.BLL.Models;

public class ResetPasswordRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
