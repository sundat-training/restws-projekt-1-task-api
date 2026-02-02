namespace feature_3_filtering.Models
{
    public class TaskItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public string Priority { get; set; } = "medium";
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Priority { get; set; }
    }

    public class UpdateTaskRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
    }

    // Query Parameters DTO for GET /api/tasks
    public class TaskQueryParameters
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Search { get; set; }
    }
}
