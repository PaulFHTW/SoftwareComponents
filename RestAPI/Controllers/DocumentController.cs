using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AutoMapper;
using BLL.Documents;
using BLL.Search;
using DAL.Entities;
using MessageQueue.Messages;
using Microsoft.AspNetCore.Mvc;
using NMinio;
using RabbitMQ;
using RestAPI.DTO;
using RestAPI.DVO;
using ILogger = Logging.ILogger;

namespace RestAPI.Controllers;

[ApiController]
[Route("documents")]
public class DocumentController : ControllerBase
{    
    private readonly IDocumentManager _documentManager;
    private readonly IRabbitClient _rabbitClient;
    private readonly ISearchIndex _searchIndex;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly DocumentValidator _validator;
    private readonly INMinioClient _minioClient;

    public DocumentController(IDocumentManager documentManager, IRabbitClient rabbitClient, INMinioClient minioClient, ISearchIndex searchIndex, IMapper mapper, ILogger logger)
    {
        _documentManager = documentManager;
        _rabbitClient = rabbitClient;
        _searchIndex = searchIndex;
        _mapper = mapper;
        _logger = logger;
        _validator = new DocumentValidator();
        _minioClient = minioClient;
    }
    
    /// <summary>
    ///   Uploads a document to the server.
    /// </summary>
    /// <param name="dtoFile"></param>
    /// <returns>Result of the operation</returns>
    [ExcludeFromCodeCoverage]
    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] DocumentDTO dtoFile)
    {
        if (dtoFile.File == null) return BadRequest("File is required.");
        
        _logger.Debug(1);
        var file = _mapper.Map<Document>(dtoFile);
        var validation = await _validator.ValidateAsync(file);
        var pdfFile = dtoFile.File;
        _logger.Debug(2);
        
        if(!validation.IsValid) return BadRequest(validation.Errors);

        _logger.Debug(3);
        
        await _minioClient.Upload(file, pdfFile);
        
        _logger.Debug(4);
        
        var res = await _documentManager.PostAsync(file);
        if(res is not OkObjectResult ok) return res;
        
        _logger.Debug(5);
        
        _rabbitClient.SendMessage(RabbitQueueType.OcrRequestQueue, JsonSerializer.Serialize(new DocumentUploadedMessage( (int) (ok.Value ?? 0), file.Title, file.UploadDate, "Document was uploaded successfully!" )));
        
        _logger.Debug(6);
        
        return res;
    }
    
    /// <summary>
    ///  Gets all documents from the server.
    /// </summary>
    /// <returns>Result of the operation</returns>
    [HttpGet("")]
    public async Task<IEnumerable<Document>> GetAll()
    {
        return await _documentManager.GetAsync();
    }

    /// <summary>
    ///  Deletes a document with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Result of the operation</returns>
    [ExcludeFromCodeCoverage]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int id)
    {
        var document = await _documentManager.GetAsyncById(id);
        if (document == null) return NotFound();
        
        await _minioClient.Delete(document);
        await _searchIndex.RemoveDocumentAsync(document);
        return await _documentManager.DeleteAsync(id);
    }
    
    /// <summary>
    ///  Updates a document with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateDto"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Update([FromQuery] int id, [FromBody] DocumentUpdateDTO updateDto)
    {
        var document = await _documentManager.GetAsyncById(id);
        if (document == null) return NotFound();
        
        document.Title = updateDto.Title!;
        
        await _searchIndex.UpdateDocumentAsync(document);
        return await _documentManager.PutAsync(id, document);
    }
    
    /// <summary>
    ///  Searches for documents with the given query.
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    [ExcludeFromCodeCoverage]
    [HttpGet("search")]
    public async Task<IEnumerable<Document>> Search([FromQuery] string q)
    {
        var documents = await _searchIndex.SearchDocumentAsync(q);
        _logger.Debug("SEARCHING FOR " + q);
        _logger.Debug(documents);
        return documents;
    }
    
    /// <summary>
    ///  Downloads a document with the given id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] int id)
    {
        var document = await _documentManager.GetAsyncById(id);
        if(document == null) return NotFound();

        var stream = await _minioClient.Download(document);
        if(stream == null) return NotFound();

        return File(stream, "application/pdf", document.Title);
    }
}