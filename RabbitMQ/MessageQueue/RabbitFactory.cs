using System.Diagnostics.CodeAnalysis;
using Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace RabbitMQ;

[ExcludeFromCodeCoverage]
public class RabbitFactory(IConfiguration configuration, ILogger logger)
{
    public RabbitClient Create(string name)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
            ClientProvidedName = name
        };
	    
        var routingKey = configuration["RabbitMQ:RoutingKey"] ?? "NPaperless-Routing-Key";
	    
        IConnection? conn = null;
        IModel? channel = null;
		
        while(true)
            try {
                conn = factory.CreateConnection();
                channel = conn.CreateModel();
                
                RabbitQueueType.All.ForEach(rabbitQueue =>
                {
                    channel.ExchangeDeclare(rabbitQueue.ExchangeName, ExchangeType.Direct);
                    channel.QueueDeclare(rabbitQueue.QueueName, false, false, false, null);
                    channel.QueueBind(rabbitQueue.QueueName, rabbitQueue.ExchangeName, routingKey, null);
                });
                channel.BasicQos(0, 1, false);
                break;
            }
            catch (Exception ex)
            {
                if(conn!.IsOpen) conn.Close();
                if(channel!.IsOpen) channel.Close();
                Thread.Sleep(100);
            }

        return new RabbitClient(conn, channel, routingKey, logger);
    }
}