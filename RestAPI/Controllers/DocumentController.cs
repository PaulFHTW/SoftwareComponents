using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using DAL.Controllers;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using ElasticSearch;
using RestAPI.DTO;
using RestAPI.DVO;
using MessageQueue;
using MessageQueue.Messages;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using ILogger = Logging.ILogger;

namespace RestAPI.Controllers;

[ApiController]
[Route("documents")]
public class DocumentController : ControllerBase
{    
    private readonly IDocumentManager _documentManager;
    private readonly IRabbitSender _rabbitSender;
    private readonly ISearchIndex _searchIndex;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly DocumentValidator _validator;
    private readonly IMinioClient _minioClient;
    private readonly string BucketName = "test";

    public DocumentController(IDocumentManager documentManager, IRabbitSender rabbitSender, IMinioClient minioClient, ISearchIndex searchIndex, IMapper mapper, ILogger logger)
    {
        _documentManager = documentManager;
        _rabbitSender = rabbitSender;
        _searchIndex = searchIndex;
        _mapper = mapper;
        _logger = logger;
        _validator = new DocumentValidator();
        _minioClient = minioClient;
    }
    
    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] DocumentDTO dtoFile)
    {
        _logger.Debug("UPLOADING FILE.....");
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

        file.Path = fileName;
        
        var res = await _documentManager.PostAsync(file);
        if(res is not OkObjectResult ok) return res;
        
        _rabbitSender.SendMessage(JsonSerializer.Serialize(new DocumentUploadedMessage(fileName, (int) (ok.Value ?? 0), file.Title, "Document was uploaded successfully!" )));
        return res;
    }
    
    [HttpGet("")]
    public async Task<IEnumerable<Document>> GetAll()
    {
        return await _documentManager.GetAsync();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        return await _documentManager.DeleteAsync(id);
    }
    
    [HttpPut]
    public async Task<IActionResult> Update([FromQuery] int id, [FromBody] DocumentUpdateDTO updateDto)
    {
        _logger.Debug("UPDATE TRIGGERED FOR ID " + id);
        var document = await _documentManager.GetAsyncById(id);
        document.Title = updateDto.Title!;
        
        return await _documentManager.PutAsync(id, document);
    }
    
    [HttpGet("search")]
    public async Task<IEnumerable<Document>> Search([FromQuery] string q)
    {
        var documents = await _searchIndex.SearchDocumentAsync(q);
        _logger.Debug("SEARCHING FOR " + q);
        _logger.Debug(documents);
        return documents;
    }
}