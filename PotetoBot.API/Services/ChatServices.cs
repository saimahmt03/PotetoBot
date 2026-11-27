using Google.GenAI.Types;
using Google.GenAI;
using Microsoft.EntityFrameworkCore;
using PotetoBot.API.Data;
using PotetoBot.API.Entities;

namespace PotetoBot.API.Services;

public class ChatServices
{
    private readonly DBContext _dbContext;
    private readonly IGeminiService _geminiService;

    public ChatServices(DBContext dbContext, IGeminiService geminiService)
    {
        _dbContext = dbContext;
        _geminiService = geminiService;
    }

    public async Task<string> SendMessageAsync(int conversationId, string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty.", nameof(message));
        }

        Conversation? conversation = await _dbContext.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken);

        if(conversation == null)
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        ChatMessage userMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "User",
            Content = message
        };

        _dbContext.ChatMessages.Add(userMessage);
        conversation.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Retrieve conversation messages including the newly added user message.
        List<ChatMessage> history = await _dbContext.ChatMessages
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        string prompt = BuildConversationPrompt(history);

        string reply = await _geminiService.GenerateTextAsync(prompt, cancellationToken);

        ChatMessage assistantMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "Assistant",
            Content = reply
        };

        _dbContext.ChatMessages.Add(assistantMessage);
        conversation.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return reply;
    }

    private static string BuildConversationPrompt(IEnumerable<ChatMessage> history)
    {
        var lines = history.Select(m =>
        {
            string speaker = string.Equals(m.Role, "User", StringComparison.OrdinalIgnoreCase)
                ? "User"
                : "Assistant";
            return $"{speaker}: {m.Content}";
        });

        return string.Join(System.Environment.NewLine, lines)
               + System.Environment.NewLine
               + "Assistant:";
    }

}
