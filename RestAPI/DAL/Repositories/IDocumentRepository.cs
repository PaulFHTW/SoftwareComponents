using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(int id);
        Task AddAsync(Document item);
        Task UpdateAsync(Document item);
        Task DeleteAsync(int id);
    }
}