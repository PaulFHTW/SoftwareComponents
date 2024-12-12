namespace BLL.Socket;

public enum DocumentStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class DocumentEventArgs(int documentId, DocumentStatus status) : EventArgs
{
    public int DocumentId { get; set; } = documentId;
    public DocumentStatus Status { get; set; } = status;
}

public class DocumentEventHandler
{
    public event EventHandler? DocumentUpdated;
    
    public void OnDocumentUpdated(EventArgs e)
    {
        DocumentUpdated?.Invoke(this, e);
    }
}