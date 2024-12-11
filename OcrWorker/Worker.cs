using System.Text.Json;
using DAL.Entities;
using ElasticSearch;
using MessageQueue;
using MessageQueue.Messages;
using NMinio;
using ILogger = Logging.ILogger;

namespace NPaperless.OCRLibrary;

public class Worker : IWorker
{
    private readonly IRabbitConsumer _rabbitConsumer;
    private readonly INMinioClient _minioClient;
    private readonly ISearchIndex _searchIndex;
    private readonly ILogger _logger;
    
    public Worker(IRabbitConsumer rabbitConsumer, INMinioClient minioClient, ISearchIndex searchIndex, ILogger logger)
    {
        _rabbitConsumer = rabbitConsumer;
        _minioClient = minioClient;
        _searchIndex = searchIndex;
        _logger = logger;
    }

    private async Task<string> Consumer(string message)
    {
        try
        {
            _logger.Info($"Message Received: {message}");
            var documentTitle = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.DocumentTitle;
            var documentId = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.DocumentId;
            var documentUploadDate = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.UploadDate;
            _logger.Info($"Performing OCR for document {documentTitle}");
            await PerformOcr(documentId, documentTitle, documentUploadDate);
        }
        catch (Exception _)
        {
            // ignored
        }

        return message;
    }

    private async Task PerformOcr(int id, string title, DateTime uploadDate)
    {
        var stream = await _minioClient.Download(id.ToString());

        if (stream == null)
        {
            _logger.Error("File not found");
            return;
        }
    
        _logger.Info("Received stream...");
        var ocrClient = new OcrClient(new OcrOptions());
        var ocrContentText = ocrClient.OcrPdf(stream);
        _logger.Info("OCR completed!");
    
        // Add document to kibana
        var document = new Document(id, title, ocrContentText, uploadDate);
        await _searchIndex.AddDocumentAsync(document);
        _logger.Info("Document added to elastic search!");
    }

    public void Start()
    {
        _rabbitConsumer.RegisterConsumer(Consumer);
    }

    public void Stop()
    {
        _rabbitConsumer.CancelConsumer();
    }
}