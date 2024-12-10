using DAL.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Logging;
using Microsoft.Extensions.Configuration;
using ILogger = Logging.ILogger;

namespace ElasticSearch;
public class SearchIndex : ISearchIndex
{
    private readonly Uri _uri;
    private readonly ILogger _logger;

    public SearchIndex()
    {
        _uri = new Uri("http://elastic_search:9200/");
        _logger = new Logger();
    }
    public SearchIndex(IConfiguration configuration, ILogger logger)
    {
        this._uri = new Uri(configuration.GetConnectionString("ElasticSearch") ?? "http://elastic_search:9200/");
        this._logger = logger;
    }

    public async Task AddDocumentAsync(Document document)
    {
        var elasticClient = new ElasticsearchClient(_uri);

        if (!(await elasticClient.Indices.ExistsAsync("documents")).Exists)
            await elasticClient.Indices.CreateAsync("documents");

        var indexResponse = await elasticClient.IndexAsync(document, (IndexName)"documents");
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
        var elasticClient = new ElasticsearchClient(_uri);
        
        var searchResponse = await elasticClient.SearchAsync<Document>(s => s
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
}


