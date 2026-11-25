using Google.GenAI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PotetoBot.API.Options;
using PotetoBot.API.Services;
using System.Security.Cryptography;

namespace PotetoBot.API.Extensions;

public static class GeminiServiceExtensions
{
    private const string ApplicationName = "PotetoBot.API";
    private const string ApiKeyPurpose = "Gemini.ApiKey";

    public static IServiceCollection AddGeminiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        services.AddDataProtection().SetApplicationName(ApplicationName);

        services.AddSingleton<IGeminiService>( sp =>
        {
            var options = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("Configuration value 'Gemini:ApiKey' is missing or empty.");
            }

            var protector = sp.GetRequiredService<IDataProtectionProvider>().CreateProtector(ApiKeyPurpose);

            string decryptedApiKey = protector.Unprotect(options.ApiKey);

            return new GeminiService(decryptedApiKey, options.Model);
        });

        return services;
    }

    public static string ProtectGeminiApiKey(string plainApiKey)
    {
        if (string.IsNullOrWhiteSpace(plainApiKey))
        {
            throw new ArgumentException("API key cannot be null or empty.", nameof(plainApiKey));
        }

        var tempServices = new ServiceCollection();
        
        tempServices.AddDataProtection().SetApplicationName(ApplicationName);

        using var provider = tempServices.BuildServiceProvider();

        var protector = provider.GetRequiredService<IDataProtectionProvider>().CreateProtector(ApiKeyPurpose);

        return protector.Protect(plainApiKey);
    }

    private static string UprotectGeminiApiKey(IDataProtector protector, string protectedApiKey)
    {
        if (string.IsNullOrWhiteSpace(protectedApiKey))
        {
            throw new InvalidOperationException("Configuration value 'Gemini:ApiKey' is missing or empty.");
        }

        try
        {
            return protector.Unprotect(protectedApiKey);
        }
        catch(CryptographicException ex)
        {
            throw new InvalidOperationException(
                "Failed to decrypt 'Gemini:ApiKey'. Ensure it was protected using the same application name, purpose, and Data Protection key ring.",
                ex);
        }
    }

}

