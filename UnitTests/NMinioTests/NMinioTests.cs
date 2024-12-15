using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DAL.Entities;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;
using NMinio;
using NUnit.Framework;
using Logging;

namespace NMinioTests
{
    [TestFixture]
    public class NMinioClientTests
    {
        private Mock<IMinioClient> _mockMinioClient;
        private Mock<ILogger> _mockLogger;
        private NMinioClient _nMinioClient;
        private const string BucketName = "test-bucket";

        [SetUp]
        public void SetUp()
        {
            _mockMinioClient = new Mock<IMinioClient>();
            _mockLogger = new Mock<ILogger>();

            _nMinioClient = new NMinioClient(_mockMinioClient.Object, BucketName, _mockLogger.Object);
        }

        [Test]
        public async Task Upload_File_Successfully()
        {
            // Arrange
            var document = new Document(1, "Test Document", "Some content", DateTime.Now);
            var mockFile = new Mock<IFormFile>();
            var fileStream = new MemoryStream(new byte[100]);
            mockFile.Setup(f => f.OpenReadStream()).Returns(fileStream);
            mockFile.Setup(f => f.Length).Returns(100);

            _mockMinioClient
                .Setup(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), default));

            // Act
            await _nMinioClient.Upload(document, mockFile.Object);

            // Assert
            _mockLogger.Verify(l => l.Info("Successfully uploaded 1"), Times.Once);
        }

    }
}