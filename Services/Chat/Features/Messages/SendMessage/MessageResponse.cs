namespace Chat.Features.Messages.SendMessage;

// Shared response record used by both the Hub and the REST endpoint
public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string? SenderName,
    string? SenderPhotoUrl,
    string Content,
    DateTime SentAt,
    bool IsRead
);