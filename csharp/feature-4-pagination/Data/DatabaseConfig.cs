using Microsoft.Data.Sqlite;

namespace feature_4_pagination.Data
{
    public class DatabaseConfig
    {
        private readonly string _connectionString;

        public DatabaseConfig()
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "task-api.db");
            _connectionString = $"Data Source={dbPath}";
            
            InitializeDatabase();
        }

        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        private void InitializeDatabase()
        {
            using var connection = GetConnection();
            connection.Open();

            // Create table
            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS tasks (
                    id TEXT PRIMARY KEY,
                    title TEXT NOT NULL,
                    description TEXT NOT NULL,
                    status TEXT DEFAULT 'pending',
                    priority TEXT DEFAULT 'medium',
                    createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    updatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                )";
            
            using var createCmd = new SqliteCommand(createTableSql, connection);
            createCmd.ExecuteNonQuery();

            // Seed data if empty (15 tasks for pagination)
            var countCmd = new SqliteCommand("SELECT COUNT(*) FROM tasks", connection);
            var countResult = countCmd.ExecuteScalar();
            var count = countResult != null ? (long)countResult : 0;

            if (count == 0)
            {
                var tasks = new[]
                {
                    new[] { "task-1", "Learn C#", "Complete C# basics course", "completed", "high" },
                    new[] { "task-2", "Build REST API", "Create Task API with ASP.NET Core", "in_progress", "high" },
                    new[] { "task-3", "Write documentation", "Document all API endpoints", "pending", "medium" },
                    new[] { "task-4", "Write unit tests", "Implement xUnit tests for API", "pending", "low" },
                    new[] { "task-5", "Deploy to production", "Deploy API to cloud server", "in_progress", "medium" },
                    new[] { "task-6", "Setup CI/CD pipeline", "Configure GitHub Actions for deployment", "pending", "high" },
                    new[] { "task-7", "Add authentication", "Implement JWT-based auth", "pending", "high" },
                    new[] { "task-8", "Configure logging", "Add structured logging with Serilog", "completed", "low" },
                    new[] { "task-9", "Implement caching", "Add Redis caching layer", "pending", "medium" },
                    new[] { "task-10", "Optimize database", "Add indexes and optimize queries", "in_progress", "high" },
                    new[] { "task-11", "Create Docker containers", "Containerize the application", "completed", "medium" },
                    new[] { "task-12", "Setup monitoring", "Configure health checks and metrics", "pending", "high" },
                    new[] { "task-13", "Implement rate limiting", "Add request throttling", "pending", "medium" },
                    new[] { "task-14", "Add API versioning", "Implement versioning strategy", "in_progress", "low" },
                    new[] { "task-15", "Create admin dashboard", "Build web interface for management", "pending", "low" }
                };

                foreach (var task in tasks)
                {
                    var insertSql = @"INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
                                     VALUES (@id, @title, @desc, @status, @priority, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, connection);
                    insertCmd.Parameters.AddWithValue("@id", task[0]);
                    insertCmd.Parameters.AddWithValue("@title", task[1]);
                    insertCmd.Parameters.AddWithValue("@desc", task[2]);
                    insertCmd.Parameters.AddWithValue("@status", task[3]);
                    insertCmd.Parameters.AddWithValue("@priority", task[4]);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
