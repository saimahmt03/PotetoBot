using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace PotetoBot.API.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ChatClient _chatClient;

    public OpenAIService(IConfiguration configuration)
    {
        string apiKey = configuration["OpenAI:ApiKey"] 
                        ?? throw new InvalidOperationException("OpenAI API key is missing.");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key cannot be empty.");
        }

        string model = configuration["OpenAI:Model"] ?? "gpt-5.1";

        _chatClient = new ChatClient(model: model, apiKey: apiKey);
    }


    public async Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
        }

        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);

        return completion.Content.Count > 0 ? completion.Content[0].Text: string.Empty;
    }


}