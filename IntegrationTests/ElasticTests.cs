using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Entities;
using Elastic.Clients.Elasticsearch;
using Logging;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using BLL.Search;

namespace BLL.Search.Tests
{
    [TestFixture]
    public class SearchIndexTests
    {
        public Uri _uri;
        public ILogger _logger;
        private ISearchIndex _searchIndex;
        private ElasticsearchClient _elasticClient;
        private readonly string _indexName = "documents";

        [SetUp]
        public async Task SetUp()
        {
            _searchIndex = new SearchIndex("http://localhost:9200");

            var elasticSettings = new ElasticsearchClientSettings(_uri).DefaultMappingFor<Document>(d => d.IndexName("documents"));
            _elasticClient = new ElasticsearchClient(elasticSettings);

            if (!(await _elasticClient.Indices.ExistsAsync("documents")).Exists)
                await _elasticClient.Indices.CreateAsync("documents");
        } 

        [TearDown]
        public async Task TearDown()
        {
            // Clean up the index after tests
            await _elasticClient.Indices.DeleteAsync(_indexName);
        }

        [Test]
        public async Task AddDocumentAsync_ShouldAddDocumentToElasticsearch()
        {
            // Arrange
            var document = new Document(1, "Test Title", "Test Content", DateTime.Now);

            // Act
            await _searchIndex.AddDocumentAsync(document);

            // Assert
            var searchResponse = await _searchIndex.SearchDocumentAsync("Test Title");

            Assert.IsTrue(searchResponse.Any(d => d.Id == document.Id), "Document should be added to Elasticsearch.");
        }

        [Test]
        public async Task SearchDocumentAsync_ShouldReturnDocuments_WhenSearchTermMatches()
        {
            // Arrange
            var document = new Document(2, "Searchable Title", "Searchable Content", DateTime.UtcNow);
            await _searchIndex.AddDocumentAsync(document);

            // Act
            var searchResult = await _searchIndex.SearchDocumentAsync("Searchable");

            // Assert
            Assert.IsTrue(searchResult.Any(d => d.Id == document.Id), "Document should be found in the search results.");
        }

        [Test]
        public async Task RemoveDocumentAsync_ShouldRemoveDocumentFromElasticsearch()
        {
            // Arrange
            var document = new Document(3, "Remove Title", "Remove Content", DateTime.UtcNow);
            await _searchIndex.AddDocumentAsync(document);

            // Act
            await _searchIndex.RemoveDocumentAsync(document);

            // Assert
            var searchResponse = await _elasticClient.SearchAsync<Document>(s => s
                .Index(_indexName)
                .Query(q => q.Term(t => t.Field(f => f.Id).Value(document.Id))));

            Assert.IsEmpty(searchResponse.Documents, "Document should be removed from Elasticsearch.");
        }

        [Test]
        public async Task UpdateDocumentAsync_ShouldUpdateDocumentInElasticsearch()
        {
            // Arrange
            var document = new Document(4, "Old Title", "Old Content", DateTime.UtcNow);
            await _searchIndex.AddDocumentAsync(document);

            var updatedDocument = new Document(4, "New Title", "New Content", DateTime.UtcNow);

            // Act
            await _searchIndex.UpdateDocumentAsync(updatedDocument);

            // Assert
            var searchResponse = await _elasticClient.SearchAsync<Document>(s => s
                .Index(_indexName)
                .Query(q => q.Term(t => t.Field(f => f.Id).Value(updatedDocument.Id))));

            var updatedDoc = searchResponse.Documents.FirstOrDefault();
            Assert.IsNotNull(updatedDoc, "Updated document should be found in the search results.");
            Assert.AreEqual("New Title", updatedDoc?.Title, "Document title should be updated.");
            Assert.AreEqual("New Content", updatedDoc?.Content, "Document content should be updated.");
        }
    }
}