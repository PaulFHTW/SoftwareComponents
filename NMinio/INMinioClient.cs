using DAL.Entities;

namespace NMinio;

public interface INMinioClient
{
    public Task Upload(Document file, IFormFile pdfFile);
    public Task<Stream?> Download(Document file);
    public Task<Stream?> Download(string fileName);
    public Task Delete(Document file);
}