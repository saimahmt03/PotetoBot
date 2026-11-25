using PotetoBot.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddDatabase(builder.Configuration)
    .AddApplicationServices()
    .AddGeminiServices(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.Run();
