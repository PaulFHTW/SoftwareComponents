using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using DAL.Repositories;
using File = DAL.Entities.File;

namespace RestAPI.Controllers;

[ApiController]
[Route("uploads")]
public class UploadController : ControllerBase
{    
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;

    public UploadController(IHttpClientFactory httpClientFactory, IMapper mapper)
    {
        _httpClientFactory = httpClientFactory;
        _mapper = mapper;
    }
    
    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] string fileName)
    {
        var file = new File()
        {
            Title = fileName,
            UploadDate = DateTime.Now,
            Path = fileName
        };   
        
        var client = _httpClientFactory.CreateClient("DAL");
        var response = await client.PostAsJsonAsync("/api/dal", file);
        
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }

        return StatusCode((int)response.StatusCode, "Error creating item in DAL");
    }
}