using NUnit.Framework;
using Moq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using RestAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http.Json;
using DAL.Entities;
using AutoMapper;
using System.Linq;
using System.Threading;

namespace RestAPI.Tests.Controllers
{
    [TestFixture]
    public class DefaultControllerTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IMapper> _mapperMock;
        private HttpClient _httpClient;
        private HttpMessageHandler _httpMessageHandler;

        [SetUp]
        public void SetUp()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _mapperMock = new Mock<IMapper>();

            // Mock HttpMessageHandler to return a custom HttpResponseMessage
            _httpMessageHandler = new MockHttpMessageHandler();

            _httpClient = new HttpClient(_httpMessageHandler);
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        }

        [Test]
        public async Task GetHomePage_ReturnsOkResult_WithDocuments()
        {
            // Arrange
            var documents = new List<Document>
            {
                new Document { Id = 1, Title = "Test Document 1" },
                new Document { Id = 2, Title = "Test Document 2" }
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(documents)
            };

            // Set up the message handler to return the response message
            ((MockHttpMessageHandler)_httpMessageHandler).SetResponseMessage(responseMessage);

            var controller = new DefaultController(_httpClientFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await controller.GetHomePage();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var returnedDocuments = okResult.Value as IEnumerable<Document>;
            Assert.NotNull(returnedDocuments);
            Assert.AreEqual(2, returnedDocuments.Count());
        }

        [Test]
        public async Task GetHomePage_ReturnsError_WhenServiceFails()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Server error")
            };

            // Set up the message handler to return the response message
            ((MockHttpMessageHandler)_httpMessageHandler).SetResponseMessage(responseMessage);

            var controller = new DefaultController(_httpClientFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await controller.GetHomePage();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
            Assert.AreEqual("Error retrieving items from DAL", objectResult.Value);
        }
    }

    // Custom MockHttpMessageHandler for simulating HttpClient responses
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _responseMessage;

        public void SetResponseMessage(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseMessage);
        }
    }
}
