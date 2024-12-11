namespace MessageQueue.Messages;

public class DocumentScannedMessage
{
    public int DocumentId { get; set; }
    public string DocumentTitle { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    
    public DocumentScannedMessage(int documentId, string documentTitle, bool success, string message)
    {
        DocumentId = documentId;
        DocumentTitle = documentTitle;
        Success = success;
        Message = message;
    }
}