namespace RabbitMQ;

public interface IRabbitClient : IDisposable
{
    public void SendMessage(RabbitQueue queue, string message);
    public void RegisterConsumer(RabbitQueue queue, Func<string, Task<string>> messageHandler);
    public void CancelConsumer();
}