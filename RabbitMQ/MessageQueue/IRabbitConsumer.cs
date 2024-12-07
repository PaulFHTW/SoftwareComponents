using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueue;
public interface IRabbitConsumer{
    public void ReceiveMessage();
    public Task RegisterConsumer(Func<string, string> messageHandler);
    public void CancelConsumer();
}
