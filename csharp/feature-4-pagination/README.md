# Task Management API - Feature 4: Pagination (C#)

## Aufgaben

In diesem Feature erweiterst du die API um **Pagination**. CRUD-Operationen, Validierung und Filterung aus den vorherigen Features sind bereits implementiert.

| Deine Aufgabe | Status |
|---------------|--------|
| GET `/api/tasks?page=&limit=` | **Selber lösen** |
| Seitennavigation (next/previous) | **Selber lösen** |
| Pagination mit Filter kombinieren | **Selber lösen** |
| Randfälle behandeln | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Pagination implementieren

**Was du tun musst:**
1. Lies `page` und `limit` aus Query-Parametern aus
2. Setze Default-Werte: `page=1`, `limit=10`
3. Berechne `offset = (page - 1) * limit`
4. Führe `COUNT(*)` Query aus für Gesamtanzahl
5. Führe `SELECT` Query aus mit `LIMIT @limit OFFSET @offset`
6. Berechne Pagination-Metadaten:
   - `totalPages = ceil(totalItems / limit)`
   - `hasNextPage = page < totalPages`
   - `hasPreviousPage = page > 1`
7. Baue `PagedResult<T>` Response

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `GetAllTasks`
- Suche nach: `// TODO AUFGABE: GET /api/tasks mit Pagination`

**Erwartete Response-Struktur:**
```json
{
  "data": [...],  // Die Tasks auf dieser Seite
  "pagination": {
    "page": 1,
    "limit": 10,
    "totalItems": 15,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

**Tipp:**
```csharp
// 1. Default-Werte setzen
int currentPage = page ?? 1;
int currentLimit = limit ?? 10;

// 2. Randfälle behandeln
currentPage = Math.Max(1, currentPage);
currentLimit = Math.Max(1, Math.Min(100, currentLimit)); // Max 100

// 3. Offset berechnen
int offset = (currentPage - 1) * currentLimit;

// 4. COUNT Query
var countSql = "SELECT COUNT(*) FROM tasks";
// ... WHERE Bedingungen hinzufügen ...
var totalItems = (long)countCmd.ExecuteScalar();

// 5. SELECT Query mit LIMIT/OFFSET
var sql = "SELECT * FROM tasks LIMIT @limit OFFSET @offset";

// 6. Pagination berechnen
int totalPages = (int)Math.Ceiling(totalItems / (double)currentLimit);
bool hasNextPage = currentPage < totalPages;
bool hasPreviousPage = currentPage > 1;

// 7. Response bauen
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
```

---

### Aufgabe 2: Randfälle behandeln

**Was du tun musst:**
1. `page=0` oder negative → als `page=1` behandeln
2. `limit=0` oder negative → Standard-Limit verwenden
3. `limit` > 100 → auf 100 begrenzen
4. `page` > `totalPages` → leere `data` oder letzte Seite

**Tipp:**
```csharp
// Validierung
if (currentPage < 1) currentPage = 1;
if (currentLimit < 1) currentLimit = 10;
if (currentLimit > 100) currentLimit = 100;

// Seite außerhalb des Bereichs
if (currentPage > totalPages && totalPages > 0)
{
    currentPage = totalPages;
    offset = (currentPage - 1) * currentLimit;
}
```

---

### Aufgabe 3: Pagination mit Filter kombinieren

**Was du tun musst:**
1. Filter (status, priority, search) anwenden
2. `COUNT(*)` zählt nur gefilterte Tasks
3. `LIMIT/OFFSET` auf gefilterte Ergebnisse anwenden

**Tipp:**
```csharp
// WHERE-Bedingungen sammeln
var conditions = new List<string>();
var parameters = new List<SqliteParameter>();

// Filter hinzufügen...
if (!string.IsNullOrEmpty(status))
{
    conditions.Add("status = @status");
    parameters.Add(new SqliteParameter("@status", status));
}

