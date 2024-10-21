using DAL.Entities;
using FluentValidation;

namespace RestAPI.DVO;

public class DocumentValidator : AbstractValidator<Document>
{
    // FluentValidation for the Document
    public DocumentValidator()
    {
        RuleFor(doc => doc.Id)
            .NotNull().WithMessage("File is required");
        
        RuleFor(doc => doc.Title)
            .NotNull().WithMessage("Title is required")
            .Length(1, 100).WithMessage("Title must be between 1 and 100 characters");
        
        RuleFor(doc => doc.UploadDate)
            .NotNull().WithMessage("Upload date is required");

        RuleFor(doc => doc.Path)
            .NotNull().WithMessage("Path is required");
    }
}