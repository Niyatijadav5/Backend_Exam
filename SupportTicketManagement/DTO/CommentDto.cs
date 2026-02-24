using System.ComponentModel.DataAnnotations;

namespace SupportTicketManagement.DTO;

public class CommentDto
{
    [Required(ErrorMessage ="Comment is required")]
    public string Comment { get; set; }
}