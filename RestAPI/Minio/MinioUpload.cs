using System;
using Minio;
using Minio.Exceptions;
using Minio.DataModel;
using Minio.Credentials;
using Minio.DataModel.Args;
using System.Threading.Tasks;

namespace FileUploader
{
    class FileUpload
    {
        public void Upload()
        {
            var endpoint  = "localhost:9000";
            var accessKey = "minioadmin";
            var secretKey = "minioadmin";
            try
            {
                var minio = new MinioClient()
                                    .WithEndpoint(endpoint)
                                    .WithCredentials(accessKey, secretKey)
                                    .Build();
                FileUpload.Run(minio).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        // File uploader task.
        public async static Task Run(IMinioClient minio)
        {
            var bucketName = "test";
            var objectName = "HelloWorld.pdf";
            var filePath = "/home/nero/FH-Folder/WS2024/Software_Komponenten/SoftwareComponents/OcrWorker/docs/HelloWorld.pdf";
            var contentType = "application/pdf";

            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                // Upload a file to bucket.
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithFileName(filePath)
                    .WithContentType(contentType);
                await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                Console.WriteLine("Successfully uploaded " + objectName );
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }
    }
}