# Lösungshinweise - Feature 3: Filtering (C#)

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Query-Parameter-Filterung in `Controllers/TasksController.cs`.

---

## Aufgabe: GET /api/tasks mit Query-Parameter-Filterung

### Lösungsansatz

1. Query-Parameter auslesen (sind bereits als Methoden-Parameter vorhanden)
2. In DTO packen und validieren
3. SQL-Query dynamisch aufbauen:
   - Starte mit `SELECT * FROM tasks`
   - Sammle WHERE-Bedingungen in einer Liste
   - Sammle Parameter in einer Liste
   - Füge WHERE hinzu wenn Bedingungen vorhanden
4. Query ausführen und Ergebnisse zurückgeben

### Code-Beispiel

```csharp
[HttpGet]
public IActionResult GetAllTasks(
    [FromQuery] string? status,
    [FromQuery] string? priority,
    [FromQuery] string? search)
{
    try
    {
        // 1. Query-Parameter in DTO packen
        var queryParams = new TaskQueryParameters
        {
            Status = status,
            Priority = priority,
            Search = search
        };

        // 2. Validierung durchführen
        var validationResult = _queryValidator.Validate(queryParams);
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }

        using var connection = _database.GetConnection();
        connection.Open();

        // 3. SQL-Query dynamisch aufbauen
        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        // Filter: status
        if (!string.IsNullOrEmpty(status))
        {
            conditions.Add("status = @status");
            parameters.Add(new SqliteParameter("@status", status));
        }

        // Filter: priority
        if (!string.IsNullOrEmpty(priority))
        {
            conditions.Add("priority = @priority");
            parameters.Add(new SqliteParameter("@priority", priority));
        }

        // Filter: search (LIKE in title und description)
        if (!string.IsNullOrEmpty(search))
        {
            conditions.Add("(title LIKE @search OR description LIKE @search)");
            var searchPattern = "%" + search + "%";
            parameters.Add(new SqliteParameter("@search", searchPattern));
        }

        // Query zusammenbauen
        var sql = "SELECT * FROM tasks";
        if (conditions.Count > 0)
        {
            sql += " WHERE " + string.Join(" AND ", conditions);
        }

        // 4. Query ausführen
        using var cmd = new SqliteCommand(sql, connection);
        cmd.Parameters.AddRange(parameters.ToArray());

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
```

---

## Wichtige Konzepte

### Dynamische SQL-Queries

```csharp
// Schritt 1: Bedingungen sammeln
var conditions = new List<string>();
var parameters = new List<SqliteParameter>();

// Schritt 2: Bedingungen hinzufügen (nur wenn Filter vorhanden)
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

// Schritt 3: Query zusammenbauen
var sql = "SELECT * FROM tasks";
if (conditions.Count > 0)
{
    sql += " WHERE " + string.Join(" AND ", conditions);
}
// Ergebnis ohne Filter: "SELECT * FROM tasks"
// Ergebnis mit Filter: "SELECT * FROM tasks WHERE status = @status AND priority = @priority"
```

### SQL LIKE für Suche

```csharp
// Suche in mehreren Spalten
if (!string.IsNullOrEmpty(search))
{
    // % ist der Wildcard für "beliebige Zeichen"
    // %search% findet "search" überall im Text
    conditions.Add("(title LIKE @search OR description LIKE @search)");
    
    // Pattern erstellen
    var searchPattern = "%" + search + "%";
    parameters.Add(new SqliteParameter("@search", searchPattern));
}

// Beispiel: search = "API"
// Pattern: "%API%"
// SQL: "(title LIKE '%API%' OR description LIKE '%API%')"
// Findet: "Build REST API", "API Documentation", etc.
```

### Case-Insensitivity

SQLite's `LIKE` ist standardmäßig case-insensitive für ASCII-Zeichen:

```csharp
// Diese Suche findet "API", "api", "Api", etc.
conditions.Add("(title LIKE @search OR description LIKE @search)");
```

Für explizite Case-Insensitivity (falls nötig):

```csharp
// Mit COLLATE NOCASE
conditions.Add("(title LIKE @search COLLATE NOCASE OR description LIKE @search COLLATE NOCASE)");
```

---

## Kombinierte Filter

Mehrere Filter werden mit `AND` verknüpft:

```csharp
// Request: ?status=pending&priority=high
// SQL: "SELECT * FROM tasks WHERE status = @status AND priority = @priority"

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

// string.Join(" AND ", conditions) erzeugt:
// "status = @status AND priority = @priority"
```

---

