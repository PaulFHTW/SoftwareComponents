using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace RestAPI.Queue;
public interface IRabbitSender{
    public void SendMessage(string message);
}