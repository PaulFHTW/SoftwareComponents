using DAL.Data;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Repositories
{
    public class DocumentRepository(DocumentContext context) : IDocumentRepository
    {
        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await context.Documents!.ToListAsync();
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return (await context.Documents!.FindAsync(id))!;
        }

        public async Task<int> AddAsync(Document item)
        {
            var entry = await context.Documents!.AddAsync(item);
            await context.SaveChangesAsync();
            
            return entry.Entity.Id;
        }

        public async Task UpdateAsync(Document item)
        {
            context.Documents!.Update(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await context.Documents!.FindAsync(id);
            if (item != null)
            {
                context.Documents.Remove(item);
                await context.SaveChangesAsync();
            }
        }
    }
}