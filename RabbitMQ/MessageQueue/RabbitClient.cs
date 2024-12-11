using System.Text;
using Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ;

public class RabbitClient : IRabbitClient
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly string _queueName;
    private readonly string _routingKey;
    private readonly ILogger _logger;

    private readonly SemaphoreSlim _semaphoreSlim;
    private string _consumerTag;
    
    public RabbitClient(IConnection connection, IModel channel, string exchangeName, string queueName, string routingKey, ILogger logger)
    {
        _connection = connection;
        _channel = channel;
        _exchangeName = exchangeName;
        _queueName = queueName;
        _routingKey = routingKey;
        _logger = logger;
        
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _consumerTag = string.Empty;
        
        _logger.Info("RabbitMQ client created!");
    }

    public void Dispose()
    {
        _semaphoreSlim.Dispose();
        _channel.Dispose();
        _connection.Dispose();
    }

    public void SendMessage(string message)
    {
        var messageBodyBytes = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: _exchangeName, routingKey: _routingKey, basicProperties: null, body: messageBodyBytes);
        _logger.Info($"Message Sent: {message}");
    }

    public void ReceiveMessage()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (_, args) => {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.Info($"Message Received: {message}");
            _channel.BasicAck(args.DeliveryTag, multiple: false);
        };

        var consumerTag = _channel.BasicConsume(_queueName, autoAck: false, consumer);
        _channel.BasicCancel(consumerTag);
    }

    public void RegisterConsumer(Func<string, Task<string>> messageHandler)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.Info($"Message Received: {message}");

            await _semaphoreSlim.WaitAsync();

            try
            {
                await messageHandler(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
            finally
            {
                _channel.BasicAck(args.DeliveryTag, multiple: false);
                _semaphoreSlim.Release();
            }
        };

        _consumerTag = _channel.BasicConsume(_queueName, autoAck: false, consumer);
        _logger.Info($"Consumer registered with tag {_consumerTag}");
    }

    public void CancelConsumer()
    {
        if(_consumerTag != string.Empty){
            _channel.BasicCancel(_consumerTag);
        }
    }
}