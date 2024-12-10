using DAL.Entities;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace NMinio;

public class NMinioClient : INMinioClient
{
    private readonly IMinioClient _minioClient;
    private readonly string BucketName = "test";
    
    public NMinioClient(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }
    
    public async Task Upload(Document file, IFormFile pdfFile)
    {
        await using (var stream = pdfFile.OpenReadStream())
        {
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(BucketName);
                bool found = await _minioClient.BucketExistsAsync(beArgs);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(BucketName);
                    await _minioClient.MakeBucketAsync(mbArgs);
                }
                // Upload a file to bucket.
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(file.Id + ".pdf")
                    .WithStreamData(stream)
                    .WithObjectSize(pdfFile.Length)
                    .WithContentType("application/pdf");
                await _minioClient.PutObjectAsync(putObjectArgs);
                Console.WriteLine("Successfully uploaded " + file.Id);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }      
        }
    }

    public async Task<Stream?> Download(string filename)
    {
        Stream s = new MemoryStream();
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(BucketName);
            var found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(BucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }

            Console.WriteLine("Checking for existence of " + filename + ".pdf");
            
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(BucketName)
                .WithObject(filename + ".pdf");
        
            var stat = await _minioClient.StatObjectAsync(statObjectArgs);
            if(stat == null)
            {
                Console.WriteLine("File not found");
                return null;
            }

            Console.WriteLine("Downloading " + filename + ".pdf");
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(filename + ".pdf")
                .WithCallbackStream((stream) => { stream.CopyTo(s); });
            await _minioClient.GetObjectAsync(getObjectArgs);
            Console.WriteLine("Successfully downloaded " + filename);
        }
        catch (MinioException e)
        {
            Console.WriteLine("File Download Error: {0}", e.Message);
            return null;
        }

        s.Position = 0;
        return s;
    }

    public async Task<Stream?> Download(Document file)
    {
        return await Download(file.Id.ToString());
    }
}