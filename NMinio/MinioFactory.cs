using Minio;

namespace NMinio;

public class MinioFactory : IMinioFactory
{
    private readonly string _endpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;
    
    public MinioFactory(IConfiguration configuration)
    {
        _endpoint = configuration.GetSection("Minio:Endpoint").Value ?? "minio:9000";
        _accessKey = configuration.GetSection("Minio:AccessKey").Value ?? "minioadmin";
        _secretKey = configuration.GetSection("Minio:SecretKey").Value ?? "minioadmin";
    }

    public MinioFactory()
    {
        _endpoint = "minio:9000";
        _accessKey = "minioadmin";
        _secretKey = "minioadmin";
    }
    
    public INMinioClient Create()
    {
        return new NMinioClient(new MinioClient()
            .WithEndpoint(_endpoint)
            .WithCredentials(_accessKey, _secretKey)
            .Build());
    }
}