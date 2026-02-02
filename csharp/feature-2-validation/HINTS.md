# Lösungshinweise - Feature 2: Validation (C#)

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Validierung in `Controllers/TasksController.cs`.

---

## Aufgabe 1: POST /api/tasks mit Validierung

### Lösungsansatz

1. Rufe `_createValidator.Validate(request)` auf
2. Prüfe `validationResult.IsValid`
3. Bei Fehlern: Formatiere Fehler und gib `400 Bad Request` zurück
4. Bei Erfolg: Erstelle Task wie in Feature 1
5. Gib `201 Created` zurück

### Code-Beispiel

```csharp
[HttpPost]
public IActionResult CreateTask([FromBody] CreateTaskRequest request)
{
    try
    {
        // 1. Validierung durchführen
        var validationResult = _createValidator.Validate(request);
        
        // 2. Bei Fehlern: 400 Bad Request
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }
        
        // 3. Task erstellen (wie in Feature 1)
        using var connection = _database.GetConnection();
        connection.Open();
        
        var id = Guid.NewGuid().ToString();
        var status = "pending";
        var priority = request.Priority ?? "medium";
        
        var sql = @"INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
                     VALUES (@id, @title, @desc, @status, @priority, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
        
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", request.Title);
        cmd.Parameters.AddWithValue("@desc", request.Description);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@priority", priority);
        
        cmd.ExecuteNonQuery();
        
        // 4. Neuen Task zurückgeben
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

---

## Aufgabe 2: PUT /api/tasks/{id} mit Validierung

### Lösungsansatz

1. Prüfe ob Task existiert (SELECT) - sonst 404
2. Führe Validierung durch
3. Bei Fehlern: 400 Bad Request
4. Baue dynamisches UPDATE (nur übergebene Felder)
5. Gib aktualisierten Task zurück

### Code-Beispiel

```csharp
[HttpPut("{id}")]
public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
{
    try
    {
        using var connection = _database.GetConnection();
        connection.Open();
        
        // 1. Prüfe ob Task existiert
        var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@id", id);
        
        var count = (long)checkCmd.ExecuteScalar();
        if (count == 0)
        {
            return NotFound(new { error = "Task not found" });
        }
        
        // 2. Validierung durchführen
        var validationResult = _updateValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }
        
        // 3. Dynamisch Felder sammeln
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
        
        // 4. UPDATE ausführen
        var sql = $"UPDATE tasks SET {string.Join(", ", updates)} WHERE id = @id";
        
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddRange(parameters.ToArray());
        cmd.ExecuteNonQuery();
        
        // 5. Aktualisierten Task zurückgeben
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

---

## Aufgabe 3: DELETE /api/tasks/{id} mit Existenz-Prüfung

### Lösungsansatz

1. Prüfe ob Task existiert (SELECT COUNT)
2. Wenn nicht: 404 Not Found
3. Wenn ja: DELETE ausführen
4. Gebe 204 No Content zurück

### Code-Beispiel

```csharp
[HttpDelete("{id}")]
public IActionResult DeleteTask(string id)
{
    try
    {
        using var connection = _database.GetConnection();
        connection.Open();
        
        // 1. Prüfe ob Task existiert
        var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id";
        using var checkCmd = new SqliteCommand(checkSql, connection);
        checkCmd.Parameters.AddWithValue("@id", id);
        
        var count = (long)checkCmd.ExecuteScalar();
        if (count == 0)
        {
            return NotFound(new { error = "Task not found" });
        }
        
        // 2. Task löschen
        var sql = "DELETE FROM tasks WHERE id = @id";
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
        
        // 3. 204 No Content zurückgeben
        return NoContent();
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Failed to delete task", message = ex.Message });
    }
}
```

---

## Hilfsmethode: FormatValidationErrors

Diese Methode ist bereits im Controller vorhanden:

```csharp
private IActionResult FormatValidationErrors(FluentValidation.Results.ValidationResult validationResult)
{
    var errors = validationResult.Errors.Select(e => new
    {
        field = e.PropertyName.ToLower(),
        message = e.ErrorMessage
    }).ToList();

    return BadRequest(new { errors });
}
```

### Ergebnis-Format

```json
{
  "errors": [
    {
      "field": "title",
      "message": "Title is required"
    },
    {
      "field": "description",
      "message": "Description is required"
    }
  ]
}
```

---

## FluentValidation Patterns

### Regeln definieren

```csharp
public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        // Pflichtfeld
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");
        
        // Längenbeschränkung
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        
        // Bedingte Regel (nur wenn nicht null)
        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .When(x => x.Priority != null)
            .WithMessage("Priority must be low, medium, or high");
    }
    
    private bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return true;
        
        var validPriorities = new[] { "low", "medium", "high" };
        return validPriorities.Contains(priority.ToLower());
    }
}
```

### Validierung ausführen

```csharp
var validationResult = validator.Validate(request);

if (!validationResult.IsValid)
{
    // Fehler verarbeiten
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

---

## Fehlerbehandlung

### Datenbank-Fehler abfangen

```csharp
try
{
    // Datenbank-Operationen
}
catch (SqliteException ex)
{
    // Spezifischer SQLite-Fehler
    return StatusCode(500, new { 
        error = "Database error", 
        code = ex.SqliteErrorCode,
        message = ex.Message 
    });
}
catch (Exception ex)
{
    // Allgemeiner Fehler
    return StatusCode(500, new { 
        error = "Internal server error",
        message = ex.Message 
    });
}
```

### Häufige Fehler vermeiden

1. **Validator nicht vergessen**
   ```csharp
   // RICHTIG:
   var validationResult = _createValidator.Validate(request);
   
   // FALSCH:
   var validationResult = new ValidationResult(); // ❌ Leeres Ergebnis!
   ```

2. **IsValid prüfen**
   ```csharp
   // RICHTIG:
   if (!validationResult.IsValid)
   {
       return FormatValidationErrors(validationResult);
   }
   
   // FALSCH:
   if (validationResult.IsValid) // ❌ Logik umgekehrt!
   {
       return FormatValidationErrors(validationResult);
   }
   ```

3. **Existenz-Prüfung bei PUT/DELETE**
   ```csharp
   // RICHTIG:
   var count = (long)checkCmd.ExecuteScalar();
   if (count == 0) return NotFound(...);
   
   // FALSCH:
   var count = (int)checkCmd.ExecuteScalar(); // ❌ Long vs Int!
   ```

---

## Vergleich: Feature 1 vs Feature 2

| Feature 1 (Basics) | Feature 2 (Validation) |
|-------------------|----------------------|
| Direkte DB-Operation | Validierung + DB-Operation |
| Keine Fehler-Prüfung für Input | Umfassende Input-Validierung |
| 500 bei allen Fehlern | 400 für Validierung, 404 für Not Found |
| Einfacher Code | Strukturierter mit Validierung |

---

## Nächste Schritte

Nach erfolgreicher Implementierung:
1. Alle Tests in `tests.http` ausführen
2. Mit `hint.http` vergleichen
3. Zu weiteren Features wechseln
