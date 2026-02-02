using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using FluentValidation;
using feature_5_auth.Data;
using feature_5_auth.Models;
using feature_5_auth.Validators;

namespace feature_5_auth.Controllers
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

        // Hilfsmethode: Hole UserId aus HttpContext (von Middleware gesetzt)
        private string? GetCurrentUserId()
        {
            // TODO: Lies die UserId aus HttpContext.Items["UserId"]
            // Dies wird von der AuthenticationMiddleware gesetzt
            return HttpContext.Items["UserId"] as string;
        }

        // ============================================================
        // TODO AUFGABE: GET /api/tasks mit User-Filter
        // ============================================================
        // Was du tun musst:
        // 1. Hole die UserId des eingeloggten Users: var userId = GetCurrentUserId()
        // 2. Wenn keine UserId: return 401 Unauthorized
        // 3. Erweitere die SQL-Query um: WHERE userId = @userId
        // 4. Alle anderen Filter (status, priority, search) bleiben erhalten
        // 5. Pagination bleibt erhalten
        // 6. Nur Tasks des eingeloggten Users zurückgeben
        //
        // SQL WHERE Reihenfolge:
        // WHERE userId = @userId AND status = @status AND priority = @priority
        //
        // Tipp: Füge userId als ersten Filter hinzu (immer vorhanden)
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
                // TODO: UserId des eingeloggten Users holen
                // TODO: Prüfen ob User eingeloggt ist
                // TODO: userId als Filter hinzufügen

                // PLATZHALTER: Aktuell werden alle Tasks zurückgegeben
                using var connection = _database.GetConnection();
                connection.Open();

                // Default-Werte
                int currentPage = page ?? 1;
                int currentLimit = limit ?? 10;
                currentPage = Math.Max(1, currentPage);
                currentLimit = Math.Max(1, Math.Min(100, currentLimit));
                int offset = (currentPage - 1) * currentLimit;

                // WHERE-Bedingungen sammeln
                var conditions = new List<string>();
                var parameters = new List<SqliteParameter>();

                // TODO: Hier userId-Filter hinzufügen!
                // conditions.Add("userId = @userId");
                // parameters.Add(new SqliteParameter("@userId", userId));

                if (!string.IsNullOrEmpty(status))
                {
                    conditions.Add("status = @status");
                    parameters.Add(new SqliteParameter("@status", status));
                }

                if (!string.IsNullOrEmpty(priority))
                {
                    conditions.Add("priority = @priority");
                    parameters.Add(new SqliteParameter("@priority", priority));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    conditions.Add("(title LIKE @search OR description LIKE @search)");
                    var searchPattern = "%" + search + "%";
                    parameters.Add(new SqliteParameter("@search", searchPattern));
                }

                // COUNT Query
                var countSql = "SELECT COUNT(*) FROM tasks";
                if (conditions.Count > 0)
                {
                    countSql += " WHERE " + string.Join(" AND ", conditions);
                }

                using var countCmd = new SqliteCommand(countSql, connection);
                countCmd.Parameters.AddRange(parameters.ToArray());
                var totalItems = (long)countCmd.ExecuteScalar();

                int totalPages = (int)Math.Ceiling(totalItems / (double)currentLimit);
                if (currentPage > totalPages && totalPages > 0)
                {
                    currentPage = totalPages;
                    offset = (currentPage - 1) * currentLimit;
                }

                // SELECT Query
                var sql = "SELECT * FROM tasks";
                if (conditions.Count > 0)
                {
                    sql += " WHERE " + string.Join(" AND ", conditions);
                }
                sql += " LIMIT @limit OFFSET @offset";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.Parameters.Add(new SqliteParameter("@limit", currentLimit));
                cmd.Parameters.Add(new SqliteParameter("@offset", offset));

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
                        UserId = reader.GetString(5),
                        CreatedAt = reader.GetString(6),
                        UpdatedAt = reader.GetString(7)
                    });
                }

                bool hasNextPage = currentPage < totalPages;
                bool hasPreviousPage = currentPage > 1;

                var result = new PagedResult<TaskItem>
                {
                    Data = tasks,
                    Pagination = new PaginationInfo
                    {
                        Page = currentPage,
                        Limit = currentLimit,
                        TotalItems = (int)totalItems,
                        TotalPages = totalPages,
                        HasNextPage = hasNextPage,
                        HasPreviousPage = hasPreviousPage
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch tasks", message = ex.Message });
            }
        }

        // ============================================================
        // TODO AUFGABE: GET /api/tasks/{id} mit User-Prüfung
        // ============================================================
        // Was du tun musst:
        // 1. Hole UserId des eingeloggten Users
        // 2. Suche Task mit id UND userId
        // 3. Wenn nicht gefunden: return 404 oder 403 (nicht autorisiert)
        // 4. User darf nur seine eigenen Tasks sehen!
        //
        // SQL: "SELECT * FROM tasks WHERE id = @id AND userId = @userId"
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetTask(string id)
        {
            try
            {
                // TODO: UserId holen
                // TODO: Task mit id UND userId suchen
                // TODO: 404 oder 403 wenn nicht gefunden

                // PLATZHALTER
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
                        UserId = reader.GetString(5),
                        CreatedAt = reader.GetString(6),
                        UpdatedAt = reader.GetString(7)
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
        // TODO AUFGABE: POST /api/tasks mit User-Zuordnung
        // ============================================================
        // Was du tun musst:
        // 1. Hole UserId des eingeloggten Users
        // 2. Validierung durchführen
        // 3. INSERT mit userId des eingeloggten Users
        //
        // SQL: "INSERT INTO tasks (..., userId) VALUES (..., @userId)"
        // ============================================================
        [HttpPost]
        public IActionResult CreateTask([FromBody] CreateTaskRequest request)
        {
            try
            {
                // TODO: UserId holen
                // TODO: Prüfen ob User eingeloggt ist
                
                var validationResult = _createValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

                using var connection = _database.GetConnection();
                connection.Open();

                var id = Guid.NewGuid().ToString();
                var status = "pending";
                var taskPriority = request.Priority ?? "medium";
                
                // TODO: Hier die userId des eingeloggten Users verwenden!
                var userId = "placeholder-user-id";

                var sql = @"INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt) 
                             VALUES (@id, @title, @desc, @status, @priority, @userId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

                using var cmd = new SqliteCommand(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@title", request.Title);
                cmd.Parameters.AddWithValue("@desc", request.Description);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@priority", taskPriority);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.ExecuteNonQuery();

                var task = new TaskItem
                {
                    Id = id,
                    Title = request.Title,
                    Description = request.Description,
                    Status = status,
                    Priority = taskPriority,
                    UserId = userId,
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
        // TODO AUFGABE: PUT /api/tasks/{id} mit Autorisierung
        // ============================================================
        // Was du tun musst:
        // 1. Hole UserId des eingeloggten Users
        // 2. Prüfe ob Task existiert UND dem User gehört
        // 3. Wenn nicht: return 403 Forbidden
        // 4. Wenn ja: UPDATE durchführen
        //
        // SQL: "SELECT * FROM tasks WHERE id = @id AND userId = @userId"
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
        {
            try
            {
                // TODO: UserId holen
                // TODO: Prüfen ob Task dem User gehört
                // TODO: 403 Forbidden wenn nicht autorisiert

                using var connection = _database.GetConnection();
                connection.Open();

                // PLATZHALTER: Prüft nicht ob Task dem User gehört!
                var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
                using var checkCmd = new SqliteCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                
                var count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    return NotFound(new { error = "Task not found" });
                }

                var validationResult = _updateValidator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return FormatValidationErrors(validationResult);
                }

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
                        UserId = reader.GetString(5),
                        CreatedAt = reader.GetString(6),
                        UpdatedAt = reader.GetString(7)
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
        // TODO AUFGABE: DELETE /api/tasks/{id} mit Autorisierung
        // ============================================================
        // Was du tun musst:
        // 1. Hole UserId des eingeloggten Users
        // 2. Prüfe ob Task existiert UND dem User gehört
        // 3. Wenn nicht: return 403 Forbidden
        // 4. Wenn ja: DELETE durchführen
        //
        // SQL: "DELETE FROM tasks WHERE id = @id AND userId = @userId"
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(string id)
        {
            try
            {
                // TODO: UserId holen
                // TODO: Prüfen ob Task dem User gehört
                // TODO: 403 Forbidden wenn nicht autorisiert

                using var connection = _database.GetConnection();
                connection.Open();

                // PLATZHALTER: Prüft nicht ob Task dem User gehört!
                var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
                using var checkCmd = new SqliteCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@id", id);
                
                var count = (long)checkCmd.ExecuteScalar();
                if (count == 0)
                {
                    return NotFound(new { error = "Task not found" });
                }

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
