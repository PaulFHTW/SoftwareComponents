// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using NPaperless.OCRLibrary;
using MessageQueue;
using MessageQueue.Messages;
using DAL.Entities;
using ElasticSearch;
using NMinio;

var builder = WebApplication.CreateBuilder(args);

var minioClient = new MinioFactory(builder.Configuration).Create();

var rabbitConsumer = new RabbitConsumer();
await rabbitConsumer.RegisterConsumer(Consumer);

Console.WriteLine("OCR consumer registered.");

while (true);
return;

string Consumer(string message)
{
    try
    {
        Console.WriteLine($"Message Received: {message}");
        var documentTitle = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.DocumentTitle;
        var documentId = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.DocumentId;
        Console.WriteLine($"Performing OCR for document {documentTitle}");
        PerformOcr(documentId, documentTitle);
    }
    catch (Exception _)
    {
        // ignored
    }

    return message;
}

async Task PerformOcr(int id, string title)
{
    var stream = await minioClient.Download(id.ToString());

    if (stream == null)
    {
        Console.WriteLine("File not found");
        return;
    }
    
    Console.WriteLine("Received stream...");
    var ocrClient = new OcrClient(new OcrOptions());
    var ocrContentText = ocrClient.OcrPdf(stream);
    Console.WriteLine(ocrContentText);
    
    // Add document to kibana
    Document document = new Document(id, title, ocrContentText, DateTime.Now);
    var ElasticSearchIndex = new SearchIndex();
    ElasticSearchIndex.AddDocumentAsync(document);
}