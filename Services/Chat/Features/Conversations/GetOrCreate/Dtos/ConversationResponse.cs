namespace Chat.Features.Conversations.GetOrCreate.Dtos;
public record ConversationResponse(
    Guid Id,
    DateTime CreatedAt,
    DateTime? LastMessageAt,
    List<ParticipantDto> Participants
);
