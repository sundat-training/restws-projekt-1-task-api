using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using feature_1_basics.Data;
using feature_1_basics.Models;

namespace feature_1_basics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly DatabaseConfig _database;

        public TasksController()
        {
            _database = new DatabaseConfig();
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
        // TODO AUFGABE 1: POST /api/tasks implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Erstelle eine neue Guid mit Guid.NewGuid().ToString()
        // 2. Setze default status = "pending"
        // 3. Setze default priority = "medium" (wenn nicht angegeben)
        // 4. Füge Task in Datenbank ein (INSERT INTO tasks ...)
        // 5. Gebe 201 Created zurück mit dem neuen Task
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskRequest request)
        {
            // HIER IMPLEMENTIEREN
            return StatusCode(501, new { error = "Not implemented yet - implement POST here" });
        }

        // ============================================================
        // TODO AUFGABE 2: PUT /api/tasks/{id} implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Hole id aus Route-Parameter
        // 2. Prüfe welche Felder im Request vorhanden sind
        // 3. Baue dynamisches UPDATE-Statement (nur übergebene Felder)
        // 4. Setze updatedAt = CURRENT_TIMESTAMP
        // 5. Prüfe ob Task existiert (404 wenn nicht)
        // 6. Gebe aktualisierten Task zurück
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
        {
            // HIER IMPLEMENTIEREN
            return StatusCode(501, new { error = "Not implemented yet - implement PUT here" });
        }

        // ============================================================
        // TODO AUFGABE 3: DELETE /api/tasks/{id} implementieren
        // ============================================================
        // Was du tun musst:
        // 1. Hole id aus Route-Parameter
        // 2. Lösche Task aus Datenbank (DELETE FROM tasks WHERE id = ...)
        // 3. Prüfe ob Task gelöscht wurde (ExecuteNonQuery gibt Anzahl betroffener Zeilen zurück)
        // 4. Gebe 204 No Content bei Erfolg zurück
        // 5. Gebe 404 wenn Task nicht gefunden
        //
        // Tipp: Siehe HINTS.md für Code-Beispiele
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(string id)
        {
            // HIER IMPLEMENTIEREN
            return StatusCode(501, new { error = "Not implemented yet - implement DELETE here" });
        }
    }
}
