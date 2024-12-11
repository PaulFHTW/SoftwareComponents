using MessageQueue;
using ElasticSearch;
using NMinio;
using Logging;
using NPaperless.OCRLibrary;
using RabbitMQ;
using ILogger = Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ILogger, Logger>();
builder.Services.AddScoped<INMinioClient>(sp => ActivatorUtilities.CreateInstance<MinioFactory>(sp, builder.Configuration, sp.GetRequiredService<ILogger>()).Create());
builder.Services.AddScoped<ISearchIndex, SearchIndex>();
builder.Services.AddScoped<IOcrClient, OcrClient>();
builder.Services.AddScoped<IRabbitClient>(sp => ActivatorUtilities.CreateInstance<RabbitFactory>(sp, builder.Configuration, sp.GetRequiredService<ILogger>()).Create("OcrWorker"));

builder.Services.AddScoped<IWorker, Worker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    ILogger? logger = null;
    try
    {
        var services = scope.ServiceProvider;
        logger = services.GetRequiredService<ILogger>();
        
        logger.Info("Starting worker...");
        using var worker = services.GetRequiredService<IWorker>();
        worker.Start();
        logger.Info("Worker started!");
        
        while (true);
    }
    catch (Exception e)
    {
        logger?.Info("Error during service resolution: " + e.Message);
    }
}