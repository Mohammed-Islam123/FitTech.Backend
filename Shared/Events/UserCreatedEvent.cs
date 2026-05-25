using Shared.Enums;

namespace Shared.Events;

public record UserCreatedEvent
{
    public Guid UserId { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public bool IsCoach { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public DateTime RegisteredAt { get; init; }
}