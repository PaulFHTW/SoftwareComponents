using DAL.Entities;

namespace ElasticSearch;

public interface ISearchIndex
{
    Task AddDocumentAsync(Document doc);
    Task<IEnumerable<Document>> SearchDocumentAsync(string searchTerm);
    Task RemoveDocumentAsync(Document doc);
    Task UpdateDocumentAsync(Document doc);
}
