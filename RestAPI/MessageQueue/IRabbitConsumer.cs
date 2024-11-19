using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RestAPI.Queue;
public interface IRabbitConsumer{
    public int ReceiveMessage();
}