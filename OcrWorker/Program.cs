// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using NPaperless.OCRLibrary;
using MessageQueue;
using MessageQueue.Messages;
using System.Threading.Tasks.Dataflow;
using DAL.Entities;
using ElasticSearch;
using Elastic.Clients.Elasticsearch;
using NMinio;

var builder = WebApplication.CreateBuilder(args);

var minioClient = new MinioFactory(builder.Configuration).Create();

var rabbitConsumer = new RabbitConsumer();
await rabbitConsumer.RegisterConsumer(Consumer);

Console.WriteLine("OCR consumer registered.");

while (true);
return;

string Consumer(string message)
{
    try
    {
        Console.WriteLine($"Message Received: {message}");
        var minioPath = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.MinioPath;
        var documentTitle = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.DocumentTitle;
        var documentId = JsonSerializer.Deserialize<DocumentUploadedMessage>(message)!.DocumentId;
        Console.WriteLine($"Performing OCR for document {minioPath}");
        PerformOcr(minioPath, documentId, documentTitle);
    }
    catch (Exception _)
    {
        // ignored
    }

    return message;
}

async void PerformOcr(string filePath, int id, string title)
{
    const string bucketName = "test";
    
    try
    {
        // Make a bucket on the server, if not already present.
        var beArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        var found = await minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs()
                .WithBucket(bucketName);
            await minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
        }
        
        var statObjectArgs = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(filePath);
        
        var stat = await minioClient.StatObjectAsync(statObjectArgs).ConfigureAwait(false);
        if(stat == null)
        {
            Console.WriteLine("File not found");
            return;
        }
        
        var getObjArgs = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(filePath)
            .WithCallbackStream(stream =>
            {
                Console.WriteLine("Received stream...");
                try
                {
                    var ocrClient = new OcrClient(new OcrOptions());
                    var ocrContentText = ocrClient.OcrPdf(stream);
                    Console.WriteLine(ocrContentText);
                    //Add document to kibana
                    Document document = new Document(id, title, ocrContentText, DateTime.Now, filePath);
                    var ElasticSearchIndex = new SearchIndex();
                    ElasticSearchIndex.AddDocumentAsync(document);
                }
                catch (IOException e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                }   
            });
        
        await minioClient.GetObjectAsync(getObjArgs).ConfigureAwait(false);
        Console.WriteLine("Successfully downloaded " + filePath);
    }
    catch (MinioException e)
    {
        Console.WriteLine("File Upload Error: {0}", e.Message);
    } 
    
     
}