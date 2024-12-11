using DAL.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Logging;
using Microsoft.Extensions.Configuration;
using ILogger = Logging.ILogger;

namespace ElasticSearch;
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

    public async Task AddDocumentAsync(Document document)
    {
        var indexResponse = await _elasticClient.IndexAsync(document);
        if (!indexResponse.IsSuccess())
        {
            // Handle errors
            _logger.Error($"Failed to index document: {indexResponse.DebugInformation}\n{indexResponse.ElasticsearchServerError}");
            throw new Exception($"Failed to index document: {indexResponse.DebugInformation}\n{indexResponse.ElasticsearchServerError}");
        }
        
        _logger.Debug($"Document indexed successfully: {document.Id}");
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

    public async Task RemoveDocumentAsync(Document doc)
    {   
        var deleteResponse = await _elasticClient.DeleteAsync<Document>(doc.Id);
        if (!deleteResponse.IsSuccess())
        {
            _logger.Error($"Failed to delete document: {deleteResponse.DebugInformation}\n{deleteResponse.ElasticsearchServerError}");
            throw new Exception($"Failed to delete document: {deleteResponse.DebugInformation}\n{deleteResponse.ElasticsearchServerError}");
        }
        
        _logger.Debug($"Document deleted successfully: {doc.Id}");
    }

    public async Task UpdateDocumentAsync(Document doc)
    {
        var currentDocument = (await _elasticClient.GetAsync<Document>(doc.Id)).Source;
        
        if(currentDocument == null)
        {
            _logger.Error($"Failed to update document: Document with id {doc.Id} not found");
            throw new Exception($"Failed to update document: Document with id {doc.Id} not found");
        }

        currentDocument.UploadDate = doc.UploadDate;
        currentDocument.Title = doc.Title;
        
        await RemoveDocumentAsync(currentDocument);
        await AddDocumentAsync(currentDocument);
        _logger.Debug($"Document updated successfully: {doc.Id}");
    }
}


