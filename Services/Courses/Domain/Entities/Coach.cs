namespace Courses.Domain.Entities;

/// <description>
/// Represents a coach profile linked to an Identity service user.
/// </description>
public class Coach
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Bio { get; set; }
    public string? Specialties { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Program> Programs { get; set; } = [];
}
