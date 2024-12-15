using DAL.Entities;

namespace BLL.Search;

public interface ISearchIndex
{
    Task<bool> AddDocumentAsync(Document doc);
    Task<IEnumerable<Document>> SearchDocumentAsync(string searchTerm);
    Task<bool> RemoveDocumentAsync(Document doc);
    Task<bool> UpdateDocumentAsync(Document doc);
}
