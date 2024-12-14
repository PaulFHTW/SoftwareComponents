using System.Text.Json;
using BLL.Search;
using DAL.Entities;
using MessageQueue.Messages;
using NMinio;
using RabbitMQ;
using ILogger = Logging.ILogger;

namespace NPaperless.OCRLibrary;

public class Worker : IWorker
{
    private readonly IRabbitClient _rabbitClient;
    private readonly INMinioClient _minioClient;
    private readonly IOcrClient _ocrClient;
    private readonly ISearchIndex _searchIndex;
    private readonly ILogger _logger;
    
    public Worker(IRabbitClient rabbitClient, INMinioClient minioClient, IOcrClient ocrClient, ISearchIndex searchIndex, ILogger logger)
    {
        _rabbitClient = rabbitClient;
        _minioClient = minioClient;
        _ocrClient = ocrClient;
        _searchIndex = searchIndex;
        _logger = logger;
    }

    private async Task<string> Consumer(string message)
    {
        try
        {
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
        var ocrContentText = _ocrClient.OcrPdf(stream);
        _logger.Info("OCR completed!");
    
        // Add document to kibana
        var document = new Document(id, title, ocrContentText, uploadDate);
        await _searchIndex.AddDocumentAsync(document);
        _rabbitClient.SendMessage(RabbitQueueType.OcrResponseQueue, JsonSerializer.Serialize(new DocumentScannedMessage(id, title, true, "Document was scanned successfully!")));
        _logger.Info("Document added to elastic search!");
    }

    public void Start()
    {
        _rabbitClient.RegisterConsumer(RabbitQueueType.OcrRequestQueue, Consumer);
    }

    public void Stop()
    {
        _rabbitClient.CancelConsumer();
    }

    public void Dispose()
    {
        _logger.Info("Running cleanup...");
        _rabbitClient.Dispose();
        _ocrClient.Dispose();
    }
}