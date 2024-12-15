using AutoMapper;
using BLL.Documents;
using DAL.Repositories;
using Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using RestAPI.DVO;
using RichardSzalay.MockHttp;

namespace RestAPITests;

[TestFixture]
public class RestAPIControllerTests : ControllerBase
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ILogger> _logMock;
    private Mock<IDocumentRepository> _documentControllerMock;
    private HttpClient _httpClient;
    private HttpMessageHandler _httpMessageHandler;
    private DocumentManager _documentManager;
    private Mock<DocumentValidator> _mockValidator;

    [SetUp]
    public void Setup(){
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _mapperMock = new Mock<IMapper>();
        _documentControllerMock = new Mock<IDocumentRepository>();
        _mockValidator = new Mock<DocumentValidator>();
        _logMock = new Mock<ILogger>();

        _httpMessageHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler);
        _httpClientFactoryMock.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _documentManager = new DocumentManager(_documentControllerMock.Object, _logMock.Object);
    }

    [Test]
    public void Homepage_returns_200(){

    }

    [Test]
    public void func(){

    }
}