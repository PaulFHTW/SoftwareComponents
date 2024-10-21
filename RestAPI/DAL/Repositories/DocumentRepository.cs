using DAL.Data;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Repositories
{
    public class DocumentRepository(DocumentContext context) : IDocumentRepository
    {
        public async Task<IEnumerable<Document>> GetAllAsync()
        {
            return await context.TodoItems!.ToListAsync();
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return (await context.TodoItems!.FindAsync(id))!;
        }

        public async Task AddAsync(Document item)
        {
            await context.TodoItems!.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Document item)
        {
            context.TodoItems!.Update(item);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await context.TodoItems!.FindAsync(id);
            if (item != null)
            {
                context.TodoItems.Remove(item);
                await context.SaveChangesAsync();
            }
        }
    }
}