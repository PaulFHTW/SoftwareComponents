using DAL.Entities;

namespace ElasticSearch;

public interface ISearchIndex
{
    Task AddDocumentAsync(Document doc);
    public Task<IEnumerable<Document>> SearchDocumentAsync(string searchTerm);
}
