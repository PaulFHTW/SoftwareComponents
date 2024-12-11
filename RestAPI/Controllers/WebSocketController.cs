using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ILogger = Logging.ILogger;

namespace RestAPI.Controllers;

[ApiController]
[Route("status")]
public class WebSocketController(DocumentEventHandler documentEventHandler, ILogger logger) : ControllerBase
{
    [Route("")]
    public async Task HandleSocket()
    {
        try
        {
            logger.Info("WebSocket request received.");
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                documentEventHandler.DocumentUpdated += async (_, args) =>
                {
                    logger.Info("Document status update event invoked.");
                    if(args is not DocumentEventArgs dargs) return;
                    
                    var message = JsonSerializer.Serialize(new { id = dargs.DocumentId, status = dargs.Status.ToString() });
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                };
            
                while(!webSocket.CloseStatus.HasValue)
                {
                    await UpdateStatus(webSocket);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        } catch (Exception e)
        {
            logger.Error(e.Message);
        }
    }
    
    private static async Task UpdateStatus(WebSocket webSocket)
    {
        await webSocket.SendAsync("keepalive"u8.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
        await Task.Delay(500);
    }
}