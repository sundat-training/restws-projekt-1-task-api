using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using FluentValidation;
using feature_4_pagination.Data;
using feature_4_pagination.Models;
using feature_4_pagination.Validators;

namespace feature_4_pagination.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly DatabaseConfig _database;
        private readonly CreateTaskRequestValidator _createValidator;
        private readonly UpdateTaskRequestValidator _updateValidator;
        private readonly TaskQueryParametersValidator _queryValidator;

        public TasksController()
        {
            _database = new DatabaseConfig();
            _createValidator = new CreateTaskRequestValidator();
            _updateValidator = new UpdateTaskRequestValidator();
            _queryValidator = new TaskQueryParametersValidator();
        }

        // ============================================================
        // TODO AUFGABE: GET /api/tasks mit Pagination
        // ============================================================
        // Was du tun musst:
        // 1. Lies Query-Parameter aus: status, priority, search, page, limit
        // 2. Validiere Parameter
        // 3. Setze Default-Werte: page=1, limit=10
        // 4. Berechne offset: (page - 1) * limit
        // 5. Baue WHERE-Bedingungen für Filter (wie in Feature 3)
        // 6. Führe COUNT(*) Query aus für totalItems
        // 7. Führe SELECT Query aus mit LIMIT und OFFSET
        // 8. Berechne Pagination-Metadaten:
        //    - totalPages = Math.Ceiling(totalItems / (double)limit)
        //    - hasNextPage = page < totalPages
        //    - hasPreviousPage = page > 1
        // 9. Baue PagedResult-Objekt und gib es zurück
        //
        // Response-Format:
        // {
        //   "data": [...],
        //   "pagination": {
        //     "page": 1,
        //     "limit": 10,
        //     "totalItems": 15,
        //     "totalPages": 2,
        //     "hasNextPage": true,
        //     "hasPreviousPage": false
        //   }
        // }
        //
        // Randfälle:
        // - page=0 oder negative -> page=1
        // - limit=0 oder negative -> limit=10 (Default)
        // - page > totalPages -> letzte Seite zurückgeben
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
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
                // TODO: Query-Parameter in DTO packen und validieren
                // TODO: Default-Werte setzen
                // TODO: Offset berechnen
                // TODO: WHERE-Bedingungen aufbauen
                // TODO: COUNT(*) Query für totalItems
                // TODO: SELECT Query mit LIMIT und OFFSET
                // TODO: Pagination-Metadaten berechnen
                // TODO: PagedResult<T> zurückgeben

                // PLATZHALTER: Aktuell werden alle Tasks ohne Pagination zurückgegeben
                using var connection = _database.GetConnection();
                connection.Open();

                var sql = "SELECT * FROM tasks";
                using var cmd = new SqliteCommand(sql, connection);
                
                var tasks = new List<TaskItem>();
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    tasks.Add(new TaskItem
                    {
                        Id = reader.GetString(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        Status = reader.GetString(3),
                        Priority = reader.GetString(4),
                        CreatedAt = reader.GetString(5),
                        UpdatedAt = reader.GetString(6)
                    });
                }

                // PLATZHALTER: Einfache Liste zurückgeben (ohne Pagination)
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch tasks", message = ex.Message });
            }
        }

        // ============================================================
        // BEREITS IMPLEMENTIERT - GET Single Task
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetTask(string id)
        {
            try
            {
                using var connection = _database.GetConnection();
                connection.Open();

                var sql = "SELECT * FROM tasks WHERE id = @id";
                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);
                
                using var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    var task = new TaskItem
                    {
                        Id = reader.GetString(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        Status = reader.GetString(3),
                        Priority = reader.GetString(4),
                        CreatedAt = reader.GetString(5),
                        UpdatedAt = reader.GetString(6)
                    };
                    return Ok(task);
                }

                return NotFound(new { error = "Task not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch task", message = ex.Message });
            }
        }

        // ============================================================
        // BEREITS IMPLEMENTIERT - POST /api/tasks mit Validierung
        // ============================================================
        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskRequest request)
        {
            try
            {
                var validationResult = _createValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                using var connection = _database.GetConnection();
                connection.Open();

                var id = Guid.NewGuid().ToString();
                var status = "pending";
                var priority = request.Priority ?? "medium";

                var sql = @"INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
                             VALUES (@id, @title, @desc, @status, @priority, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@title", request.Title);
                cmd.Parameters.AddWithValue("@desc", request.Description);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@priority", priority);
                cmd.ExecuteNonQuery();

                var task = new TaskItem
                {
                    Id = id,
                    Title = request.Title,
                    Description = request.Description,
                    Status = status,
                    Priority = priority,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    UpdatedAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                return StatusCode(201, task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create task", message = ex.Message });
            }
        }

        // ============================================================
        // BEREITS IMPLEMENTIERT - PUT /api/tasks/{id} mit Validierung
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                using var connection = _database.GetConnection();
                connection.Open();

                // Prüfe ob Task existiert
                var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
                using var checkCmd = new SqliteCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                
                var count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    return NotFound(new { error = "Task not found" });
                }

                // Validierung
                var validationResult = _updateValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                // Dynamisch Felder sammeln
                var updates = new List<string>();
                var parameters = new List<SqliteParameter>();

                if (request.Title != null)
                {
                    updates.Add("title = @title");
                    parameters.Add(new SqliteParameter("@title", request.Title));
                }

                if (request.Description != null)
                {
                    updates.Add("description = @desc");
                    parameters.Add(new SqliteParameter("@desc", request.Description));
                }

                if (request.Status != null)
                {
                    updates.Add("status = @status");
                    parameters.Add(new SqliteParameter("@status", request.Status));
                }

                if (request.Priority != null)
                {
                    updates.Add("priority = @priority");
                    parameters.Add(new SqliteParameter("@priority", request.Priority));
                }

                if (updates.Count == 0)
                {
                    return BadRequest(new { error = "No fields to update" });
                }

                updates.Add("updatedAt = CURRENT_TIMESTAMP");
                parameters.Add(new SqliteParameter("@id", id));

                var sql = $"UPDATE tasks SET {string.Join(", ", updates)} WHERE id = @id";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.ExecuteNonQuery();

                // Hole aktualisierten Task
                var selectSql = "SELECT * FROM tasks WHERE id = @id";
                using var selectCmd = new SqliteCommand(selectSql, connection);
                selectCmd.Parameters.AddWithValue("@id", id);

                using var reader = selectCmd.ExecuteReader();
                if (reader.Read())
                {
                    var task = new TaskItem
                    {
                        Id = reader.GetString(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        Status = reader.GetString(3),
                        Priority = reader.GetString(4),
                        CreatedAt = reader.GetString(5),
                        UpdatedAt = reader.GetString(6)
                    };
                    return Ok(task);
                }

                return NotFound(new { error = "Task not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update task", message = ex.Message });
            }
        }

        // ============================================================
        // BEREITS IMPLEMENTIERT - DELETE /api/tasks/{id} mit Validierung
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(string id)
        {
            try
            {
                using var connection = _database.GetConnection();
                connection.Open();

                // Prüfe ob Task existiert
                var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
                using var checkCmd = new SqliteCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                
                var count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    return NotFound(new { error = "Task not found" });
                }

                // Lösche Task
                var sql = "DELETE FROM tasks WHERE id = @id";
                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete task", message = ex.Message });
            }
        }

        // Hilfsmethode für Validierungsfehler-Formatierung
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
