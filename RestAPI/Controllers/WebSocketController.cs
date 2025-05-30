using System.Diagnostics.CodeAnalysis;
using BLL.Socket;
using Microsoft.AspNetCore.Mvc;
using ILogger = Logging.ILogger;

namespace RestAPI.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("status")]

[ExcludeFromCodeCoverage]
public class WebSocketController(IWebSocketManager webSocketManager, ILogger logger) : ControllerBase
{
    [Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task HandleSocket()
    {
        try
        {
            logger.Info("WebSocket request received.");
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await webSocketManager.HandleSocket(webSocket);
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
}