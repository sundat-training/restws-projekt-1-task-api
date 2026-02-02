# Lösungshinweise - Feature 5: Authentication (C#)

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Authentifizierung.

---

## Aufgabe 1: Login Endpunkt

### Vollständiges Code-Beispiel

```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    try
    {
        // Validierung
        var validationResult = _loginValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }

        using var connection = _database.GetConnection();
        connection.Open();

        // User suchen
        var sql = "SELECT * FROM users WHERE username = @username";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@username", request.Username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var user = new User
            {
                Id = reader.GetString(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2)
            };

            // Password prüfen (Klartext-Variante)
            if (user.Password == request.Password)
            {
                return Ok(new LoginResponse
                {
                    UserId = user.Id,
                    Username = user.Username
                });
            }
        }

        // User nicht gefunden oder Passwort falsch
        return Unauthorized(new { error = "Invalid credentials" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Login failed", message = ex.Message });
    }
}
```

### Mit Password Hashing (Bonus)

```csharp
// NuGet: BCrypt.Net-Next
using BCrypt;

// Beim Login:
if (BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
{
    // Passwort stimmt überein
}

// Beim Registrieren (zukünftig):
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
```

---

## Aufgabe 2: Authentication Middleware

### Vollständiges Code-Beispiel

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Öffentliche Endpunkte überspringen
        if (IsPublicPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Authorization Header lesen
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Authentication required" });
            return;
        }

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid authorization format. Use 'Bearer user-id'" });
            return;
        }

        // userId extrahieren
        var userId = authHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid token" });
            return;
        }

        // Optional: Prüfen ob User in DB existiert
        // (kann auch im Controller geprüft werden)

        // UserId in HttpContext speichern
        context.Items["UserId"] = userId;

        await _next(context);
    }

    private bool IsPublicPath(string path)
    {
        // Login und andere öffentliche Endpunkte
        return path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase);
    }
}

// Extension Method
public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}
```

### Registrierung in Program.cs

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// WICHTIG: Vor Authorization und MapControllers!
app.UseAuthenticationMiddleware();

app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Aufgabe 3: User-Filter in GET

### Vollständiges Beispiel

```csharp
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
        // UserId aus HttpContext holen (von Middleware gesetzt)
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Authentication required" });
        }

        // Validierung
        var queryParams = new TaskQueryParameters
        {
            Status = status,
            Priority = priority,
            Search = search,
            Page = page,
            Limit = limit
        };

        var validationResult = _queryValidator.Validate(queryParams);
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }

        using var connection = _database.GetConnection();
        connection.Open();

        // Pagination Werte
        int currentPage = page ?? 1;
        int currentLimit = limit ?? 10;
        currentPage = Math.Max(1, currentPage);
        currentLimit = Math.Max(1, Math.Min(100, currentLimit));
        int offset = (currentPage - 1) * currentLimit;

        // WHERE-Bedingungen - userId zuerst!
        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        // WICHTIG: User-Isolation
        conditions.Add("userId = @userId");
        parameters.Add(new SqliteParameter("@userId", userId));

        // Weitere Filter
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
        var countSql = "SELECT COUNT(*) FROM tasks WHERE " + string.Join(" AND ", conditions);
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
        var sql = "SELECT * FROM tasks WHERE " + string.Join(" AND ", conditions);
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

        var result = new PagedResult<TaskItem>
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

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Failed to fetch tasks", message = ex.Message });
    }
}

