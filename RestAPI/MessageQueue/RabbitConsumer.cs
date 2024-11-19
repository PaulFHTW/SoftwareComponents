using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RestAPI.Queue;
public class RabbitConsumer : IRabbitConsumer{
    public int ReceiveMessage(){
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
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, args) => {
            var body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"Message Received: {message}");

            channel.BasicAck(args.DeliveryTag, multiple: false);
        };

        string consumerTag = channel.BasicConsume(queueName, autoAck: false, consumer);

        Console.ReadLine();

        channel.BasicCancel(consumerTag);

        channel.Close();
        conn.Close();
        return 0;
    }
}