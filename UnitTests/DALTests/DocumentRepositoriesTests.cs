using NUnit.Framework;
using DAL.Repositories;
using DAL.Entities;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DAL.Tests.Repositories
{
    [TestFixture]
    public class DocumentRepositoryTests
    {
        private DocumentContext _context;
        private DocumentRepository _repository;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<DocumentContext>()
                .UseInMemoryDatabase(databaseName: "DocumentTestDb")
                .Options;

            _context = new DocumentContext(options);
            _repository = new DocumentRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllDocuments()
        {
            // Arrange
            var documents = new List<Document>
            {
                new Document(1, "Title 1", DateTime.Now, "/path/1"),
                new Document(2, "Title 2", DateTime.Now, "/path/2")
            };
            _context.TodoItems.AddRange(documents);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnDocument_WhenDocumentExists()
        {
            // Arrange
            var document = new Document(1, "Title 1", DateTime.Now, "/path/1");
            _context.TodoItems.Add(document);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task AddAsync_ShouldAddDocument()
        {
            // Arrange
            var document = new Document(3, "Title 3", DateTime.Now, "/path/3");

            // Act
            await _repository.AddAsync(document);
            var result = await _context.TodoItems.FindAsync(3);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Title 3", result.Title);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateDocument()
        {
            // Arrange
            var document = new Document(4, "Title 4", DateTime.Now, "/path/4");
            _context.TodoItems.Add(document);
            await _context.SaveChangesAsync();

            // Modify document and update
            document.Title = "Updated Title";
            await _repository.UpdateAsync(document);

            // Act
            var result = await _context.TodoItems.FindAsync(4);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Title", result.Title);
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveDocument_WhenDocumentExists()
        {
            // Arrange
            var document = new Document(5, "Title 5", DateTime.Now, "/path/5");
            _context.TodoItems.Add(document);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(5);
            var result = await _context.TodoItems.FindAsync(5);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task DeleteAsync_ShouldNotThrow_WhenDocumentDoesNotExist()
        {
            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _repository.DeleteAsync(999));
        }
    }
}