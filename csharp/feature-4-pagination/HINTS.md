# Lösungshinweise - Feature 4: Pagination (C#)

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Pagination in `Controllers/TasksController.cs`.

---

## Aufgabe: GET /api/tasks mit Pagination

### Lösungsansatz

1. Query-Parameter auslesen und validieren
2. Default-Werte setzen (page=1, limit=10)
3. Randfälle behandeln (negative Werte, zu hohes limit)
4. Offset berechnen: `(page - 1) * limit`
5. WHERE-Bedingungen für Filter aufbauen
6. COUNT(*) Query ausführen für totalItems
7. SELECT Query mit LIMIT/OFFSET ausführen
8. Pagination-Metadaten berechnen
9. PagedResult<T> zurückgeben

### Vollständiges Code-Beispiel

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
        // 1. Query-Parameter in DTO packen
        var queryParams = new TaskQueryParameters
        {
            Status = status,
            Priority = priority,
            Search = search,
            Page = page,
            Limit = limit
        };

        // 2. Validierung
        var validationResult = _queryValidator.Validate(queryParams);
        if (!validationResult.IsValid)
        {
            return FormatValidationErrors(validationResult);
        }

        // 3. Default-Werte setzen
        int currentPage = page ?? 1;
        int currentLimit = limit ?? 10;

        // 4. Randfälle behandeln
        currentPage = Math.Max(1, currentPage);  // Minimum 1
        currentLimit = Math.Max(1, Math.Min(100, currentLimit));  // 1-100

        // 5. Offset berechnen
        int offset = (currentPage - 1) * currentLimit;

        using var connection = _database.GetConnection();
        connection.Open();

        // 6. WHERE-Bedingungen sammeln
        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

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

        // 7. COUNT(*) Query für totalItems
        var countSql = "SELECT COUNT(*) FROM tasks";
        if (conditions.Count > 0)
        {
            countSql += " WHERE " + string.Join(" AND ", conditions);
        }

        using var countCmd = new SqliteCommand(countSql, connection);
        countCmd.Parameters.AddRange(parameters.ToArray());
        var totalItems = (long)countCmd.ExecuteScalar();

        // 8. Seite korrigieren wenn außerhalb des Bereichs
        int totalPages = (int)Math.Ceiling(totalItems / (double)currentLimit);
        if (currentPage > totalPages && totalPages > 0)
        {
            currentPage = totalPages;
            offset = (currentPage - 1) * currentLimit;
        }

        // 9. SELECT Query mit LIMIT und OFFSET
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
                CreatedAt = reader.GetString(5),
                UpdatedAt = reader.GetString(6)
            });
        }

        // 10. Pagination-Metadaten berechnen
        bool hasNextPage = currentPage < totalPages;
        bool hasPreviousPage = currentPage > 1;

        // 11. PagedResult bauen
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
```

---

## Wichtige Konzepte

### Offset-Berechnung

```csharp
// Seite 1: offset = (1 - 1) * 5 = 0  -> Zeilen 1-5
// Seite 2: offset = (2 - 1) * 5 = 5  -> Zeilen 6-10
// Seite 3: offset = (3 - 1) * 5 = 10 -> Zeilen 11-15

int offset = (page - 1) * limit;
```

### Total Pages berechnen

```csharp
// Math.Ceiling rundet immer auf
// 15 Items / 5 Limit = 3.0 -> 3 Seiten
// 15 Items / 4 Limit = 3.75 -> 4 Seiten (aufrunden!)

int totalPages = (int)Math.Ceiling(totalItems / (double)currentLimit);
```

### Randfälle behandeln

```csharp
// Negative oder 0-Werte korrigieren
currentPage = Math.Max(1, currentPage);
currentLimit = Math.Max(1, currentLimit);

// Zu hohes Limit begrenzen
currentLimit = Math.Min(100, currentLimit);

// Oder alles zusammen:
currentLimit = Math.Max(1, Math.Min(100, currentLimit));

// Seite außerhalb des Bereichs korrigieren
if (currentPage > totalPages && totalPages > 0)
{
    currentPage = totalPages;
    offset = (currentPage - 1) * currentLimit;
}
```

### SQL LIMIT und OFFSET

```csharp
// SQLite Syntax:
// LIMIT = maximale Anzahl Zeilen
// OFFSET = wie viele Zeilen überspringen

