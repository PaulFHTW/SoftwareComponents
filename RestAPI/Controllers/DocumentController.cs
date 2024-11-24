using System.Diagnostics;
using AutoMapper;
using DAL.Controllers;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using RestAPI.DTO;
using RestAPI.DVO;
using RestAPI.Queue;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace RestAPI.Controllers;

[ApiController]
[Route("documents")]
public class DocumentController : ControllerBase
{    
    private readonly IDocumentController _documentController;
    private readonly IRabbitSender _rabbitSender;
    private readonly IMapper _mapper;
    private readonly DocumentValidator _validator;
    private readonly IMinioClient _minioClient;
    private readonly string BucketName = "test";

    public DocumentController(IDocumentController documentController, IRabbitSender rabbitSender, IMapper mapper)
    {
        _documentController = documentController;
        _rabbitSender = rabbitSender;
        _mapper = mapper;
        _validator = new DocumentValidator();
        
        _minioClient = new MinioClient()
                .WithEndpoint("minio:9000")
                .WithCredentials("minioadmin", "minioadmin")
                .Build();
    }
    
    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] DocumentDTO dtoFile)
    {
        var file = _mapper.Map<Document>(dtoFile);
        var validation = await _validator.ValidateAsync(file);
        
        var pdfFile = dtoFile.File!;
        
        if(!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        var fileName = Guid.NewGuid() + Path.GetExtension(pdfFile.FileName);
        
        await using (var stream = pdfFile.OpenReadStream())
        {
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(BucketName);
                bool found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(BucketName);
                    await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                // Upload a file to bucket.
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(BucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(pdfFile.Length)
                    .WithContentType("application/pdf");
                await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                Console.WriteLine("Successfully uploaded " + fileName );
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }      
        }

        _rabbitSender.SendMessage("document was uploaded");

        return await _documentController.PostAsync(file);
    }
    
    [HttpGet]
    public async Task<IEnumerable<Document>> GetAll()
    {
        return await _documentController.GetAsync();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        return await _documentController.DeleteAsync(id);
    }
}