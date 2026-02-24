using System.ComponentModel.DataAnnotations;

namespace SupportTicketManagement.DTO;

public class AssignDto
{
    [Required(ErrorMessage ="UserId is required")]
    public int UserId { get; set; }
}