using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;

namespace MessageQueue;
public interface IRabbitSender{
    public void SendMessage(string message);
}