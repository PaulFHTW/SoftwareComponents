using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Entities;
using DAL.Repositories;
using Logging;
using BLL.Documents;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace BLL.Tests.Documents
{
    [TestFixture]
    public class DocumentManagerTests
    {
        private Mock<IDocumentRepository> _repositoryMock;
        private Mock<ILogger> _loggerMock;
        private DocumentManager _documentManager;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IDocumentRepository>();
            _loggerMock = new Mock<ILogger>();
            _documentManager = new DocumentManager(_repositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task GetAsync_ReturnsAllDocuments()
        {
            // Arrange
            var documents = new List<Document>
            {
                new Document { Id = 1, Title = "Doc1" },
                new Document { Id = 2, Title = "Doc2" }
            };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(documents);

            // Act
            var result = await _documentManager.GetAsync();

            // Assert
            Assert.AreEqual(2, result.Count());
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _loggerMock.Verify(l => l.Debug("Getting all documents..."), Times.Once);
        }

        [Test]
        public async Task GetAsyncById_ReturnsDocument_WhenDocumentExists()
        {
            // Arrange
            var document = new Document { Id = 1, Title = "Test Document" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(document);

            // Act
            var result = await _documentManager.GetAsyncById(1);

            // Assert
            Assert.AreEqual(document, result);
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Test]
        public async Task GetAsyncById_ReturnsNull_WhenDocumentDoesNotExist()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Document)null);

            // Act
            var result = await _documentManager.GetAsyncById(1);

            // Assert
            Assert.IsNull(result);
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Test]
        public async Task PostAsync_ReturnsBadRequest_WhenTitleIsEmpty()
        {
            // Arrange
            var document = new Document { Title = "" };

            // Act
            var result = await _documentManager.PostAsync(document) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        public async Task PostAsync_ReturnsOk_WithDocumentId()
        {
            // Arrange
            var document = new Document { Title = "Valid Title" };
            _repositoryMock.Setup(r => r.AddAsync(document)).ReturnsAsync(1);

            // Act
            var result = await _documentManager.PostAsync(document) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(1, result.Value);
            _repositoryMock.Verify(r => r.AddAsync(document), Times.Once);
        }

        [Test]
        public async Task PutAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            var document = new Document { Title = "Updated Title" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Document)null);

            // Act
            var result = await _documentManager.PutAsync(1, document) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Test]
        public async Task PutAsync_UpdatesDocument_WhenDocumentExists()
        {
            // Arrange
            var existingDocument = new Document { Id = 1, Title = "Old Title" };
            var updatedDocument = new Document { Title = "New Title", UploadDate = DateTime.UtcNow };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDocument);

            // Act
            var result = await _documentManager.PutAsync(1, updatedDocument) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("New Title", existingDocument.Title);
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
            _repositoryMock.Verify(r => r.UpdateAsync(existingDocument), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Document)null);

            // Act
            var result = await _documentManager.DeleteAsync(1) as NotFoundResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_DeletesDocument_WhenDocumentExists()
        {
            // Arrange
            var document = new Document { Id = 1, Title = "To Be Deleted" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(document);

            // Act
            var result = await _documentManager.DeleteAsync(1) as OkResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
            _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}
