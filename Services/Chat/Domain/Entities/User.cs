namespace Chat.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsCoach { get; set; }

    // Navigation
    public ICollection<ConversationParticipant> Participations { get; set; } = [];
}