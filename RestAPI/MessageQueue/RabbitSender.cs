using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace RestAPI.Queue;

public class RabbitSender{

    public RabbitSender(){
        ConnectionFactory factory= new ConnectionFactory();    
        factory.Uri = new Uri("amqp://user:password@localhost:9093");
        factory.ClientProvidedName = "RabbitSender";

        IConnection conn = factory.CreateConnection();

        RabbitMQ.Client.IModel channel = conn.CreateModel();

        string exchangeName = "NPaperless";
        string routingKey = "NPaperless-Routing-Key";
        string queueName = "NPaperlessQueue";

        channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
        channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queueName, exchangeName, routingKey, arguments: null);

        byte[] messageBodyBytes = Encoding.UTF8.GetBytes("Hello World!");

        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: messageBodyBytes);

        channel.Close();
        conn.Close();
    }
}