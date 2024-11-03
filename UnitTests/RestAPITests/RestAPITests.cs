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
using DAL.Repositories;
using AutoMapper;
using System.Linq;
using System.Threading;
using DAL.Controllers;
using RestAPI.DTO;
using FluentValidation.Results;
using RestAPI.DVO;
using RichardSzalay.MockHttp;


namespace RestAPI.Tests{

    [TestFixture]
    public class RestAPIControllerTests : ControllerBase
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<RestAPI.Utility.ILogger> _logMock;
        private Mock<IDocumentRepository> _documentControllerMock;
        private HttpClient _httpClient;
        private HttpMessageHandler _httpMessageHandler;
        private DefaultController _defaultController;
        private DAL.Controllers.DocumentController _documentController;
        private Mock<DocumentValidator> _mockValidator;

        [SetUp]
        public void Setup(){
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _mapperMock = new Mock<IMapper>();
            _documentControllerMock = new Mock<IDocumentRepository>();
            _mockValidator = new Mock<DocumentValidator>();
            _logMock = new Mock<RestAPI.Utility.ILogger>();

            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler);
            _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            _defaultController = new DefaultController(_httpClientFactoryMock.Object, _mapperMock.Object);
            _documentController = new DAL.Controllers.DocumentController(_documentControllerMock.Object, _logMock.Object);
        }

        [Test]
        public async int Homepage_returns_200(){
            
        }

        [Test]
        public void func(){

        }
    }
}