namespace RestAPI.Queue.Messages;

public class DocumentUploadedMessage
{
    public string DocumentId { get; set; }
    public string Message { get; set; }
    
    public DocumentUploadedMessage(string documentId, string message)
    {
        DocumentId = documentId;
        Message = message;
    }
}