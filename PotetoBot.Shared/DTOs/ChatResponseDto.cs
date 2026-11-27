namespace PotetoBot.Shared.DTOs;

public class ChatResponseDto
{
    public int ConversationId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public string AssistantMessage { get; set; } = string.Empty;
}
