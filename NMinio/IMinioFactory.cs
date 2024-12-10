using Minio;

namespace NMinio;

public interface IMinioFactory
{
    public INMinioClient Create();
}