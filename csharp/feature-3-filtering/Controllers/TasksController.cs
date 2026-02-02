using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using FluentValidation;
using feature_3_filtering.Data;
using feature_3_filtering.Models;
using feature_3_filtering.Validators;

namespace feature_3_filtering.Controllers
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
        // TODO AUFGABE: GET /api/tasks mit Query-Parameter-Filterung
        // ============================================================
        // Was du tun musst:
        // 1. Query-Parameter auslesen: status, priority, search
        // 2. Validiere die Query-Parameter mit _queryValidator
        // 3. Baue die SQL-Query dynamisch auf:
        //    - Starte mit "SELECT * FROM tasks"
        //    - Füge WHERE clauses hinzu wenn Filter vorhanden
        //    - Verwende AND für kombinierte Filter
        //    - Für search: Verwende LIKE mit % Wildcards
        // 4. Führe die Query aus und gib Ergebnisse zurück
        //
        // Beispiel-URLs:
        //   GET /api/tasks?status=pending
        //   GET /api/tasks?priority=high
        //   GET /api/tasks?status=in_progress&priority=high
        //   GET /api/tasks?search=TypeScript
        //   GET /api/tasks (ohne Filter = alle Tasks)
        //
        // SQL LIKE für Suche:
        //   "(title LIKE @search OR description LIKE @search)"
        //   Parameter: @search = "%" + search + "%"
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpGet]
        public IActionResult GetAllTasks(
            [FromQuery] string? status,
            [FromQuery] string? priority,
            [FromQuery] string? search)
        {
            try
            {
                // TODO: Query-Parameter in DTO packen und validieren
                var queryParams = new TaskQueryParameters
                {
                    Status = status,
                    Priority = priority,
                    Search = search
                };

                // TODO: Validierung durchführen
                // var validationResult = _queryValidator.Validate(queryParams);
                // if (!validationResult.IsValid) return FormatValidationErrors(validationResult);

                using var connection = _database.GetConnection();
                connection.Open();

                // TODO: SQL-Query dynamisch aufbauen
                // 1. Starte mit "SELECT * FROM tasks"
                // 2. Erstelle eine Liste für WHERE-Bedingungen
                // 3. Erstelle eine Liste für Parameter
                // 4. Füge Bedingungen hinzu wenn Filter vorhanden
                //    - if (!string.IsNullOrEmpty(status)) conditions.Add("status = @status")
                //    - if (!string.IsNullOrEmpty(priority)) conditions.Add("priority = @priority")
                //    - if (!string.IsNullOrEmpty(search)) conditions.Add("(title LIKE @search OR description LIKE @search)")
                // 5. Wenn conditions.Count > 0: query += " WHERE " + string.Join(" AND ", conditions)
                // 6. Erstelle SqliteCommand und füge Parameter hinzu
                // 7. Führe Query aus

                // PLATZHALTER: Aktuell werden alle Tasks zurückgegeben
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
