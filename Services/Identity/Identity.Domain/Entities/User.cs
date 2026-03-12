using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? UserName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public bool IsEmailConfirmed { get; set; }
    public bool IsActive { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? MedicalFileUrl { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}