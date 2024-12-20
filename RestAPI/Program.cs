using System.Reflection;
using BLL.Documents;
using BLL.Search;
using BLL.Socket;
using DAL.Data;
using DAL.Repositories;
using FluentValidation.AspNetCore;
using Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NMinio;
using RabbitMQ;
using RestAPI.Mappings;
using ILogger = Logging.ILogger;
using WebSocketManager = BLL.Socket.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ILogger, Logger>();
builder.Services.AddScoped<IRabbitClient>(sp => ActivatorUtilities.CreateInstance<RabbitFactory>(sp, builder.Configuration, sp.GetRequiredService<ILogger>()).Create("RestAPI"));
builder.Services.AddScoped<INMinioClient>(sp => ActivatorUtilities.CreateInstance<MinioFactory>(sp, builder.Configuration, sp.GetRequiredService<ILogger>()).Create());
builder.Services.AddScoped<ISearchIndex, SearchIndex>();
builder.Services.AddDbContext<DocumentContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentManager, DocumentManager>(); 
builder.Services.AddSingleton<DocumentEventHandler>();
builder.Services.AddSingleton<RabbitStatusUpdateHandler>();
builder.Services.AddSingleton<IWebSocketManager, WebSocketManager>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddLog4Net();
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "NPaperless API",
        Version = "v1",
        Description =  "API for the NPaperless application."
    });
    
    const string xmlFile = "NPaperless.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    Console.WriteLine(xmlFile);
    Console.WriteLine(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI",
        policy =>
        {
            policy.WithOrigins("http://localhost") // Die URL deiner Web-UI
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
    
    // Instantiate event handler singletons
    var eventHandler = scope.ServiceProvider.GetRequiredService<DocumentEventHandler>();
    var rabbitHandler = scope.ServiceProvider.GetRequiredService<RabbitStatusUpdateHandler>();
    
    try
    {
        logger.Info("Trying to establish connection to the database...");

        while (!context.Database.CanConnect())
        {
            logger.Info("Database not reachable. Retrying in 1 second...");
            Thread.Sleep(1000);
        }

        logger.Info("Connection to the database established.");

        context.Database.EnsureCreated();
        logger.Info("Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        logger.Error($"Error applying database migrations: {ex.Message}");
    }
}

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/swagger/v1/swagger.json", "NPaperless API");
    options.RoutePrefix = string.Empty;
});

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};
webSocketOptions.AllowedOrigins.Add("http://localhost");
app.UseWebSockets(webSocketOptions);

app.UseCors("AllowWebUI");
app.UseAuthorization();

app.MapControllers();

app.Run();
