using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RestAPI.Queue;
public interface IRabbitConsumer{
    public void ReceiveMessage();
    public void RegisterConsumer(Func<string, string> messageHandler);
}