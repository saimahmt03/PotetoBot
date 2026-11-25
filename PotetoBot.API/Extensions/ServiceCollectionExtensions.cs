using Microsoft.EntityFrameworkCore;
using PotetoBot.API.Data;
using PotetoBot.API.Services;

namespace PotetoBot.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Controller Configuration for Endpoints
        services.AddControllers();

        // OpenAI Configuration
        services.AddOpenApi();

        return services;
    }

    // DB Context
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<DBContext>(
            options => options.UseSqlServer(connectionString)
            );

        return services;
    }

    // Custome Services
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IOpenAIService, OpenAIService>();

        return services;
    }

}

