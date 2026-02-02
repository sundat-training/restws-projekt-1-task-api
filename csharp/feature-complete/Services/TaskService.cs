using Microsoft.Data.Sqlite;
using feature_complete.Data;
using feature_complete.Models;

namespace feature_complete.Services
{
    public class TaskService
    {
        private readonly DatabaseConfig _database;

        public TaskService(DatabaseConfig database)
        {
            _database = database;
        }

        public PagedResult<TaskItem> GetTasks(
            string userId,
            string? status = null,
            string? priority = null,
            string? search = null,
            int? page = null,
            int? limit = null)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            // Default pagination values
            int currentPage = page ?? 1;
            int currentLimit = limit ?? 10;
            currentPage = Math.Max(1, currentPage);
            currentLimit = Math.Max(1, Math.Min(100, currentLimit));
            int offset = (currentPage - 1) * currentLimit;

            // Build WHERE conditions
            var conditions = new List<string>();
            var parameters = new List<SqliteParameter>();

            // User isolation - always filter by userId
            conditions.Add("userId = @userId");
            parameters.Add(new SqliteParameter("@userId", userId));

            // Optional filters
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

            // Count total items
            var countSql = "SELECT COUNT(*) FROM tasks WHERE " + string.Join(" AND ", conditions);
            using var countCmd = new SqliteCommand(countSql, connection);
            countCmd.Parameters.AddRange(parameters.ToArray());
            var totalItems = (long)countCmd.ExecuteScalar();

            // Calculate pagination
            int totalPages = (int)Math.Ceiling(totalItems / (double)currentLimit);
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = totalPages;
                offset = (currentPage - 1) * currentLimit;
            }

            // Get paginated results
            var sql = "SELECT * FROM tasks WHERE " + string.Join(" AND ", conditions);
            sql += " ORDER BY createdAt DESC";
            sql += " LIMIT @limit OFFSET @offset";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.Parameters.Add(new SqliteParameter("@limit", currentLimit));
            cmd.Parameters.Add(new SqliteParameter("@offset", offset));

            var tasks = new List<TaskItem>();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapTaskFromReader(reader));
            }

            return new PagedResult<TaskItem>
            {
                Data = tasks,
                Pagination = new PaginationInfo
                {
                    Page = currentPage,
                    Limit = currentLimit,
                    TotalItems = (int)totalItems,
                    TotalPages = totalPages,
                    HasNextPage = currentPage < totalPages,
                    HasPreviousPage = currentPage > 1
                }
            };
        }

        public TaskItem? GetTaskById(string id, string userId)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            var sql = "SELECT * FROM tasks WHERE id = @id AND userId = @userId";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapTaskFromReader(reader);
            }

            return null;
        }

        public TaskItem CreateTask(CreateTaskRequest request, string userId)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            var id = Guid.NewGuid().ToString();
            var status = "pending";
            var priority = request.Priority ?? "medium";

            var sql = @"INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt) 
                         VALUES (@id, @title, @desc, @status, @priority, @userId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@title", request.Title);
            cmd.Parameters.AddWithValue("@desc", request.Description);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@priority", priority);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.ExecuteNonQuery();

            return new TaskItem
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                Status = status,
                Priority = priority,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                UpdatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
        }

        public TaskItem? UpdateTask(string id, UpdateTaskRequest request, string userId)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            // Check if task exists and belongs to user
            var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id AND userId = @userId";
            using var checkCmd = new SqliteCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@id", id);
            checkCmd.Parameters.AddWithValue("@userId", userId);

            var count = (long)checkCmd.ExecuteScalar();
            if (count == 0)
            {
                return null;
            }

            // Build update query
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
                // No fields to update, return current task
                return GetTaskById(id, userId);
            }

            updates.Add("updatedAt = CURRENT_TIMESTAMP");
            parameters.Add(new SqliteParameter("@id", id));

            var sql = $"UPDATE tasks SET {string.Join(", ", updates)} WHERE id = @id AND userId = @userId";
            parameters.Add(new SqliteParameter("@userId", userId));

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.ExecuteNonQuery();

            return GetTaskById(id, userId);
        }

        public bool DeleteTask(string id, string userId)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            var sql = "DELETE FROM tasks WHERE id = @id AND userId = @userId";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@userId", userId);

            var rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool TaskExists(string id, string userId)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            var sql = "SELECT COUNT(*) FROM tasks WHERE id = @id AND userId = @userId";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@userId", userId);

            var count = (long)cmd.ExecuteScalar();
            return count > 0;
        }

        private TaskItem MapTaskFromReader(SqliteDataReader reader)
        {
            return new TaskItem
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
        }
    }
}
