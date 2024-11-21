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

namespace RestAPI.Controllers;

[ApiController]
[Route("documents")]
public class DocumentController : ControllerBase
{    
    private readonly IDocumentController _documentController;
    private readonly IMapper _mapper;
    private readonly DocumentValidator _validator;
    private readonly IRabbitSender? _rabbitSender;
    private readonly IMinioClient _minioClient = new MinioClient();

    public DocumentController(IDocumentController documentController, IMapper mapper)
    {
        _documentController = documentController;
        _mapper = mapper;
        
        _validator = new DocumentValidator();
    }
    
    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] DocumentDTO uploadedFile)
    {
        var file = _mapper.Map<Document>(uploadedFile);
        var validation = await _validator.ValidateAsync(file);
        
        if(!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }
    
        _minioClient.WithEndpoint("minio").Build();

        _rabbitSender?.SendMessage("document was uploaded");

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