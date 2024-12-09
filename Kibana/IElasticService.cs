using System;
using Elastic.Clients.Elasticsearch.Ingest;

namespace Elastic;

public interface IElasticService{
    Task CreateIndexIfNotExistsAsync(string indexName);

    Task<bool> AddOrUpdate(Document document);

    Task<bool> AddOrUpdateBulk(IEnumerable<Document> documents, string indexName);

    Task<Document> Get(string id);

    Task<List<Document>> GetAll();

    Task<bool> Remove(string id);

    Task<bool> RemoveAll();
}