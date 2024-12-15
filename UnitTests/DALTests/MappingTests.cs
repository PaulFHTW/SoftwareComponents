using AutoMapper;
using DAL.Entities;
using NUnit.Framework;
using RestAPI.Mappings;

namespace UnitTests.DALTests;

public class MappingTests
{
    [Test]
    public void DocumentToEntity_ShouldMapCorrectly()
    {
        // Arrange
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        var document = new Document
        {
            Id = 1,
            Title = "Title 1",
            Content = "Content 1",
            UploadDate = DateTime.Now
        };
        
        // Act
        var entity = mapper.Map<Document>(document);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(document.Id));
            Assert.That(entity.Title, Is.EqualTo(document.Title));
            Assert.That(entity.Content, Is.EqualTo(document.Content));
            Assert.That(entity.UploadDate, Is.EqualTo(document.UploadDate));
        });
    }
}