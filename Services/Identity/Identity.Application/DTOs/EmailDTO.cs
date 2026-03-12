using System.ComponentModel.DataAnnotations;
namespace Identity.Application.DTOs ;

public class EmailDTO
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = null!;
}
