namespace Shared.Events;

public record UserRegisteredEvent
{
    public Guid UserId   { get; init; }
    public string Email  { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public DateTime RegisteredAt { get; init; }
}