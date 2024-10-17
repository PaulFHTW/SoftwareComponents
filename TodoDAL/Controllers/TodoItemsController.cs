using Microsoft.AspNetCore.Mvc;
using TodoDAL.Repositories;
using TodoDAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoDAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController(ITodoItemRepository repository) : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<TodoItem>> GetAsync()
        {
            return await repository.GetAllAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(TodoItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                return BadRequest(new { message = "Task name cannot be empty." });
            }
            await repository.AddAsync(item);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, TodoItem item)
        {
            var existingItem = await repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = item.Name;
            existingItem.IsComplete = item.IsComplete;
            await repository.UpdateAsync(existingItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var item = await repository.GetByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await repository.DeleteAsync(id);
            return NoContent();
        }
    }
}