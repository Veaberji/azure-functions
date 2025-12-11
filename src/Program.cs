using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using EDU.Services;
using EDU.Func.Durable.Services;
using Azure.AI.Vision.ImageAnalysis;
using Azure.AI.TextAnalytics;
using Azure;
using Microsoft.Azure.Cosmos;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddScoped<IImageProcessingService, ImageProcessingService>()
    .AddScoped<IImageAnalysisService, ImageAnalysisService>()
    .AddScoped<IEmailService, EmailService>()
    .AddScoped<IBlobStorageService, BlobStorageService>()
    .AddScoped<ITwitterService, TwitterService>()
    .AddScoped<ISentimentService, SentimentService>()
    .AddSingleton<IPaymentGateway, SimulatedPaymentGateway>()
    .AddHttpClient()
    .AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var endpoint = config["VISION_ENDPOINT"];
        var key = config["VISION_KEY"];
        return new ImageAnalysisClient(new Uri(endpoint!), new AzureKeyCredential(key!));
    })
    .AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var endpoint = config["TEXT_ANALYTICS_ENDPOINT"];
        var key = config["TEXT_ANALYTICS_KEY"];
        return new TextAnalyticsClient(new Uri(endpoint!), new AzureKeyCredential(key!));
    })
    .AddSingleton<CosmosClient>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var connectionString = config["COSMOS_DB"];
        return new CosmosClient(connectionString);
    });

builder.Build().Run();
