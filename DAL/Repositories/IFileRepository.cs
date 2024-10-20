using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Entities;
using Entities_File = DAL.Entities.File;
using File = DAL.Entities.File;

namespace DAL.Repositories
{
    public interface IFileRepository
    {
        Task<IEnumerable<Entities_File>> GetAllAsync();
        Task<Entities_File> GetByIdAsync(int id);
        Task AddAsync(Entities_File item);
        Task UpdateAsync(Entities_File item);
        Task DeleteAsync(int id);
    }
}