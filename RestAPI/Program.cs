using DAL.Controllers;
using DAL.Data;
using DAL.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using log4net;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Logging;
using RestAPI.Mappings;
using RestAPI.Queue;
using RestAPI.Utility;
using ILogger = RestAPI.Utility.ILogger;
using Minio;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using FileUploader;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
RabbitInitalizer _rabbitMQ = new RabbitInitalizer();
_rabbitMQ.RabbitInit();
builder.Services.AddSingleton<IRabbitInitalizer>(_rabbitMQ);
builder.Services.AddScoped<IRabbitSender, RabbitSender>();

//FileUpload _minioUpload = new FileUpload();
//_minioUpload.Upload();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddDbContext<DocumentContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentController, DocumentController>(); 
builder.Services.AddScoped<ILogger, Logger>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddLog4Net();
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<TodoItemDtoValidator>();

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

/*
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});
*/

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentContext>();

    try
    {
        Console.WriteLine("Versuche, eine Verbindung zur Datenbank herzustellen...");

        while (!context.Database.CanConnect())
        {
            Console.WriteLine("Datenbank ist noch nicht bereit, warte...");
            Thread.Sleep(1000);
        }

        Console.WriteLine("Verbindung zur Datenbank erfolgreich.");

        context.Database.EnsureCreated();
        Console.WriteLine("Datenbankmigrationen erfolgreich angewendet.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler bei der Anwendung der Migrationen: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = "swagger"; 
    });
}
*/

app.UseCors("AllowWebUI");
//app.Urls.Add("http://*:8081");
app.UseAuthorization();

app.MapControllers();

app.Run();
