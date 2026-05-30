namespace Identity.Application.DTOs ;
public class ProfileDTO
{
    public Guid UserId { get; set; }
    public string? FirstName { get; set; } = null!;
    public string? LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? UserName { get; set; } = null!;
    public DateTime? LastLoginAt { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public bool EmailConfirmed { get; set; }
}
