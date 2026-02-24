using System.ComponentModel.DataAnnotations;

namespace SupportTicketManagement.DTO;

public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must longer than 6 characters.")]
    public string Password { get; set; }
}