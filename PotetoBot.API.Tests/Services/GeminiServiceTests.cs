using PotetoBot.API.Services;
using Xunit;

namespace PotetoBot.API.Tests.Services;
public class GeminiServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WhenApiKeyIsNullOrWhitespace_ThrowsArgumentException(string? invalidKey)
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new GeminiService(invalidKey!, "gemini-2.5-flash"));

        // Assert
        Assert.Equal("apiKey", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenValidApiKeyAndBlankModelProvided_DoesNotThrow()
    {
        // Arrange
        const string testApiKey = "AIzaSyTestKey_NotReal_ForUnitTestsOnly";

        // Act
        var service = new GeminiService(testApiKey, "   ");

        // Assert
        Assert.NotNull(service);
    }
}

