using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;

namespace MessageQueue;
public class RabbitSender : IRabbitSender {
    public void SendMessage(string message){
        ConnectionFactory factory= new ConnectionFactory();
        factory.Uri = new Uri("amqp://user:password@rabbitmq:5672/");
        factory.ClientProvidedName = "RabbitSender";
        IConnection conn = factory.CreateConnection();
        RabbitMQ.Client.IModel channel = conn.CreateModel();

        string exchangeName = "NPaperless";
        string routingKey = "NPaperless-Routing-Key";
        string queueName = "NPaperlessQueue";

        channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queueName, exchangeName, routingKey, arguments: null);

        byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: messageBodyBytes);

        channel.Close();
        conn.Close();
    }
}