// COUNT Query mit WHERE
var countSql = "SELECT COUNT(*) FROM tasks";
if (conditions.Count > 0)
{
    countSql += " WHERE " + string.Join(" AND ", conditions);
}

// SELECT Query mit WHERE + LIMIT + OFFSET
var sql = "SELECT * FROM tasks";
if (conditions.Count > 0)
{
    sql += " WHERE " + string.Join(" AND ", conditions);
}
sql += " LIMIT @limit OFFSET @offset";
parameters.Add(new SqliteParameter("@limit", currentLimit));
parameters.Add(new SqliteParameter("@offset", offset));
```

---

## Gegeben (bereits implementiert)

### Struktur
```
feature-4-pagination/
├── Controllers/
│   └── TasksController.cs          # Hier implementierst du Pagination
├── Models/
│   └── Task.cs                     # + PagedResult<T>, PaginationInfo
├── Data/
│   └── DatabaseConfig.cs           # SQLite mit 15 Tasks
├── Validators/
│   └── TaskValidators.cs           # + Pagination Validation
├── Program.cs                      # DI-Setup
└── feature-4-pagination.csproj     # + FluentValidation
```

### Was bereits fertig ist:
- GET `/api/tasks/{id}` - Einzelnen Task
- POST, PUT, DELETE - Mit Validierung
- Filterung (status, priority, search) - Im Controller vorhanden
- SQLite-Datenbank mit **15 Seed-Tasks** (für Pagination)
- `PagedResult<T>` und `PaginationInfo` Models
- Pagination Parameter Validation
- Alle Validatoren

### Zu implementieren

Erweitere `GetAllTasks` Methode um:
1. Pagination-Parameter verarbeiten
2. COUNT Query für totalItems
3. SELECT mit LIMIT/OFFSET
4. Pagination-Metadaten berechnen
5. PagedResult<T> zurückgeben

---

## Erwartetes Ergebnis

### Pagination Response

**Request:**
```bash
GET http://localhost:3004/api/tasks?page=1&limit=5
```

**Response:**
```json
{
  "data": [
    {
      "id": "task-1",
      "title": "Learn C# basics",
      "description": "Complete C# basics course",
      "status": "completed",
      "priority": "high"
    },
    {
      "id": "task-2",
      "title": "Build REST API",
      "description": "Create Task API with ASP.NET Core",
      "status": "in_progress",
      "priority": "high"
    },
    {
      "id": "task-3",
      "title": "Write documentation",
      "description": "Document all API endpoints",
      "status": "pending",
      "priority": "medium"
    },
    {
      "id": "task-4",
      "title": "Write unit tests",
      "description": "Implement xUnit tests for API",
      "status": "pending",
      "priority": "low"
    },
    {
      "id": "task-5",
      "title": "Deploy to production",
      "description": "Deploy API to cloud server",
      "status": "in_progress",
      "priority": "medium"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 5,
    "totalItems": 15,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Zweite Seite

**Request:**
```bash
GET http://localhost:3004/api/tasks?page=2&limit=5
```

**Response:**
```json
{
  "data": [
    { "id": "task-6", "title": "Setup CI/CD pipeline", ... },
    { "id": "task-7", "title": "Add authentication", ... },
    { "id": "task-8", "title": "Configure logging", ... },
    { "id": "task-9", "title": "Implement caching", ... },
    { "id": "task-10", "title": "Optimize database", ... }
  ],
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

### Kombinierte Filter + Pagination

**Request:**
```bash
GET http://localhost:3004/api/tasks?status=pending&page=1&limit=3
```

**Response:**
```json
{
  "data": [
    { "id": "task-3", "title": "Write documentation", "status": "pending" },
    { "id": "task-4", "title": "Write unit tests", "status": "pending" },
    { "id": "task-6", "title": "Setup CI/CD pipeline", "status": "pending" }
  ],
  "pagination": {
    "page": 1,
    "limit": 3,
    "totalItems": 7,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

## Akzeptanzkriterien

- [ ] `?page=1&limit=5` gibt 5 Tasks zurück
- [ ] `?page=2&limit=5` gibt Tasks 6-10 zurück (offset funktioniert)
- [ ] `?page=3&limit=5` gibt die letzten 5 Tasks zurück
- [ ] `pagination.totalItems` zeigt korrekte Gesamtzahl
- [ ] `pagination.totalPages` berechnet korrekte Seitenzahl
- [ ] `hasNextPage` ist true wenn weitere Seiten existieren
- [ ] `hasPreviousPage` ist true wenn nicht auf Seite 1
- [ ] `page=0` oder negative Werte werden als `page=1` behandelt
- [ ] Pagination funktioniert mit `?status=` Filter
- [ ] Pagination funktioniert mit `?search=` Suche
- [ ] Ohne Parameter wird mit Default-Werten paginiert (page=1, limit=10)

---

## Projekt starten (im DevContainer)

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → csharp/projekt-1-task-api/feature-4-pagination`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist

### Schritt 2: API starten

```bash
docker compose up -d
```

**Prüfen ob es läuft:**
```bash
curl http://localhost:3004/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

---

## Tests ausführen

### Option A: REST Client Extension (empfohlen)

1. **Datei öffnen:** `tests.http` oder `hint.http`
2. **Auf "Send Request" klicken**
3. **Response wird angezeigt**

### Option B: Mit curl im Terminal

```bash
# Seite 1 mit 5 Einträgen
curl "http://localhost:3004/api/tasks?page=1&limit=5"

# Seite 2
curl "http://localhost:3004/api/tasks?page=2&limit=5"

# Mit Filter und Pagination
curl "http://localhost:3004/api/tasks?status=pending&page=1&limit=3"
```

> **Wichtig:** URLs mit Query-Parametern in Anführungszeichen!

---

## C# Spezifika

### Pagination Berechnung

```csharp
// Offset berechnen
int offset = (page - 1) * limit;

// Total Pages berechnen (mit Ceiling für Aufrundung)
int totalPages = (int)Math.Ceiling(totalItems / (double)limit);

// Navigation Flags
bool hasNextPage = page < totalPages;
bool hasPreviousPage = page > 1;
```

### SQL LIMIT und OFFSET

```csharp
// SQLite Syntax
var sql = "SELECT * FROM tasks LIMIT @limit OFFSET @offset";

using var cmd = new SqliteCommand(sql, connection);
cmd.Parameters.AddWithValue("@limit", limit);
cmd.Parameters.AddWithValue("@offset", offset);
```

### PagedResult<T> verwenden

```csharp
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
```

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **TasksController.cs öffnen** - Finde GetAllTasks Methode
3. **Default-Werte setzen** - `page ?? 1`, `limit ?? 10`
4. **Randfälle behandeln** - Math.Max, Math.Min
5. **Offset berechnen** - `(page - 1) * limit`
6. **WHERE-Bedingungen** - Filter von Feature 3 übernehmen
7. **COUNT Query** - Für totalItems
8. **SELECT mit LIMIT/OFFSET** - Für die Daten
9. **Pagination berechnen** - totalPages, hasNext/Previous
10. **PagedResult bauen** - Response zurückgeben
11. **Testen** - Mit REST Client oder curl
12. **Vergleichen** - Mit `hint.http` vergleichen

---

## C# Ressourcen

- [[C# Grundlagen]]
- [[ASP.NET Core Web API]]
- [[FluentValidation]]
- [[SQLite LIMIT OFFSET]]
- [[REST API Standards]]

---

## Siehe auch

- [[../feature-3-filtering/README|Feature 3: Filtering]] - Voraussetzung
- [[HINTS.md|Code-Hinweise und Beispiele]]
- [[tests.http|Test-Szenarien]]
- [[hint.http|Lösungen]]
