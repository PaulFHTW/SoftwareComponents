using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using File = DAL.Entities.File;

namespace RestAPI.Controllers;

[ApiController]
[Route("/")]
public class DefaultController : ControllerBase
{    
    
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMapper _mapper;

    public DefaultController(IHttpClientFactory httpClientFactory, IMapper mapper)
    {
        _httpClientFactory = httpClientFactory;
        _mapper = mapper;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetHomePage()
    {
        var client = _httpClientFactory.CreateClient("DAL");
        var response = await client.GetAsync("/api/dal");

        if (response.IsSuccessStatusCode)
        {
            var items = await response.Content.ReadFromJsonAsync<IEnumerable<File>>();
            //var dtoItems = _mapper.Map<IEnumerable<TodoItemDto>>(items);
            return Ok(items);
        }

        return StatusCode((int)response.StatusCode, "Error retrieving items from DAL");
    }
}