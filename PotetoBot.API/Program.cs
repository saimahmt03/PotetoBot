using PotetoBot.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices()
    .AddDatabase(builder.Configuration)
    .AddApplicationServices();

var app = builder.Build();

app.ConfigurePipeline();

app.Run();
