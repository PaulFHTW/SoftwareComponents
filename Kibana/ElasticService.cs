using System;
using System.Collections.Specialized;
using System.Reflection.Metadata.Ecma335;
using Elastic;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Ingest;
using Elastic.Clients.Elasticsearch.Mapping;

namespace Elastic;
public class ElasticService : IElasticService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ElasticSettings _elasticSettings;

    public ElasticService(IOptions<ElasticSettings> options)
    {
        
    }
    public Task<bool> AddOrUpdate(Document document)
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddOrUpdateBulk(IEnumerable<Document> documents, string indexName)
    {
        throw new NotImplementedException();
    }

    public Task CreateIndexIfNotExistsAsync(string indexName)
    {
        throw new NotImplementedException();
    }

    public Task<Document> Get(string id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Document>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Remove(string id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAll()
    {
        throw new NotImplementedException();
    }
}