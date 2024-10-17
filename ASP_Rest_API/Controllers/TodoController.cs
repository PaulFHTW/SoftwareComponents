using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ASP_Rest_API.DTO;
using AutoMapper;
using TodoDAL.Entities;

namespace ASP_Rest_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public TodoController(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var client = _httpClientFactory.CreateClient("TodoDAL");
            var response = await client.GetAsync("/api/todo"); // Endpunkt des DAL

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<TodoItem>>();
                var dtoItems = _mapper.Map<IEnumerable<TodoItemDto>>(items);
                return Ok(dtoItems);
            }

            return StatusCode((int)response.StatusCode, "Error retrieving Todo items from DAL");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = _httpClientFactory.CreateClient("TodoDAL");
            var response = await client.GetAsync($"/api/todo/{id}");

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<TodoItem>();
                var dtoItem = _mapper.Map<TodoItemDto>(item);
                if (item != null)
                {
                    return Ok(dtoItem);
                }
                return NotFound();
            }

            return StatusCode((int)response.StatusCode, "Error retrieving Todo item from DAL");
        }

        [HttpPost]
        public async Task<IActionResult> Create(TodoItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("TodoDAL");
            var item = _mapper.Map<TodoItem>(itemDto);
            var response = await client.PostAsJsonAsync("/api/todo", item);

            if (response.IsSuccessStatusCode)
            {
                return CreatedAtAction(nameof(GetById), new { id = item.Id }, itemDto);
            }

            return StatusCode((int)response.StatusCode, "Error creating Todo item in DAL");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TodoItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != itemDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var client = _httpClientFactory.CreateClient("TodoDAL");
            var item = _mapper.Map<TodoItem>(itemDto);
            var response = await client.PutAsJsonAsync($"/api/todo/{id}", item);

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error updating Todo item in DAL");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("TodoDAL");
            var response = await client.DeleteAsync($"/api/todo/{id}");

            if (response.IsSuccessStatusCode)
            {
                return NoContent();
            }

            return StatusCode((int)response.StatusCode, "Error deleting Todo item from DAL");
        }
    }
}
