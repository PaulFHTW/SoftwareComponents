namespace MessageQueue.Messages;

public class DocumentUploadedMessage
{
    public string MinioPath { get; set; }
    public int DocumentId { get; set; }
    public string DocumentTitle { get; set; }
    public string Message { get; set; }
    
    public DocumentUploadedMessage(string minioPath, int documentId, string documentTitle, string message)
    {
        MinioPath = minioPath;
        DocumentId = documentId;
        DocumentTitle = documentTitle;
        Message = message;
    }
}