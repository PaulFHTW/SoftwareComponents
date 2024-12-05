using System.Text;
using Microsoft.AspNetCore.Identity;
using RabbitMQ.Client;

namespace MessageQueue;

public class RabbitInitalizer : IRabbitInitalizer{
    public void RabbitInit(){
	IConnection? conn = null;
	RabbitMQ.Client.IModel? channel = null;
		while(true) {
			try {
				ConnectionFactory factory= new ConnectionFactory();
				factory.Uri = new Uri("amqp://user:password@rabbitmq:5672/");
				factory.ClientProvidedName = "RabbitSender";
				conn = factory.CreateConnection();
				channel = conn.CreateModel();

				string exchangeName = "NPaperless";
				string routingKey = "NPaperless-Routing-Key";
				string queueName = "NPaperlessQueue";

				channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
				channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
				channel.QueueBind(queueName, exchangeName, routingKey, arguments: null);

				byte[] messageBodyBytes = Encoding.UTF8.GetBytes("Hello World!");

				channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: messageBodyBytes);
			break;
			}
			catch(Exception ex)
			{}
		}

        channel!.Close();
        conn!.Close();
    }
}
