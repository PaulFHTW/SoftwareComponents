using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using ILogger = Logging.ILogger;

namespace BLL.Documents;

public class DocumentManager(IDocumentRepository repository, ILogger logger) : ControllerBase, IDocumentManager
{
    public async Task<IEnumerable<Document>> GetAsync()
    {
        logger.Debug("Getting all documents...");
        return await repository.GetAllAsync();
    }

    public async Task<Document?> GetAsyncById(int id)
    {
        logger.Debug($"Getting document with id {id}...");
        return await repository.GetByIdAsync(id);
    }

    public async Task<IActionResult> PostAsync(Document item)
    {
        logger.Debug("Adding new document...");
        var id = await repository.AddAsync(item);
        return Ok(id);
    }

    public async Task<IActionResult> PutAsync(int id, Document item)
    {
        logger.Debug($"Updating document with id {id}...");
        var existingItem = await repository.GetByIdAsync(id);
        if (existingItem == null) return NotFound();

        existingItem.Title = item.Title;
        existingItem.UploadDate = item.UploadDate;
        await repository.UpdateAsync(existingItem);
        return Ok();
    }

    public async Task<IActionResult> DeleteAsync(int id)
    {
        logger.Debug($"Deleting document with id {id}...");
        var item = await repository.GetByIdAsync(id);
        if (item == null) return NotFound();

        await repository.DeleteAsync(id);
        return Ok();
    }
}