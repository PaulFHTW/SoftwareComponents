namespace RabbitMQ;

public interface IRabbitClient : IDisposable
{
    public void SendMessage(string message);
    public void ReceiveMessage();
    public void RegisterConsumer(Func<string, Task<string>> messageHandler);
    public void CancelConsumer();
}