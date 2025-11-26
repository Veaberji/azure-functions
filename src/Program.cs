using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using EDU.Services;
using Azure.AI.Vision.ImageAnalysis;
using Azure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddScoped<IImageProcessingService, ImageProcessingService>()
    .AddScoped<IImageAnalysisService, ImageAnalysisService>()
    .AddScoped<IEmailService, EmailService>()
    .AddScoped<IBlobStorageService, BlobStorageService>()
    .AddHttpClient()
    .AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var endpoint = config["VISION_ENDPOINT"];
        var key = config["VISION_KEY"];
        return new ImageAnalysisClient(new Uri(endpoint!), new AzureKeyCredential(key!));
    });

builder.Build().Run();
