using System.Text.Json;
using DAL.Controllers;
using MessageQueue.Messages;
using RabbitMQ;
using ILogger = Logging.ILogger;

namespace RestAPI.Controllers;

public class RabbitStatusUpdateHandler
{
    private readonly IRabbitClient _rabbitClient;
    private readonly IDocumentManager _documentManager;
    private readonly DocumentEventHandler _documentEventHandler;
    private readonly ILogger _logger;
    
    public RabbitStatusUpdateHandler(IRabbitClient rabbitClient, IDocumentManager documentManager, DocumentEventHandler documentEventHandler, ILogger logger)
    {
        _rabbitClient = rabbitClient;
        _documentManager = documentManager;
        _documentEventHandler = documentEventHandler;
        _logger = logger;
        
        rabbitClient.RegisterConsumer(RabbitQueueType.OcrResponseQueue, Consumer);
    }

    private async Task<string> Consumer(string text)
    {
        try
        {
            var message = JsonSerializer.Deserialize<DocumentScannedMessage>(text);
            if(message == null) return "";
            
            _logger.Info("Received document status update: " + message.DocumentId);
            if (message.Success)
            {
                var document = await _documentManager.GetAsyncById(message.DocumentId);
                document.Content = "Scanned";
                await _documentManager.PutAsync(document.Id, document);
            }
            _documentEventHandler.OnDocumentUpdated(new DocumentEventArgs(message.DocumentId, message.Success ? DocumentStatus.Completed : DocumentStatus.Failed));
            return "";
        } catch (Exception e)
        {
            return "";
        }
    }
}