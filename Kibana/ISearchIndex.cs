using DAL.Entities;

namespace ElasticSearch;

public interface ISearchIndex
{
    void AddDocumentAsync(Document doc);
    IEnumerable<Document> SearchDocumentAsync(string searchTerm);
}
