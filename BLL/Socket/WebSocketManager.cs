using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Logging;

namespace BLL.Socket;

public class WebSocketManager(DocumentEventHandler documentEventHandler, ILogger logger) : IWebSocketManager
{
    public async Task HandleSocket(WebSocket webSocket)
    {
        var cancellationToken = new CancellationTokenSource();
        var updateHandler = new EventHandler(async void (_, args) => await OnDocumentUpdated(args, webSocket));
        documentEventHandler.DocumentUpdated += updateHandler;
            
        while(webSocket.State == WebSocketState.Open) await UpdateStatus(webSocket, cancellationToken.Token);

        documentEventHandler.DocumentUpdated -= updateHandler;
        await webSocket.CloseAsync(
            webSocket.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
            webSocket.CloseStatusDescription,
            CancellationToken.None
        );
        await cancellationToken.CancelAsync();
        logger.Info("WebSocket connection closed.");
    }
    
    private async Task OnDocumentUpdated(EventArgs args, WebSocket webSocket)
    {
        logger.Info("Document status update event invoked.");
        if(args is not DocumentEventArgs dargs) return;
                    
        var message = JsonSerializer.Serialize(new { id = dargs.DocumentId, status = dargs.Status.ToString() });
        await webSocket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
    
    private static async Task UpdateStatus(WebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        
        await webSocket.SendAsync("keepalive"u8.ToArray(), WebSocketMessageType.Text, true, cancellationToken);
        webSocket.ReceiveAsync(buffer, cancellationToken);
        await Task.Delay(500);
    }
}