using Microsoft.AspNetCore.Mvc;
using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Repositories;
using Entities_File = DAL.Entities.File;
using File = DAL.Entities.File;

namespace DAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(IFileRepository repository) : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<Entities_File>> GetAsync()
        {
            return await repository.GetAllAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(Entities_File item)
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                return BadRequest(new { message = "Task name cannot be empty." });
            }
            
            await repository.AddAsync(item);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, Entities_File item)
        {
            var existingItem = await repository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Title = item.Title;
            existingItem.UploadDate = item.UploadDate;
            existingItem.Path = item.Path;
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