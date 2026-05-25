namespace Shared.Events;

public record EmailConfirmationRequestedEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string Token { get; init; } = null!;
}