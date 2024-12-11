using Minio;
using Logging;
using ILogger = Logging.ILogger;

namespace NMinio;

public class MinioFactory : IMinioFactory
{
    private readonly string _endpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _bucketName;
    
    private readonly ILogger _logger;
    
    public MinioFactory(IConfiguration configuration, ILogger logger)
    {
        _endpoint = configuration.GetSection("Minio:Endpoint").Value ?? "minio:9000";
        _accessKey = configuration.GetSection("Minio:AccessKey").Value ?? "minioadmin";
        _secretKey = configuration.GetSection("Minio:SecretKey").Value ?? "minioadmin";
        _bucketName = configuration.GetSection("Minio:BucketName").Value ?? "documents";
        
        _logger = logger;
    }
    
    public INMinioClient Create()
    {
        return new NMinioClient(new MinioClient()
            .WithEndpoint(_endpoint)
            .WithCredentials(_accessKey, _secretKey)
            .Build(), _bucketName, _logger);
    }
}