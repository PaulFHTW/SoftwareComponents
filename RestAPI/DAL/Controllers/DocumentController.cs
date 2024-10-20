﻿using Microsoft.AspNetCore.Mvc;
using DAL.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DAL.Repositories;

namespace DAL.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class DocumentController(IDocumentRepository repository) : ControllerBase, IDocumentController
    {
        [HttpGet]
        public async Task<IEnumerable<Document>> GetAsync()
        {
            return await repository.GetAllAsync();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(Document item)
        {
            Debug.WriteLine("Hello World 2");
            
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                return BadRequest(new { message = "Task name cannot be empty." });
            }
            
            await repository.AddAsync(item);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, Document item)
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