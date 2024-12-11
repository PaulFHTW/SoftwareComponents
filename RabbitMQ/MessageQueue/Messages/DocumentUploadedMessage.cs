namespace MessageQueue.Messages;

public class DocumentUploadedMessage
{
    public int DocumentId { get; set; }
    public string DocumentTitle { get; set; }
    public DateTime UploadDate { get; set; }
    public string Message { get; set; }
    
    public DocumentUploadedMessage(int documentId, string documentTitle, DateTime uploadDate, string message)
    {
        DocumentId = documentId;
        DocumentTitle = documentTitle;
        UploadDate = uploadDate;
        Message = message;
    }
}