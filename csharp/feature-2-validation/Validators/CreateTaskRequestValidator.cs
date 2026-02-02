using FluentValidation;
using feature_2_validation.Models;

namespace feature_2_validation.Validators
{
    public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
    {
        public CreateTaskRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");

            RuleFor(x => x.Priority)
                .Must(BeValidPriority)
                .When(x => x.Priority != null)
                .WithMessage("Priority must be low, medium, or high");
        }

        private bool BeValidPriority(string? priority)
        {
            if (string.IsNullOrEmpty(priority))
                return true;

            var validPriorities = new[] { "low", "medium", "high" };
            return validPriorities.Contains(priority.ToLower());
        }
    }
}
