using DAL.Entities;
using FluentValidation.TestHelper;
using NUnit.Framework;
using RestAPI.DVO;

namespace DAL.Tests.Validator;

[TestFixture]
public class DocumentValidatorTests
{
    private DocumentValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new DocumentValidator();
    }

    [Test]
    public void Validate_ShouldNotHaveError_WhenTitleIsValid()
    {
        // Arrange
        var document = new Document(1, "Valid Title.pdf", "", DateTime.Now);

        // Act & Assert
        var result = _validator.TestValidate(document);
        result.ShouldNotHaveValidationErrorFor(doc => doc.Title);
    }

    [Test]
    public void Validate_ShouldHaveError_WhenTitleIsNull()
    {
        // Arrange
        var document = new Document(1, null, "", DateTime.Now);

        // Act & Assert
        var result = _validator.TestValidate(document);
        result.ShouldHaveValidationErrorFor(doc => doc.Title).WithErrorMessage("Title is required");
    }

    [Test]
    public void Validate_ShouldHaveError_WhenTitleIsTooLong()
    {
        // Arrange
        var document = new Document(1, new string('A', 101),  "", DateTime.Now);

        // Act & Assert
        var result = _validator.TestValidate(document);
        result.ShouldHaveValidationErrorFor(doc => doc.Title).WithErrorMessage("Title must be between 1 and 100 characters");
    }
}