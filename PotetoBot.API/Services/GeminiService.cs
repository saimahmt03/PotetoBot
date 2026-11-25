using Google.GenAI;
using Microsoft.Extensions.Configuration;

namespace PotetoBot.API.Services;

public class GeminiService : IGeminiService
{
    private readonly Client _client;
    private readonly string _model;

    public GeminiService(string apiKey, string model)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("Gemini API key cannot be empty.", nameof(apiKey));
        }

        _client = new Client(apiKey: apiKey);
        _model = string.IsNullOrWhiteSpace(model) ? "gemini-2.5-flash" : model;
    }

    public async Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var response = await _client.Models.GenerateContentAsync(
            model: _model,
            contents: prompt
        );

        return response.Text ?? string.Empty;
    }

}
