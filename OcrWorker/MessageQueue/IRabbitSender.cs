using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;

namespace RestAPI.Queue;
public interface IRabbitSender{
    public void SendMessage(string message);
}