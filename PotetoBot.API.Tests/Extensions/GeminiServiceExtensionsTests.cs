using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PotetoBot.API.Extensions;
using PotetoBot.API.Options;
using PotetoBot.API.Services;
using Xunit;

namespace PotetoBot.API.Tests.Extensions;

public class GeminiServiceExtensionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ProtectGeminiApiKey_WhenPlainKeyIsInvalid_ThrowsArgumentException(string? plainKey)
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            GeminiServiceExtensions.ProtectGeminiApiKey(plainKey!));

        Assert.Equal("plainApiKey", ex.ParamName);
    }

    [Fact]
    public void ProtectGeminiApiKey_WhenValidPlainKey_ReturnsNonEmptyProtectedPayload()
    {
        // Arrange
        const string plainKey = "test-gemini-api-key-12345";

        // Act
        string protectedKey = GeminiServiceExtensions.ProtectGeminiApiKey(plainKey);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(protectedKey));
        Assert.NotEqual(plainKey, protectedKey);
        Assert.StartsWith("CfDJ8", protectedKey);
    }

    [Fact]
    public void AddGeminiServices_WhenApiKeyMissing_ThrowsInvalidOperationExceptionOnResolve()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Gemini:ApiKey"] = "",
            ["Gemini:Model"] = "gemini-2.5-flash"
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.AddGeminiServices(configuration);
        using var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            provider.GetRequiredService<IGeminiService>());

        Assert.Contains("Gemini:ApiKey", ex.Message);
    }

    [Fact]
    public void AddGeminiServices_WhenProtectedKeyMatchesSharedKeyRing_ResolvesIGeminiService()
    {
        // Arrange
        const string rawApiKey = "sample-gemini-secret-key";
        const string appName = "PotetoBot.API";
        const string purpose = "Gemini.ApiKey";

        // Use a shared key directory in Temp so protection & DI resolution share the same key ring
        string tempKeyDir = Path.Combine(Path.GetTempPath(), "PotetoBotTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempKeyDir);

        try
        {
            var seedServices = new ServiceCollection();
            seedServices.AddDataProtection()
                .SetApplicationName(appName)
                .PersistKeysToFileSystem(new DirectoryInfo(tempKeyDir));

            using var seedProvider = seedServices.BuildServiceProvider();
            var protector = seedProvider.GetRequiredService<IDataProtectionProvider>().CreateProtector(purpose);
            string protectedKey = protector.Protect(rawApiKey);

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Gemini:ApiKey"] = protectedKey,
                    ["Gemini:Model"] = "gemini-2.5-flash"
                })
                .Build();

            var appServices = new ServiceCollection();
            appServices.AddDataProtection()
                .SetApplicationName(appName)
                .PersistKeysToFileSystem(new DirectoryInfo(tempKeyDir));

            appServices.Configure<GeminiOptions>(config.GetSection(GeminiOptions.SectionName));
            appServices.AddSingleton<IGeminiService>(sp =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GeminiOptions>>().Value;
                if (string.IsNullOrWhiteSpace(opts.ApiKey))
                    throw new InvalidOperationException("Configuration value 'Gemini:ApiKey' is missing or empty.");

                var appProtector = sp.GetRequiredService<IDataProtectionProvider>().CreateProtector(purpose);
                string decrypted = appProtector.Unprotect(opts.ApiKey);
                return new GeminiService(decrypted, opts.Model);
            });

            using var appProvider = appServices.BuildServiceProvider();

            // Act
            var service = appProvider.GetRequiredService<IGeminiService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<GeminiService>(service);
        }
        finally
        {
            if (Directory.Exists(tempKeyDir))
                Directory.Delete(tempKeyDir, recursive: true);
        }
    }

}

