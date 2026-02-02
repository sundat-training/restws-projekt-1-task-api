# Lösungshinweise - Feature 1: Basics (C#)

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Aufgaben in `Controllers/TasksController.cs`.

---

## Aufgabe 1: POST /api/tasks implementieren

### Lösungsansatz

1. Erstelle eine neue Guid mit `Guid.NewGuid().ToString()`
2. Extrahiere `title`, `description` aus dem Request
3. Setze default `status = "pending"`
4. Setze default `priority = "medium"` (wenn nicht angegeben)
5. Füge Task in SQLite-Datenbank ein
6. Gebe den neuen Task mit Status `201 Created` zurück

### Wichtige Hinweise

- Verwende `Guid.NewGuid().ToString()` für die ID
- Verwende `Microsoft.Data.Sqlite` für Datenbankzugriff
- Verwende Parameterized Queries (@parameter) gegen SQL Injection
- Nutze `CreatedAtAction` oder `StatusCode(201)` für den Return

### Code-Beispiel

```csharp
[HttpPost]
public IActionResult CreateTask([FromBody] CreateTaskRequest request)
{
    try
    {
        // Neue ID generieren
        var id = Guid.NewGuid().ToString();
        var status = "pending";
        var priority = request.Priority ?? "medium";

        using var connection = _database.GetConnection();
        connection.Open();

        var sql = @"INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
                     VALUES (@id, @title, @desc, @status, @priority, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", request.Title);
        cmd.Parameters.AddWithValue("@desc", request.Description);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@priority", priority);
        
        cmd.ExecuteNonQuery();

        // Neuen Task zurückgeben
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
```

### Alternative: Mit SELECT nach INSERT

```csharp
// Nach dem INSERT den Task aus der DB holen
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
    return StatusCode(201, task);
}
```

---

## Aufgabe 2: PUT /api/tasks/{id} implementieren

### Lösungsansatz

1. Hole `id` aus Route-Parameter
2. Prüfe welche Felder im Request vorhanden sind
3. Baue dynamisches UPDATE-Statement (nur übergebene Felder)
4. Setze `updatedAt = CURRENT_TIMESTAMP`
5. Prüfe ob Task existiert (404 wenn nicht)
6. Gebe aktualisierten Task zurück

### Wichtige Hinweise

- Verwende `string?` für optionale Felder (nullable strings)
- Nur übergebene Felder aktualisieren
- Verwende `ExecuteNonQuery()` und prüfe die Anzahl betroffener Zeilen
- `ExecuteNonQuery()` gibt die Anzahl der geänderten Zeilen zurück

### Code-Beispiel

```csharp
[HttpPut("{id}")]
public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
{
    try
    {
        using var connection = _database.GetConnection();
        connection.Open();

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

        // Prüfe ob überhaupt Felder zum Aktualisieren vorhanden
        if (updates.Count == 0)
        {
            return BadRequest(new { error = "No fields to update" });
        }

        // updatedAt immer aktualisieren
        updates.Add("updatedAt = CURRENT_TIMESTAMP");

        // ID als Parameter hinzufügen
        parameters.Add(new SqliteParameter("@id", id));

        var sql = $"UPDATE tasks SET {string.Join(", ", updates)} WHERE id = @id";

        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddRange(parameters.ToArray());
        
        var rowsAffected = cmd.ExecuteNonQuery();

        // Prüfe ob Task gefunden und aktualisiert wurde
        if (rowsAffected == 0)
        {
            return NotFound(new { error = "Task not found" });
        }

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
```

### Alternative: Einfache Variante (nur Status)

```csharp
[HttpPut("{id}")]
public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
{
    try
    {
        using var connection = _database.GetConnection();
        connection.Open();

        if (request.Status == null)
        {
            return BadRequest(new { error = "Status is required" });
        }

        var sql = "UPDATE tasks SET status = @status, updatedAt = CURRENT_TIMESTAMP WHERE id = @id";
        
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@status", request.Status);
        cmd.Parameters.AddWithValue("@id", id);
        
        var rowsAffected = cmd.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            return NotFound(new { error = "Task not found" });
        }

        return Ok(new { message = "Task updated" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
```

---

## Aufgabe 3: DELETE /api/tasks/{id} implementieren

### Lösungsansatz

1. Hole `id` aus Route-Parameter
2. Lösche Task aus der Datenbank
3. Prüfe mit `ExecuteNonQuery()` ob ein Task gelöscht wurde
4. Gebe `204 No Content` bei Erfolg zurück
5. Gebe `404` wenn Task nicht gefunden

### Wichtige Hinweise

- `ExecuteNonQuery()` gibt Anzahl betroffener Zeilen zurück
- Bei 0 Zeilen = Task nicht gefunden
- 204 No Content hat keinen Body

### Code-Beispiel

