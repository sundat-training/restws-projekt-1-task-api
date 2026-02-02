using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using feature_complete.Data;
using feature_complete.Models;

namespace feature_complete.Services
{
    public class AuthService
    {
        private readonly DatabaseConfig _database;

        public AuthService(DatabaseConfig database)
        {
            _database = database;
        }

        public AuthResponse? Authenticate(string username, string password)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            var sql = "SELECT * FROM users WHERE username = @username";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetString(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2)
                };

                // Password verification (plaintext for demo, use bcrypt in production)
                if (VerifyPassword(password, user.Password))
                {
                    var token = GenerateToken(user.Id);
                    
                    return new AuthResponse
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Token = token
                    };
                }
            }

            return null;
        }

        public AuthResponse? Register(string username, string password)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            // Check if username already exists
            var checkSql = "SELECT COUNT(*) FROM users WHERE username = @username";
            using var checkCmd = new SqliteCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@username", username);
            
            var count = (long)checkCmd.ExecuteScalar();
            if (count > 0)
            {
                return null; // Username already exists
            }

            // Create new user
            var id = Guid.NewGuid().ToString();
            var hashedPassword = HashPassword(password);

            var insertSql = @"INSERT INTO users (id, username, password) VALUES (@id, @username, @password)";
            using var insertCmd = new SqliteCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@id", id);
            insertCmd.Parameters.AddWithValue("@username", username);
            insertCmd.Parameters.AddWithValue("@password", hashedPassword);
            insertCmd.ExecuteNonQuery();

            var token = GenerateToken(id);

            return new AuthResponse
            {
                UserId = id,
                Username = username,
                Token = token
            };
        }

        public UserProfile? GetProfile(string userId)
        {
            using var connection = _database.GetConnection();
            connection.Open();

            var sql = "SELECT id, username FROM users WHERE id = @id";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserProfile
                {
                    UserId = reader.GetString(0),
                    Username = reader.GetString(1)
                };
            }

            return null;
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // For demo: plaintext comparison
            // In production: use BCrypt or similar
            return password == hashedPassword;
        }

        private string HashPassword(string password)
        {
            // For demo: return as-is
            // In production: use BCrypt.HashPassword(password)
            return password;
        }

        private string GenerateToken(string userId)
        {
            // Simple token generation for demo
            // In production: use JWT
            var bytes = Encoding.UTF8.GetBytes(userId + DateTime.UtcNow.Ticks);
            return Convert.ToBase64String(bytes);
        }

        public string? ValidateToken(string token)
        {
            // Simple token validation for demo
            // In production: validate JWT signature and expiration
            try
            {
                var bytes = Convert.FromBase64String(token);
                var decoded = Encoding.UTF8.GetString(bytes);
                // Extract userId from token (before the timestamp)
                return decoded.Substring(0, 36); // Guid length
            }
            catch
            {
                return null;
            }
        }
    }
}
