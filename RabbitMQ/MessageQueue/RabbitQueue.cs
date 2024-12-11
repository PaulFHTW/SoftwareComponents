namespace RabbitMQ;

public class RabbitQueue
{
    public string QueueName { get; set; } = "";
    public string ExchangeName { get; set; } = "";
}

public abstract class RabbitQueueType
{
    public static readonly RabbitQueue OcrRequestQueue = new()
    {
        QueueName = "OcrRequest",
        ExchangeName = "OcrRequestExchange"
    };
    
    public static readonly RabbitQueue OcrResponseQueue = new()
    {
        QueueName = "OcrResponse",
        ExchangeName = "OcrResponseExchange"
    };
    
    public static readonly List<RabbitQueue> All =
    [
        OcrRequestQueue,
        OcrResponseQueue
    ];
}