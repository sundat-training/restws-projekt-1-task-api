using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using FluentValidation;
using feature_2_validation.Data;
using feature_2_validation.Models;
using feature_2_validation.Validators;

namespace feature_2_validation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly DatabaseConfig _database;
        private readonly CreateTaskRequestValidator _createValidator;
        private readonly UpdateTaskRequestValidator _updateValidator;

        public TasksController()
        {
            _database = new DatabaseConfig();
            _createValidator = new CreateTaskRequestValidator();
            _updateValidator = new UpdateTaskRequestValidator();
        }

        // ============================================================
        // BEREITS IMPLEMENTIERT - GET All Tasks
        // ============================================================
        [HttpGet]
        public IActionResult GetAllTasks()
        {
            try
            {
                using var connection = _database.GetConnection();
                connection.Open();

                var tasks = new List<TaskItem>();
                var sql = "SELECT * FROM tasks";
                
                using var cmd = new SqliteCommand(sql, connection);
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
        // TODO AUFGABE 1: POST /api/tasks mit Validierung implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Validierung durchführen mit _createValidator.Validate(request)
        // 2. Bei Fehlern: 400 Bad Request mit Validierungsfehlern zurückgeben
        // 3. Bei Erfolg: Task erstellen wie in Feature 1
        // 4. Gebe 201 Created mit dem neuen Task zurück
        //
        // Validierungsfehler-Format:
        // {
        //   "errors": [
        //     { "field": "title", "message": "Title is required" },
        //     { "field": "description", "message": "Description is required" }
        //   ]
        // }
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskRequest request)
        {
            // TODO AUFGABE 1: Validierung implementieren
            // 1. var validationResult = _createValidator.Validate(request);
            // 2. if (!validationResult.IsValid) return BadRequest(...)
            // 3. Dann Task erstellen (siehe Feature 1)
            
            return StatusCode(501, new { error = "Not implemented yet - implement POST validation here" });
        }

        // ============================================================
        // TODO AUFGABE 2: PUT /api/tasks/{id} mit Validierung implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Prüfe ob Task existiert (404 wenn nicht)
        // 2. Validierung durchführen mit _updateValidator.Validate(request)
        // 3. Bei Fehlern: 400 Bad Request mit Validierungsfehlern zurückgeben
        // 4. Bei Erfolg: Task aktualisieren wie in Feature 1
        // 5. Gebe aktualisierten Task zurück
        //
        // Tipp: Alle Felder in UpdateTaskRequest sind optional
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
        {
            // TODO AUFGABE 2: Validierung implementieren
            // 1. Prüfe ob Task existiert
            // 2. var validationResult = _updateValidator.Validate(request);
            // 3. if (!validationResult.IsValid) return BadRequest(...)
            // 4. Dann Task aktualisieren (siehe Feature 1)
            
            return StatusCode(501, new { error = "Not implemented yet - implement PUT validation here" });
        }

        // ============================================================
        // TODO AUFGABE 3: DELETE /api/tasks/{id} mit Validierung implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Prüfe ob Task existiert (404 wenn nicht)
        // 2. Lösche Task aus Datenbank
        // 3. Gebe 204 No Content bei Erfolg zurück
        //
        // Zusätzliche Validierung:
        // - Prüfe vor dem Löschen ob der Task überhaupt existiert
        // - Gib 404 zurück wenn nicht gefunden
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(string id)
        {
            // TODO AUFGABE 3: DELETE mit Existenz-Prüfung implementieren
            // 1. Prüfe ob Task existiert (SELECT COUNT)
            // 2. Wenn nicht: return NotFound(...)
            // 3. Wenn ja: DELETE ausführen
            // 4. return NoContent()
            
            return StatusCode(501, new { error = "Not implemented yet - implement DELETE validation here" });
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
