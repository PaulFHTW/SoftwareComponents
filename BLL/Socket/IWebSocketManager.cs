using System.Net.WebSockets;

namespace BLL.Socket;

public interface IWebSocketManager
{
    public Task HandleSocket(WebSocket webSocket);
}