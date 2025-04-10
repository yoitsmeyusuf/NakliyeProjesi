namespace NakliyeApp.DTOs;

public class ChatMessageDto
{
    public required int SenderId { get; set; }
    public required int RecipientId { get; set; }
    public required string Content { get; set; }
}
