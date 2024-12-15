using DAL.Data;
using DAL.Entities;
using Logging;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class DocumentRepository(DocumentContext context, ILogger logger) : IDocumentRepository
{
    public async Task<IEnumerable<Document>> GetAllAsync()
    {
        if (!CheckIfDocumentsSetIsInitialized()) return new List<Document>();
        return await context.Documents!.ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        if (!CheckIfDocumentsSetIsInitialized()) return null;
        return await context.Documents!.FindAsync(id);
    }

    public async Task<int> AddAsync(Document item)
    {
        if (!CheckIfDocumentsSetIsInitialized()) return -1;
        
        var entry = await context.Documents!.AddAsync(item);
        await context.SaveChangesAsync();
            
        return entry.Entity.Id;
    }

    public async Task UpdateAsync(Document item)
    {
        if (!CheckIfDocumentsSetIsInitialized()) return;
        
        context.Documents!.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        if (!CheckIfDocumentsSetIsInitialized()) return;
        
        var item = await context.Documents!.FindAsync(id);
        if (item != null)
        {
            context.Documents.Remove(item);
            await context.SaveChangesAsync();
        }
    }
    
    private bool CheckIfDocumentsSetIsInitialized()
    {
        if (context.Documents != null) return true;
        
        logger.Error("Documents set is not initialized.");
        return false;

    }
}