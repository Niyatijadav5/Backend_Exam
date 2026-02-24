using System.ComponentModel.DataAnnotations;

namespace SupportTicketManagement.DTO;

public class CreateTicketDto
{
    [Required(ErrorMessage ="Title is required")]
    [MinLength(5,ErrorMessage ="Title must be at least 5 characters")]
    public string Title { get; set; }

    [Required(ErrorMessage ="Description is required")]
    [MinLength(10,ErrorMessage ="Description must be at least 10 characters")]
    public string Description { get; set; }

    [Required(ErrorMessage ="Priority is required")]
    public string Priority { get; set; }
}