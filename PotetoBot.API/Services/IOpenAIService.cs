namespace PotetoBot.API.Services;

public interface IOpenAIService
{
    Task<string> GetResponseAsync(
        string prompt,
        CancellationToken cancellationToken = default
    );
}
