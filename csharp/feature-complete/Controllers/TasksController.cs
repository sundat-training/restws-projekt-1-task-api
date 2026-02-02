using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using feature_complete.Models;
using feature_complete.Services;
using feature_complete.Validators;

namespace feature_complete.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;
        private readonly CreateTaskRequestValidator _createValidator;
        private readonly UpdateTaskRequestValidator _updateValidator;
        private readonly TaskQueryParametersValidator _queryValidator;

        public TasksController(
            TaskService taskService,
            CreateTaskRequestValidator createValidator,
            UpdateTaskRequestValidator updateValidator,
            TaskQueryParametersValidator queryValidator)
        {
            _taskService = taskService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _queryValidator = queryValidator;
        }

        [HttpGet]
        public IActionResult GetAllTasks(
            [FromQuery] string? status,
            [FromQuery] string? priority,
            [FromQuery] string? search,
            [FromQuery] int? page,
            [FromQuery] int? limit)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                var queryParams = new TaskQueryParameters
                {
                    Status = status,
                    Priority = priority,
                    Search = search,
                    Page = page,
                    Limit = limit
                };

                var validationResult = _queryValidator.Validate(queryParams);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                var result = _taskService.GetTasks(userId, status, priority, search, page, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch tasks", message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetTask(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                var task = _taskService.GetTaskById(id, userId);
                
                if (task == null)
                {
                    return NotFound(new { error = "Task not found" });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch task", message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                var validationResult = _createValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                var task = _taskService.CreateTask(request, userId);
                return StatusCode(201, task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create task", message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                var validationResult = _updateValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                var task = _taskService.UpdateTask(id, request, userId);
                
                if (task == null)
                {
                    // Check if task exists but belongs to different user
                    return StatusCode(403, new { error = "Not authorized to modify this task" });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update task", message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTask(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Authentication required" });
                }

                var deleted = _taskService.DeleteTask(id, userId);
                
                if (!deleted)
                {
                    // Check if task exists but belongs to different user
                    return StatusCode(403, new { error = "Not authorized to delete this task" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete task", message = ex.Message });
            }
        }

        private string? GetCurrentUserId()
        {
            return HttpContext.Items["UserId"] as string;
        }

        private IActionResult FormatValidationErrors(FluentValidation.Results.ValidationResult validationResult)
        {
            var errors = validationResult.Errors.Select(e => new
            {
                field = e.PropertyName.ToLower(),
                message = e.ErrorMessage
            }).ToList();

            return BadRequest(new { errors });
        }
    }
}
