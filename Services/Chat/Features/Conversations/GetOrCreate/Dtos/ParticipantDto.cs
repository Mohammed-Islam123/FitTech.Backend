namespace Chat.Features.Conversations.GetOrCreate.Dtos;

public record ParticipantDto(
    Guid UserId,
    string? UserName,
    string? ProfilePhotoUrl,
    bool IsCoach
);