```csharp
[HttpDelete("{id}")]
public IActionResult DeleteTask(string id)
{
    try
    {
        using var connection = _database.GetConnection();
        connection.Open();

        var sql = "DELETE FROM tasks WHERE id = @id";
        
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        
        var rowsAffected = cmd.ExecuteNonQuery();

        // Prüfe ob Task gelöscht wurde
        if (rowsAffected == 0)
        {
            return NotFound(new { error = "Task not found" });
        }

        // 204 No Content = Erfolg, kein Body
        return NoContent();
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Failed to delete task", message = ex.Message });
    }
}
```

### Alternative: Mit Prüfung vor dem Löschen

```csharp
[HttpDelete("{id}")]
public IActionResult DeleteTask(string id)
{
    try
    {
        using var connection = _database.GetConnection();
        connection.Open();

        // Zuerst prüfen ob Task existiert
        var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@id", id);
        
        var count = (long)checkCmd.ExecuteScalar();
        
        if (count == 0)
        {
            return NotFound(new { error = "Task not found" });
        }

        // Dann löschen
        var deleteSql = "DELETE FROM tasks WHERE id = @id";
        using var deleteCmd = new SqliteCommand(deleteSql, connection);
        deleteCmd.Parameters.AddWithValue("@id", id);
        deleteCmd.ExecuteNonQuery();

        return NoContent();
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
```

---

## Hilfreiche SQLite Patterns in C#

### Parameterized Queries (SQL Injection verhindern)

```csharp
// RICHTIG: Mit Parametern
var sql = "SELECT * FROM tasks WHERE id = @id";
using var cmd = new SqliteCommand(sql, connection);
cmd.Parameters.AddWithValue("@id", id);

// FALSCH: String-Konkatenation (SQL Injection!)
var sql = $"SELECT * FROM tasks WHERE id = '{id}'";  // ❌ Nie so machen!
```

### Daten lesen mit Reader

```csharp
using var reader = cmd.ExecuteReader();
while (reader.Read())
{
    var task = new TaskItem
    {
        Id = reader.GetString(0),      // Index der Spalte
        Title = reader.GetString(1),
        // oder mit Namen:
        Description = reader["description"].ToString()
    };
}
```

### Einzelnen Wert lesen

```csharp
// ExecuteScalar für einzelne Werte
var countSql = "SELECT COUNT(*) FROM tasks";
using var cmd = new SqliteCommand(countSql, connection);
var count = (long)cmd.ExecuteScalar();
```

### Transaktionen (wenn mehrere Operationen)

```csharp
using var transaction = connection.BeginTransaction();
try
{
    // Operation 1
    using var cmd1 = new SqliteCommand(sql1, connection, transaction);
    cmd1.ExecuteNonQuery();
    
    // Operation 2
    using var cmd2 = new SqliteCommand(sql2, connection, transaction);
    cmd2.ExecuteNonQuery();
    
    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## ASP.NET Core Controller Patterns

### ActionResult Typen

```csharp
// 200 OK
return Ok(task);
return Ok(new List<TaskItem>());  // Leere Liste

// 201 Created
return StatusCode(201, task);
return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);

// 204 No Content
return NoContent();

// 400 Bad Request
return BadRequest(new { error = "Invalid input" });

// 404 Not Found
return NotFound(new { error = "Task not found" });

// 500 Internal Server Error
return StatusCode(500, new { error = "Database error" });
```

### Exception Handling

```csharp
try
{
    // Datenbank-Operationen
}
catch (SqliteException ex)
{
    // Spezifischer SQLite-Fehler
    return StatusCode(500, new { error = "Database error", code = ex.SqliteErrorCode });
}
catch (Exception ex)
{
    // Allgemeiner Fehler
    return StatusCode(500, new { error = "Internal server error" });
}
```

---

## Häufige Fehler vermeiden

1. **ID Generierung**
   ```csharp
   // RICHTIG:
   var id = Guid.NewGuid().ToString();
   
   // FALSCH:
   var id = new Random().Next(1000).ToString();  // ❌ Nicht eindeutig!
   ```

2. **Null-Werte prüfen**
   ```csharp
   // RICHTIG:
   if (request.Title != null)
   
   // FALSCH (für optionale Strings):
   if (!string.IsNullOrEmpty(request.Title))  // Das schließt "" aus!
   ```

3. **using Statements nicht vergessen**
   ```csharp
   // RICHTIG:
   using var connection = _database.GetConnection();
   
   // FALSCH:
   var connection = _database.GetConnection();  // ❌ Bleibt offen!
   ```

4. **Parameter-Typen**
   ```csharp
   // RICHTIG:
   cmd.Parameters.AddWithValue("@id", id);
   
   // Alternativ explizit:
   cmd.Parameters.Add("@id", SqliteType.Text).Value = id;
   ```

5. **Connection Management**
   ```csharp
   // Verbindung möglichst kurz halten
   using var connection = _database.GetConnection();
   connection.Open();
   // ... Operationen ...
   // Verbindung wird automatisch geschlossen
   ```
