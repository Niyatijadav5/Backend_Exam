using System.ComponentModel.DataAnnotations;

namespace SupportTicketManagement.DTO;

public class StatusDto
{
    [Required(ErrorMessage ="Status is required")]
    public string Status { get; set; }
}