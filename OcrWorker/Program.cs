using MessageQueue;
using ElasticSearch;
using NMinio;
using Logging;
using NPaperless.OCRLibrary;
using ILogger = Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ILogger, Logger>();
builder.Services.AddScoped<INMinioClient>(sp => ActivatorUtilities.CreateInstance<MinioFactory>(sp, builder.Configuration, sp.GetRequiredService<ILogger>()).Create());
builder.Services.AddScoped<ISearchIndex, SearchIndex>();

Console.WriteLine("Initializing RabbitMQ...");
RabbitInitalizer.RabbitInit();
builder.Services.AddScoped<IRabbitConsumer, RabbitConsumer>();

builder.Services.AddScoped<IWorker, Worker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger>();
        
        logger.Info("Starting worker...");
        var worker = services.GetRequiredService<IWorker>();
        worker.Start();
        logger.Info("Worker started!");
    }
    catch (Exception e)
    {
        Console.WriteLine("Error during service resolution: " + e.Message);
    }
}

while (true);
return;