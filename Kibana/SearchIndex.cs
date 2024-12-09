namespace ElasticSearch;

using System.Diagnostics;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DAL.Entities;

public class ElasticSearchIndex : ISearchIndex
{
    private readonly Uri _uri;
    private readonly ILogger<ElasticSearchIndex> _logger;

    public ElasticSearchIndex(IConfiguration configuration, ILogger<ElasticSearchIndex> logger)
    {
        this._uri = new Uri(configuration.GetConnectionString("ElasticSearch") ?? "http://localhost:9200/");
        this._logger = logger;
    }

    [Obsolete]
    public async void AddDocumentAsync(Document document)
    {
        var elasticClient = new ElasticsearchClient(_uri);

        if (!elasticClient.Indices.Exists("documents").Exists)
            elasticClient.Indices.Create("documents");

        var indexResponse = await elasticClient.IndexAsync(document, (IndexName)"documents");
        if (!indexResponse.IsSuccess())
        {
            // Handle errors
            _logger.LogError($"Failed to index document: {indexResponse.DebugInformation}\n{indexResponse.ElasticsearchServerError}");

            throw new Exception($"Failed to index document: {indexResponse.DebugInformation}\n{indexResponse.ElasticsearchServerError}");
        }


    }

    [Obsolete]
    public IEnumerable<Document> SearchDocumentAsync(string searchTerm)
    {
        var elasticClient = new ElasticsearchClient(_uri);

        var searchResponse = elasticClient.Search<Document>(s => s
            .Index("documents")
            .Query(q => q.QueryString(qs => qs.DefaultField(p => p.Content).Query($"*{searchTerm}*")))
        );

        return searchResponse.Documents;
    }
}


