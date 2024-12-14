using DAL.Entities;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using ILogger = Logging.ILogger;

namespace NMinio;

public class NMinioClient : INMinioClient
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly ILogger _logger;
    
    public NMinioClient(IMinioClient minioClient, string bucketName, ILogger logger)
    {
        _minioClient = minioClient;
        _bucketName = bucketName;
        _logger = logger;

        // Create bucket if it doesn't exist
        InitializeBuckets();
    }

    private async void InitializeBuckets()
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);
            var found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                _logger.Info("Bucket not found. Creating bucket...");
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }
        }
        catch (Exception e)
        {
            _logger.Error("Error creating bucket: " + e.Message);
        }
    }
    
    public async Task Upload(Document file, IFormFile pdfFile)
    {
        await using (var stream = pdfFile.OpenReadStream())
        {
            try
            {
                // Upload a file to bucket.
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(file.Id + ".pdf")
                    .WithStreamData(stream)
                    .WithObjectSize(pdfFile.Length)
                    .WithContentType("application/pdf");
                await _minioClient.PutObjectAsync(putObjectArgs);
                _logger.Info("Successfully uploaded " + file.Id);
            }
            catch (MinioException e)
            {
                _logger.Error($"File Upload Error: {e.Message}");
            }      
        }
    }

    public async Task<Stream?> Download(string filename)
    {
        Stream s = new MemoryStream();
        try
        {
            _logger.Info("Checking for existence of " + filename + ".pdf");
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filename + ".pdf");
        
            var stat = await _minioClient.StatObjectAsync(statObjectArgs);
            if(stat == null)
            {
                _logger.Error("File not found");
                return null;
            }

            _logger.Info("Downloading " + filename + ".pdf");
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filename + ".pdf")
                .WithCallbackStream(stream => { stream.CopyTo(s); });
            await _minioClient.GetObjectAsync(getObjectArgs);
            _logger.Info("Successfully downloaded " + filename);
        }
        catch (MinioException e)
        {
            _logger.Error($"File Download Error: {e.Message}");
            return null;
        }

        s.Position = 0;
        return s;
    }
    
    public async Task<Stream?> Download(Document file)
    {
        return await Download(file.Id.ToString());
    }

    public async Task Delete(Document file)
    {   
        try
        {
            _logger.Info("Checking for existence of " + file.Id + ".pdf");
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(file.Id + ".pdf");
        
            var stat = await _minioClient.StatObjectAsync(statObjectArgs);
            if(stat == null)
            {
                _logger.Error("File not found");
                return;
            }

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(file.Id + ".pdf");
            await _minioClient.RemoveObjectAsync(removeObjectArgs);
            _logger.Info("Successfully deleted " + file.Id);
        }
        catch (MinioException e)
        {
            _logger.Error($"File Delete Error: {e.Message}");
        }
    }
}