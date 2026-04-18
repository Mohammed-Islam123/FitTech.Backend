using System.ComponentModel.DataAnnotations;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Identity.Application.DTOs ;
public class UpdateProfileDTO
{
    [Required(ErrorMessage = "User ID is required.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; } = null!;

    public Gender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public IFormFile? ProfilePicture { get; set; }
}
