using FluentValidation;
using feature_3_filtering.Models;

namespace feature_3_filtering.Validators
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

    public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
    {
        public UpdateTaskRequestValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
                .When(x => x.Title != null);

            RuleFor(x => x.Status)
                .Must(BeValidStatus)
                .When(x => x.Status != null)
                .WithMessage("Status must be pending, in_progress, or completed");

            RuleFor(x => x.Priority)
                .Must(BeValidPriority)
                .When(x => x.Priority != null)
                .WithMessage("Priority must be low, medium, or high");
        }

        private bool BeValidStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return true;

            var validStatuses = new[] { "pending", "in_progress", "completed" };
            return validStatuses.Contains(status.ToLower());
        }

        private bool BeValidPriority(string? priority)
        {
            if (string.IsNullOrEmpty(priority))
                return true;

            var validPriorities = new[] { "low", "medium", "high" };
            return validPriorities.Contains(priority.ToLower());
        }
    }

    // Validator for query parameters
    public class TaskQueryParametersValidator : AbstractValidator<TaskQueryParameters>
    {
        public TaskQueryParametersValidator()
        {
            RuleFor(x => x.Status)
                .Must(BeValidStatus)
                .When(x => !string.IsNullOrEmpty(x.Status))
                .WithMessage("Status must be pending, in_progress, or completed");

            RuleFor(x => x.Priority)
                .Must(BeValidPriority)
                .When(x => !string.IsNullOrEmpty(x.Priority))
                .WithMessage("Priority must be low, medium, or high");
        }

        private bool BeValidStatus(string? status)
        {
            if (string.IsNullOrEmpty(status))
                return true;

            var validStatuses = new[] { "pending", "in_progress", "completed" };
            return validStatuses.Contains(status.ToLower());
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
