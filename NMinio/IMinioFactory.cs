using Minio;

namespace NMinio;

public interface IMinioFactory
{
    public IMinioClient Create();
}