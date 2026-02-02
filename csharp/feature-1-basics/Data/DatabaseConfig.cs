using Microsoft.Data.Sqlite;

namespace feature_1_basics.Data
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

            // Seed data if empty
            var countCmd = new SqliteCommand("SELECT COUNT(*) FROM tasks", connection);
            var countResult = countCmd.ExecuteScalar();
            var count = countResult != null ? (long)countResult : 0;

            if (count == 0)
            {
                var tasks = new[]
                {
                    new[] { "task-1", "Learn C#", "Complete C# basics", "completed", "high" },
                    new[] { "task-2", "Build REST API", "Create Task API", "in_progress", "high" },
                    new[] { "task-3", "Write docs", "Document all endpoints", "pending", "medium" }
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
