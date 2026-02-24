using System.ComponentModel.DataAnnotations;

namespace SupportTicketManagement.DTO;

public class CreateUserDto
{
    [Required(ErrorMessage ="Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage ="Email is required")]
    [EmailAddress(ErrorMessage ="Invalid email")]
    public string Email { get; set; }

    [Required(ErrorMessage ="Password is required")]
    [MinLength(6,ErrorMessage ="Password must be greate than 6 characters.")]
    public string Password { get; set; }

    [Required(ErrorMessage ="Role is required")]
    public string Role { get; set; }
}