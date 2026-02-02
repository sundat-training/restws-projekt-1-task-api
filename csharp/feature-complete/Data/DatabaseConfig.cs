using Microsoft.Data.Sqlite;

namespace feature_complete.Data
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

            // Create users table
            var createUsersTableSql = @"
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    username TEXT NOT NULL UNIQUE,
                    password TEXT NOT NULL
                )";
            
            using var createUsersCmd = new SqliteCommand(createUsersTableSql, connection);
            createUsersCmd.ExecuteNonQuery();

            // Create tasks table with userId
            var createTasksTableSql = @"
                CREATE TABLE IF NOT EXISTS tasks (
                    id TEXT PRIMARY KEY,
                    title TEXT NOT NULL,
                    description TEXT NOT NULL,
                    status TEXT DEFAULT 'pending',
                    priority TEXT DEFAULT 'medium',
                    userId TEXT NOT NULL,
                    createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    updatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (userId) REFERENCES users(id)
                )";
            
            using var createTasksCmd = new SqliteCommand(createTasksTableSql, connection);
            createTasksCmd.ExecuteNonQuery();

            // Seed users if empty
            var usersCountCmd = new SqliteCommand("SELECT COUNT(*) FROM users", connection);
            var usersCountResult = usersCountCmd.ExecuteScalar();
            var usersCount = usersCountResult != null ? (long)usersCountResult : 0;

            if (usersCount == 0)
            {
                var users = new[]
                {
                    new[] { "user-1", "alice", "password123" },
                    new[] { "user-2", "bob", "password456" }
                };

                foreach (var user in users)
                {
                    var insertSql = @"INSERT INTO users (id, username, password) VALUES (@id, @username, @password)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, connection);
                    insertCmd.Parameters.AddWithValue("@id", user[0]);
                    insertCmd.Parameters.AddWithValue("@username", user[1]);
                    insertCmd.Parameters.AddWithValue("@password", user[2]);
                    insertCmd.ExecuteNonQuery();
                }
            }

            // Seed tasks if empty
            var tasksCountCmd = new SqliteCommand("SELECT COUNT(*) FROM tasks", connection);
            var tasksCountResult = tasksCountCmd.ExecuteScalar();
            var tasksCount = tasksCountResult != null ? (long)tasksCountResult : 0;

            if (tasksCount == 0)
            {
                var aliceTasks = new[]
                {
                    new[] { "task-1", "Learn C#", "Complete C# basics course", "completed", "high", "user-1" },
                    new[] { "task-2", "Build REST API", "Create Task API with ASP.NET Core", "in_progress", "high", "user-1" },
                    new[] { "task-3", "Write documentation", "Document all API endpoints", "pending", "medium", "user-1" }
                };

                foreach (var task in aliceTasks)
                {
                    var insertSql = @"INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt) 
                                     VALUES (@id, @title, @desc, @status, @priority, @userId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, connection);
                    insertCmd.Parameters.AddWithValue("@id", task[0]);
                    insertCmd.Parameters.AddWithValue("@title", task[1]);
                    insertCmd.Parameters.AddWithValue("@desc", task[2]);
                    insertCmd.Parameters.AddWithValue("@status", task[3]);
                    insertCmd.Parameters.AddWithValue("@priority", task[4]);
                    insertCmd.Parameters.AddWithValue("@userId", task[5]);
                    insertCmd.ExecuteNonQuery();
                }

                var bobTasks = new[]
                {
                    new[] { "task-4", "Setup CI/CD", "Configure GitHub Actions", "pending", "high", "user-2" },
                    new[] { "task-5", "Add authentication", "Implement JWT-based auth", "in_progress", "medium", "user-2" }
                };

                foreach (var task in bobTasks)
                {
                    var insertSql = @"INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt) 
                                     VALUES (@id, @title, @desc, @status, @priority, @userId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, connection);
                    insertCmd.Parameters.AddWithValue("@id", task[0]);
                    insertCmd.Parameters.AddWithValue("@title", task[1]);
                    insertCmd.Parameters.AddWithValue("@desc", task[2]);
                    insertCmd.Parameters.AddWithValue("@status", task[3]);
                    insertCmd.Parameters.AddWithValue("@priority", task[4]);
                    insertCmd.Parameters.AddWithValue("@userId", task[5]);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
