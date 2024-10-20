using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<FileContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

Console.WriteLine($"Connection string: {builder.Configuration.GetConnectionString("Database")}");

builder.Services.AddScoped<IFileRepository, FileRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FileContext>();

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

app.MapControllers();

app.Run();