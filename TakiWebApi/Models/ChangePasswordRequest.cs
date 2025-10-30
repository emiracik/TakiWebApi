using System.ComponentModel.DataAnnotations;

namespace TakiWebApi.Models;

public class ChangePasswordRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = string.Empty;
}