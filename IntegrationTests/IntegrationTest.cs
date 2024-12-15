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
using RabbitMQ;
using AutoMapper;
using RestAPI.Mappings;
using RestAPI.DVO;
using NMinio;
using System.Net.Http;

namespace BLL.Search.Tests
{
    [TestFixture]
    public class SearchIndexTests
    {
        private Uri _uri;
        private ElasticsearchClient _elasticClient;
        private IRabbitClient _rabbitClient;
        private ISearchIndex _searchIndex;
        private IMapper _mapper;
        private ILogger _logger;
        private DocumentValidator _validator;
        private INMinioClient _minioClient;
        private readonly string _indexName = "documents";

        [SetUp]
        public async Task SetUp()
        {
            _logger = new Logger();

            //Create elasticSearchClient
            _searchIndex = new SearchIndex("http://localhost:9200", _logger);
            _uri = new Uri("http://localhost:9200");

            var elasticSettings = new ElasticsearchClientSettings(_uri).DefaultMappingFor<Document>(d => d.IndexName("documents"));
            _elasticClient = new ElasticsearchClient(elasticSettings);

            if (!(await _elasticClient.Indices.ExistsAsync("documents")).Exists)
                await _elasticClient.Indices.CreateAsync("documents");
        } 

        [Test]
        public void UploadDocument()
        {
            byte[] pdfFile = File.ReadAllBytes("../Paul_Bogza_Proposal.pdf");

            WebRequest request = WebRequest.Create("http://localhost/");
            request.Method = "POST";
            request.ContentLength = pdfFile.Length;
            request.ContentType = "application/pdf";

            Stream stream = request.GetRequestStream();
            stream.Write(pdfFile, 0, pdfFile.Length);
            stream.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            Console.WriteLine(reader.ReadToEnd());
            reader.Close();

            Assert.That(response == 200);
        }
    }
}