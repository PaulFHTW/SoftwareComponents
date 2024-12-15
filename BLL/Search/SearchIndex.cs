using DAL.Entities;
using Elastic.Clients.Elasticsearch;
using Logging;
using Microsoft.Extensions.Configuration;
using ILogger = Logging.ILogger;

namespace BLL.Search;
public class SearchIndex : ISearchIndex
{
    private readonly Uri _uri;
    private readonly ILogger _logger;
    private ElasticsearchClient _elasticClient;

    public SearchIndex()
    {
        _uri = new Uri("http://elastic_search:9200/");
        _logger = new Logger();
        Configure().Wait();
    }
    public SearchIndex(IConfiguration configuration, ILogger logger)
    {
        _uri = new Uri(configuration.GetConnectionString("ElasticSearch") ?? "http://elastic_search:9200/");
        _logger = logger;
        Configure().Wait();
    }

    private async Task Configure()
    {
        var elasticSettings = new ElasticsearchClientSettings(_uri).DefaultMappingFor<Document>(d => d.IndexName("documents"));
        _elasticClient = new ElasticsearchClient(elasticSettings);

        if (!(await _elasticClient.Indices.ExistsAsync("documents")).Exists)
            await _elasticClient.Indices.CreateAsync("documents");
    }

    public async Task<bool> AddDocumentAsync(Document document)
    {
        var indexResponse = await _elasticClient.IndexAsync(document);
        if (!indexResponse.IsSuccess())
        {
            _logger.Error($"Failed to index document: {indexResponse.DebugInformation}\n{indexResponse.ElasticsearchServerError}");
            return false;
        }
        
        _logger.Debug($"Document indexed successfully: {document.Id}");
        return true;
    }

    public async Task<IEnumerable<Document>> SearchDocumentAsync(string searchTerm)
    {
        var searchResponse = await _elasticClient.SearchAsync<Document>(s => s
            .Index("documents")
            .Query(q => 
                q.QueryString(qs => 
                    qs.Fields(new [] { "content", "title" })
                        .Query($"*{searchTerm}*")
        )));

        try
        {
            return searchResponse.Documents;
        }
        catch (NullReferenceException)
        {
            return Array.Empty<Document>();
        }
    }

    public async Task<bool> RemoveDocumentAsync(Document doc)
    {   
        var deleteResponse = await _elasticClient.DeleteAsync<Document>(doc.Id);
        if (!deleteResponse.IsSuccess())
        {
            _logger.Error($"Failed to delete document: {deleteResponse.DebugInformation}\n{deleteResponse.ElasticsearchServerError}");
            return false;
        }
        
        _logger.Debug($"Document deleted successfully: {doc.Id}");
        return true;
    }

    public async Task<bool> UpdateDocumentAsync(Document doc)
    {
        var currentDocument = (await _elasticClient.GetAsync<Document>(doc.Id)).Source;
        
        if(currentDocument == null)
        {
            _logger.Error($"Failed to update document: Document with id {doc.Id} not found");
            return false;
        }

        currentDocument.UploadDate = doc.UploadDate;
        currentDocument.Title = doc.Title;

        if (!await RemoveDocumentAsync(currentDocument) || await AddDocumentAsync(currentDocument))
        {
            _logger.Error($"Failed to update document: Document with id {doc.Id} not found");
            return false;
        }
        
        _logger.Debug($"Document updated successfully: {doc.Id}");
        return true;
    }
}


