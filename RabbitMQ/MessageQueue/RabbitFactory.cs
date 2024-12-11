using Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace RabbitMQ;

public class RabbitFactory(IConfiguration configuration, ILogger logger)
{
    public RabbitClient Create(string name)
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
            ClientProvidedName = name
        };
	    
        var exchangeName = configuration["RabbitMQ:ExchangeName"]; 
        var queueName = configuration["RabbitMQ:QueueName"];
        var routingKey = configuration["RabbitMQ:RoutingKey"];
	    
        IConnection? conn = null;
        IModel? channel = null;
		
        while(true) {
            try {
                conn = factory.CreateConnection();
                channel = conn.CreateModel();
                
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queueName, exchangeName, routingKey, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                break;
            }
            catch (Exception ex)
            {
                if(conn!.IsOpen) conn.Close();
                if(channel!.IsOpen) channel.Close();
                Thread.Sleep(100);
            }
        }
        
        return new RabbitClient(conn, channel, exchangeName, queueName, routingKey, logger);
    }
}