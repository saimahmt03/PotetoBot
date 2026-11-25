namespace PotetoBot.API.Services;

public interface IGeminiService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}
