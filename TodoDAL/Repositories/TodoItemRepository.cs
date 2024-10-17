using Microsoft.EntityFrameworkCore;
using TodoDAL.Data;
using TodoDAL.Entities;

namespace TodoDAL.Repositories
{
    public class TodoItemRepository(TodoContext context) : ITodoItemRepository
    {
        public async Task<IEnumerable<TodoItem>> GetAllAsync()
        {
            return await context.TodoItems!.ToListAsync();
        }

        public async Task<TodoItem> GetByIdAsync(int id)
        {
            return (await context.TodoItems!.FindAsync(id))!;
        }

        public async Task AddAsync(TodoItem item)
        {
            await context.TodoItems!.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TodoItem item)
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