// Beispiel: page=2, limit=5
// LIMIT 5 OFFSET 5 -> Zeilen 6-10

var sql = "SELECT * FROM tasks LIMIT @limit OFFSET @offset";
using var cmd = new SqliteCommand(sql, connection);
cmd.Parameters.AddWithValue("@limit", 5);
cmd.Parameters.AddWithValue("@offset", 5);
```

### COUNT mit WHERE

```csharp
// COUNT(*) zählt alle Zeilen (auch mit NULL-Werten)

var countSql = "SELECT COUNT(*) FROM tasks";
if (conditions.Count > 0)
{
    countSql += " WHERE " + string.Join(" AND ", conditions);
}

using var countCmd = new SqliteCommand(countSql, connection);
countCmd.Parameters.AddRange(parameters.ToArray());
var totalItems = (long)countCmd.ExecuteScalar();
```

---

## Pagination Response Format

```csharp
public class PagedResult<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public PaginationInfo Pagination { get; set; } = new PaginationInfo();
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

### JSON Output

```json
{
  "data": [...],
  "pagination": {
    "page": 2,
    "limit": 5,
    "totalItems": 15,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": true
  }
}
```

---

## Häufige Fehler vermeiden

### 1. Integer Division

```csharp
// FALSCH:
int totalPages = totalItems / limit;  // ❌ 15 / 10 = 1 (statt 2!)

// RICHTIG:
int totalPages = (int)Math.Ceiling(totalItems / (double)limit);  // ✓ 1.5 -> 2
```

### 2. Offset-Fehler

```csharp
// FALSCH:
int offset = page * limit;  // ❌ Seite 1 würde bei 5 beginnen

// RICHTIG:
int offset = (page - 1) * limit;  // ✓ Seite 1 beginnt bei 0
```

### 3. Parameter nicht doppelt hinzufügen

```csharp
// FALSCH:
// COUNT Query und SELECT Query haben beide die Parameter
// Aber sie werden nur einmal erstellt und wiederverwendet
// Bei zweiter Verwendung: Parameter müssen neu hinzugefügt werden!

// RICHTIG:
// Für SELECT Query Parameter neu erstellen oder kopieren
var selectParameters = parameters.ToList();  // Kopie erstellen
selectParameters.Add(new SqliteParameter("@limit", limit));
selectParameters.Add(new SqliteParameter("@offset", offset));
```

### 4. Connection Management

```csharp
// RICHTIG:
using var connection = _database.GetConnection();
connection.Open();
// Mehrere Queries in derselben Connection

// FALSCH:
using var connection1 = _database.GetConnection();  // ❌ Neue Connection
using var connection2 = _database.GetConnection();  // ❌ Noch eine
```

---

## Test-Beispiele

### Alle Pagination-Szenarien testen

```bash
# 1. Default Pagination
curl http://localhost:3004/api/tasks

# 2. Erste Seite mit limit=5
curl "http://localhost:3004/api/tasks?page=1&limit=5"

# 3. Zweite Seite
curl "http://localhost:3004/api/tasks?page=2&limit=5"

# 4. Letzte Seite
curl "http://localhost:3004/api/tasks?page=3&limit=5"

# 5. Randfälle
curl "http://localhost:3004/api/tasks?page=0&limit=5"      # Sollte Seite 1 sein
curl "http://localhost:3004/api/tasks?page=999&limit=5"    # Sollte leer sein
curl "http://localhost:3004/api/tasks?limit=1000"          # Sollte begrenzt sein

# 6. Filter + Pagination
curl "http://localhost:3004/api/tasks?status=pending&page=1&limit=3"
curl "http://localhost:3004/api/tasks?search=API&page=1&limit=2"
```

---

## Vergleich mit TypeScript

| TypeScript | C# |
|------------|-----|
| `parseInt(req.query.page)` | `page ?? 1` |
| `Math.ceil()` | `Math.Ceiling()` |
| `LIMIT ? OFFSET ?` | `LIMIT @limit OFFSET @offset` |
| `db.get('SELECT COUNT...')` | `countCmd.ExecuteScalar()` |
| `db.all('SELECT...')` | `cmd.ExecuteReader()` |

---

## Nächste Schritte

Nach erfolgreicher Implementierung:
1. Alle Tests in `tests.http` ausführen
2. Randfälle testen (page=0, page=999, limit=0)
3. Filter + Pagination kombiniert testen
4. Zu weiteren Features wechseln
