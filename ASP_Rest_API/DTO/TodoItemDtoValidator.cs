using FluentValidation;

namespace ASP_Rest_API.DTO
{
    public class TodoItemDtoValidator : AbstractValidator<TodoItemDto>
    {
        public TodoItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("The task name cannot be empty.")
                .MaximumLength(100).WithMessage("The task name must not exceed 100 chars.");

            RuleFor(x => x.IsComplete)
                .NotNull().WithMessage("The task completion status must be specified.");
        }
    }
}
