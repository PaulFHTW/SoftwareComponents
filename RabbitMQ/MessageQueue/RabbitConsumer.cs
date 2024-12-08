using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace MessageQueue;
public class RabbitConsumer : IRabbitConsumer {
    public void ReceiveMessage(){
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
        channel.BasicCancel(consumerTag);

        channel.Close();
        conn.Close();
    }

    private string? consumerTag;
    private IModel channel;
    public async Task RegisterConsumer(Func<string, string> messageHandler)
    {
        ConnectionFactory factory = new ConnectionFactory();
        factory.Uri = new Uri("amqp://user:password@rabbitmq:5672/");
        factory.ClientProvidedName = "RabbitSender";
        IConnection? conn; 
        while (true)
        {
            try
            {
                conn = factory.CreateConnection();
                break;
            }
            catch (BrokerUnreachableException e) { }
        }
        channel = conn.CreateModel();

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
        
        consumer.Received += (_, msg) =>
        {
            messageHandler(Encoding.UTF8.GetString(msg.Body.ToArray()));
        };

        consumerTag = channel.BasicConsume(queueName, autoAck: false, consumer);
    }
    
    public void CancelConsumer() {
        if(consumerTag != null){
            channel.BasicCancel(consumerTag);
        }
    }
}