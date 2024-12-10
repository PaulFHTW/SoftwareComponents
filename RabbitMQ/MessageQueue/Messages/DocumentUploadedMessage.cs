namespace MessageQueue.Messages;

public class DocumentUploadedMessage
{
    public int DocumentId { get; set; }
    public string DocumentTitle { get; set; }
    public string Message { get; set; }
    
    public DocumentUploadedMessage(int documentId, string documentTitle, string message)
    {
        DocumentId = documentId;
        DocumentTitle = documentTitle;
        Message = message;
    }
}