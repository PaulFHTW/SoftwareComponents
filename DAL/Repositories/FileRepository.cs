using DAL.Data;
using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using Entities_File = DAL.Entities.File;
using File = DAL.Entities.File;

namespace DAL.Repositories
{
    public class FileRepository(FileContext context) : IFileRepository
    {
        public async Task<IEnumerable<Entities_File>> GetAllAsync()
        {
            return await context.TodoItems!.ToListAsync();
        }

        public async Task<Entities_File> GetByIdAsync(int id)
        {
            return (await context.TodoItems!.FindAsync(id))!;
        }

        public async Task AddAsync(Entities_File item)
        {
            await context.TodoItems!.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Entities_File item)
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