## Parameterized Queries (SQL Injection verhindern)

**RICHTIG:**
```csharp
conditions.Add("status = @status");
parameters.Add(new SqliteParameter("@status", status));

// SQL: "SELECT * FROM tasks WHERE status = @status"
// Parameter wird sicher übergeben
```

**FALSCH (SQL Injection!):**
```csharp
// NIE String-Konkatenation für SQL!
conditions.Add($"status = '{status}'");

// Angreifer könnte senden: ?status='; DROP TABLE tasks; --
// SQL würde: "SELECT * FROM tasks WHERE status = ''; DROP TABLE tasks; --'"
```

---

## Query-Parameter Validierung

Der `TaskQueryParametersValidator` prüft ob die Werte gültig sind:

```csharp
public class TaskQueryParametersValidator : AbstractValidator<TaskQueryParameters>
{
    public TaskQueryParametersValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage("Status must be pending, in_progress, or completed");

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .When(x => !string.IsNullOrEmpty(x.Priority))
            .WithMessage("Priority must be low, medium, or high");
    }
}
```

### Validierung verwenden

```csharp
var queryParams = new TaskQueryParameters
{
    Status = status,
    Priority = priority,
    Search = search
};

var validationResult = _queryValidator.Validate(queryParams);
if (!validationResult.IsValid)
{
    return FormatValidationErrors(validationResult);
    // Gibt 400 Bad Request mit Fehlerdetails zurück
}
```

---

## Häufige Fehler vermeiden

### 1. Null-Prüfung nicht vergessen

```csharp
// RICHTIG:
if (!string.IsNullOrEmpty(status))

// FALSCH:
if (status != null)  // ❌ Leerer String "" würde durchgehen!
```

### 2. Parameter-Typen beachten

```csharp
// RICHTIG:
parameters.Add(new SqliteParameter("@status", status));

// Alternativ:
cmd.Parameters.AddWithValue("@status", status);
```

### 3. WHERE nur wenn nötig

```csharp
// RICHTIG:
var sql = "SELECT * FROM tasks";
if (conditions.Count > 0)
{
    sql += " WHERE " + string.Join(" AND ", conditions);
}
// Ohne Filter: "SELECT * FROM tasks"
// Mit Filter: "SELECT * FROM tasks WHERE ..."

// FALSCH:
var sql = "SELECT * FROM tasks WHERE";  // ❌ Syntaxfehler wenn keine Filter!
```

### 4. LIKE-Pattern richtig erstellen

```csharp
// RICHTIG:
var searchPattern = "%" + search + "%";
parameters.Add(new SqliteParameter("@search", searchPattern));

// FALSCH:
parameters.Add(new SqliteParameter("@search", "%search%"));  // ❌ Sucht nach "search", nicht dem Wert!
```

---

## Test-Beispiele

### Alle Filter testen

```bash
# 1. Keine Filter
curl http://localhost:3003/api/tasks

# 2. Nur status
curl "http://localhost:3003/api/tasks?status=pending"

# 3. Nur priority
curl "http://localhost:3003/api/tasks?priority=high"

# 4. Kombiniert
curl "http://localhost:3003/api/tasks?status=pending&priority=high"

# 5. Suche
curl "http://localhost:3003/api/tasks?search=API"

# 6. Alles zusammen
curl "http://localhost:3003/api/tasks?status=pending&priority=high&search=test"

# 7. Ungültiger Parameter (sollte 400 geben)
curl "http://localhost:3003/api/tasks?status=invalid"
```

---

## Performance-Tipps

### Indexe für häufige Filter

Falls die Datenbank wächst, sollten Indexe hinzugefügt werden:

```sql
-- In DatabaseConfig.cs oder Migration
CREATE INDEX IF NOT EXISTS idx_tasks_status ON tasks(status);
CREATE INDEX IF NOT EXISTS idx_tasks_priority ON tasks(priority);
```

### Full-Text Search (für große Datenmengen)

Für komplexe Suchen bietet SQLite FTS (Full-Text Search):

```sql
-- Virtual Table für Full-Text Search
CREATE VIRTUAL TABLE tasks_fts USING fts5(title, description, content=tasks, content_rowid=id);
```

Für dieses Feature reicht jedoch `LIKE` aus.

---

## Nächste Schritte

Nach erfolgreicher Implementierung:
1. Alle Tests in `tests.http` ausführen
2. Kombinierte Filter testen
3. Edge Cases prüfen (ungültige Parameter, leere Suche)
4. Zu weiteren Features wechseln