private string? GetCurrentUserId()
{
    return HttpContext.Items["UserId"] as string;
}
```

---

## Aufgabe 4: Autorisierung in PUT/DELETE

### PUT mit Autorisierung

```csharp
[HttpPut("{id}")]
public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
{
    try
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Authentication required" });
        }

        using var connection = _database.GetConnection();
        connection.Open();

        // Prüfe ob Task existiert und User gehört
        var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id AND userId = @userId";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@id", id);
        checkCmd.Parameters.AddWithValue("@userId", userId);

        var count = (long)checkCmd.ExecuteScalar();
        if (count == 0)
        {
            // Unterscheide: nicht gefunden vs. nicht autorisiert
            var existsSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
            using var existsCmd = new SqliteCommand(existsSql, connection);
            existsCmd.Parameters.AddWithValue("@id", id);
            var exists = (long)existsCmd.ExecuteScalar();

            if (exists == 0)
            {
                return NotFound(new { error = "Task not found" });
            }

            return StatusCode(403, new { error = "Not authorized to modify this task" });
        }

        // Validierung
        var validationResult = _updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }

        // ... UPDATE Logik (wie bisher)
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Failed to update task", message = ex.Message });
    }
}
```

### DELETE mit Autorisierung

```csharp
[HttpDelete("{id}")]
public IActionResult DeleteTask(string id)
{
    try
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "Authentication required" });
        }

        using var connection = _database.GetConnection();
        connection.Open();

        // Prüfe ob Task existiert und User gehört
        var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id AND userId = @userId";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@id", id);
        checkCmd.Parameters.AddWithValue("@userId", userId);

        var count = (long)checkCmd.ExecuteScalar();
        if (count == 0)
        {
            // Unterscheide: nicht gefunden vs. nicht autorisiert
            var existsSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
            using var existsCmd = new SqliteCommand(existsSql, connection);
            existsCmd.Parameters.AddWithValue("@id", id);
            var exists = (long)existsCmd.ExecuteScalar();

            if (exists == 0)
            {
                return NotFound(new { error = "Task not found" });
            }

            return StatusCode(403, new { error = "Not authorized to delete this task" });
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
```

---

## Wichtige Konzepte

### HttpContext.Items

`HttpContext.Items` ist ein Dictionary das Daten für die Dauer einer Request speichert:

```csharp
// In Middleware setzen:
context.Items["UserId"] = "user-1";
context.Items["Username"] = "alice";

// Im Controller lesen:
var userId = HttpContext.Items["UserId"] as string;
var username = HttpContext.Items["Username"] as string;
```

### Authorization Header Format

```
Authorization: Bearer user-1
Authorization: Bearer user-2
```

### 401 vs 403 Status Codes

| Code | Bedeutung | Wann verwenden |
|------|-----------|----------------|
| 401 | Unauthorized | Kein Auth-Header, ungültiger Token |
| 403 | Forbidden | Auth OK, aber keine Berechtigung für diese Ressource |

```csharp
// 401 - Keine/ungültige Authentifizierung
return Unauthorized(new { error = "Authentication required" });

// 403 - Authentifiziert, aber nicht autorisiert
return StatusCode(403, new { error = "Not authorized to access this task" });
```

---

## Häufige Fehler vermeiden

### 1. Middleware Reihenfolge

```csharp
// FALSCH:
app.UseAuthorization();
app.UseAuthenticationMiddleware();  // ❌ Zu spät!
app.MapControllers();

// RICHTIG:
app.UseAuthenticationMiddleware();  // ✓ Vor Authorization!
app.UseAuthorization();
app.MapControllers();
```

### 2. Items Key nicht vergessen

```csharp
// FALSCH:
HttpContext.Items["userId"]  // ❌ Kleines 'u'
HttpContext.Items["Userid"]  // ❌ Kleines 'i'

// RICHTIG:
HttpContext.Items["UserId"]  // ✓ Exakt gleich!
```

### 3. Null-Prüfung

```csharp
// FALSCH:
var userId = HttpContext.Items["UserId"] as string;
// Später: userId.ToString()  // ❌ NullReferenceException!

// RICHTIG:
var userId = HttpContext.Items["UserId"] as string;
if (string.IsNullOrEmpty(userId))
{
    return Unauthorized(...);
}
```

---

## Test-Befehle

```bash
# 1. Login als alice
curl -X POST http://localhost:3005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "password123"}'

# 2. Alice' Tasks anzeigen
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-1"

# 3. Bob's Tasks anzeigen
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-2"

# 4. Ohne Auth (sollte 401 geben)
curl http://localhost:3005/api/tasks

# 5. Fremden Task lesen (sollte 403 geben)
curl http://localhost:3005/api/tasks/task-4 \
  -H "Authorization: Bearer user-1"

# 6. Eigenen Task ändern
curl -X PUT http://localhost:3005/api/tasks/task-1 \
  -H "Authorization: Bearer user-1" \
  -H "Content-Type: application/json" \
  -d '{"status": "completed"}'

# 7. Fremden Task ändern (sollte 403 geben)
curl -X PUT http://localhost:3005/api/tasks/task-4 \
  -H "Authorization: Bearer user-1" \
  -H "Content-Type: application/json" \
  -d '{"status": "completed"}'
```

---

## Nächste Schritte

Nach erfolgreicher Implementierung:
1. Alle Tests in `tests.http` ausführen
2. User-Isolation prüfen (Alice vs Bob)
3. Autorisierung testen (403 bei fremden Tasks)
4. Randfälle testen (kein Auth, ungültiger Token)
5. Zu weiteren Features wechseln
