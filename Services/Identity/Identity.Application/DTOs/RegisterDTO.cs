using System.ComponentModel.DataAnnotations;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Http;
namespace Identity.Application.DTOs ;

public class RegisterDTO
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = null!;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string? FirstName { get; set; }

    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string? LastName { get; set; }

    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool IsCoach { get; set; } = false;

    /// <summary>
    /// Optional profile photo uploaded at sign-up.
    /// </summary>
    public IFormFile? ProfilePicture { get; set; }

    /// <summary>
    /// Optional medical file uploaded at sign-up (PDF, image, etc.)
    /// </summary>
    public IFormFile? MedicalFile { get; set; }

}
