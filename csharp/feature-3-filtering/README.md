# Task Management API - Feature 3: Filtering (C#)

## Aufgaben

In diesem Feature erweiterst du die API um **Query-Parameter-Filterung** für den GET Endpunkt. CRUD-Operationen und Validierung aus den vorherigen Features sind bereits implementiert.

| Deine Aufgabe | Status |
|---------------|--------|
| GET `/api/tasks?status=` | **Selber lösen** |
| GET `/api/tasks?priority=` | **Selber lösen** |
| GET `/api/tasks?search=` | **Selber lösen** |
| Kombinierte Filter | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Filter nach Status implementieren

**Was du tun musst:**
1. Lies den `status` Query-Parameter aus `[FromQuery] string? status`
2. Validiere den Parameter mit `_queryValidator.Validate()`
3. Baue die SQL-Query dynamisch auf
4. Füge `WHERE status = @status` hinzu wenn status vorhanden

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `GetAllTasks`
- Suche nach: `// TODO AUFGABE: GET /api/tasks mit Query-Parameter-Filterung`

**Tipp:**
```csharp
// Query-Parameter verarbeiten
var queryParams = new TaskQueryParameters
{
    Status = status,
    Priority = priority,
    Search = search
};

// Validierung
var validationResult = _queryValidator.Validate(queryParams);
if (!validationResult.IsValid)
{
    return FormatValidationErrors(validationResult);
}

// SQL dynamisch aufbauen
var conditions = new List<string>();
var parameters = new List<SqliteParameter>();

if (!string.IsNullOrEmpty(status))
{
    conditions.Add("status = @status");
    parameters.Add(new SqliteParameter("@status", status));
}

var sql = "SELECT * FROM tasks";
if (conditions.Count > 0)
{
    sql += " WHERE " + string.Join(" AND ", conditions);
}
```

---

### Aufgabe 2: Filter nach Priority implementieren

**Was du tun musst:**
1. Erweitere die Filter-Logik um `?priority=`
2. Kombiniere mit Status-Filter (AND)
3. Beide Parameter sollen gleichzeitig funktionieren

**Wo du es findest:**
- Gleiche Stelle wie Aufgabe 1

**Tipp:**
```csharp
if (!string.IsNullOrEmpty(priority))
{
    conditions.Add("priority = @priority");
    parameters.Add(new SqliteParameter("@priority", priority));
}
```

---

### Aufgabe 3: Suche implementieren

**Was du tun musst:**
1. Implementiere `?search=` Parameter
2. Suche sollte in `title` UND `description` suchen
3. Verwende SQL `LIKE` mit `%` Wildcards
4. Case-insensitive Suche (SQLite ist per default case-insensitive)

**Wo du es findest:**
- Gleiche Stelle wie Aufgabe 1

**Tipp:**
```csharp
if (!string.IsNullOrEmpty(search))
{
    conditions.Add("(title LIKE @search OR description LIKE @search)");
    var searchPattern = "%" + search + "%";
    parameters.Add(new SqliteParameter("@search", searchPattern));
}
```

---

## Gegeben (bereits implementiert)

### Struktur
```
feature-3-filtering/
├── Controllers/
│   └── TasksController.cs          # Hier implementierst du die Filterung
├── Models/
│   └── Task.cs                     # + TaskQueryParameters DTO
├── Data/
│   └── DatabaseConfig.cs           # SQLite Setup
├── Validators/
│   └── TaskValidators.cs           # Validatoren (inkl. Query-Parameter)
├── Program.cs                      # DI-Setup
└── feature-3-filtering.csproj      # + FluentValidation
```

### Was bereits fertig ist:
- GET `/api/tasks` - Basis-Implementation (ohne Filter)
- GET `/api/tasks/{id}` - Einzelnen Task abrufen
- POST `/api/tasks` - Mit Validierung
- PUT `/api/tasks/{id}` - Mit Validierung
- DELETE `/api/tasks/{id}` - Mit Existenz-Prüfung
- SQLite-Datenbank mit 5 Seed-Tasks
- FluentValidation NuGet-Paket
- Alle Validatoren (Create, Update, Query Parameters)
- Dependency Injection Setup

### Zu implementieren

Erweitere die `GetAllTasks` Methode in `Controllers/TasksController.cs` um:
1. Query-Parameter-Validierung
2. Dynamische SQL-Query mit WHERE clauses
3. Kombinierte Filter (AND)
4. Suche mit LIKE

---

## Erwartetes Ergebnis

### Filter nach Status

**Request:**
```bash
GET http://localhost:3003/api/tasks?status=pending
```

**Response:**
```json
[
  {
    "id": "task-3",
    "title": "Write docs",
    "status": "pending",
    "priority": "medium"
  },
  {
    "id": "task-4", 
    "title": "Test API",
    "status": "pending",
    "priority": "low"
  },
  {
    "id": "task-5",
    "title": "Deploy to production",
    "status": "pending",
    "priority": "high"
  }
]
```

### Filter nach Priority

**Request:**
```bash
GET http://localhost:3003/api/tasks?priority=high
```

**Response:**
```json
[
  {
    "id": "task-1",
    "title": "Learn C#",
    "status": "completed",
    "priority": "high"
  },
  {
    "id": "task-2",
    "title": "Build REST API", 
    "status": "in_progress",
    "priority": "high"
  },
  {
    "id": "task-5",
    "title": "Deploy to production",
    "status": "pending",
    "priority": "high"
  }
]
```

### Kombinierte Filter

**Request:**
```bash
GET http://localhost:3003/api/tasks?status=pending&priority=high
```

**Response:**
```json
[
  {
    "id": "task-5",
    "title": "Deploy to production",
    "status": "pending",
    "priority": "high"
  }
]
```

### Suche

**Request:**
```bash
GET http://localhost:3003/api/tasks?search=API
```

**Response:**
```json
[
  {
    "id": "task-2",
    "title": "Build REST API",
    "description": "Create Task API",
    "status": "in_progress",
    "priority": "high"
  },
  {
    "id": "task-4",
    "title": "Test API",
    "description": "Test all endpoints",
    "status": "pending",
    "priority": "low"
  }
]
```

---

## Akzeptanzkriterien

- [ ] `?status=pending` filtert nach pending Tasks
- [ ] `?status=completed` filtert nach completed Tasks
- [ ] `?status=in_progress` filtert nach in_progress Tasks
- [ ] `?priority=high|medium|low` filtert nach Priority
- [ ] Kombinierte Filter funktionieren: `?status=X&priority=Y`
- [ ] `?search=keyword` sucht in title und description
- [ ] Suche ist case-insensitive
- [ ] Suche verwendet Wildcards (LIKE %keyword%)
- [ ] Kombination Filter + Suche funktioniert
- [ ] Ohne Parameter werden alle Tasks zurückgegeben
- [ ] Ungültige Query-Parameter geben 400 Bad Request zurück

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → csharp/projekt-1-task-api/feature-3-filtering`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist (`dotnet restore` läuft automatisch)

### Schritt 2: API starten

```bash
# Im VSCode Terminal (DevContainer):
docker compose up -d
```

**Prüfen ob es läuft:**
```bash
docker ps
# Oder:
curl http://localhost:3003/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

---

## Tests ausführen

### Option A: REST Client Extension (empfohlen)

1. **Datei öffnen:** `tests.http` oder `hint.http`
2. **Auf "Send Request" klicken** (erscheint über jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Option B: VSCode Command Palette

Falls die "Send Request" Links nicht angezeigt werden:

1. **Datei öffnen:** `tests.http`
2. **Cursor auf die Anfrage setzen**
3. **Command Palette öffnen:** `Ctrl+Shift+P`
4. **Tippen:** `Rest Client: Send Request`

### Option C: Mit curl im Terminal

```bash
# Alle Tasks:
curl http://localhost:3003/api/tasks

# Filter nach status:
curl "http://localhost:3003/api/tasks?status=pending"

# Filter nach priority:
curl "http://localhost:3003/api/tasks?priority=high"

# Kombinierte Filter:
curl "http://localhost:3003/api/tasks?status=in_progress&priority=high"

# Suche:
curl "http://localhost:3003/api/tasks?search=API"
```

> **Wichtig:** Bei URLs mit Query-Parametern immer Anführungszeichen verwenden!

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- .NET 8.0 SDK
- C# Dev Kit Extension für VSCode
- Docker & Docker Compose
- SQLite (Microsoft.Data.Sqlite)
- FluentValidation.AspNetCore
- REST Client Extension

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 3-5 | Filter nach Status | Query-Parameter + Filter implementieren |
| Test 6-8 | Filter nach Priority | ?priority= implementieren |
| Test 9-10 | Kombinierte Filter | AND-Verknüpfung |
| Test 11-12 | Suche | SQL LIKE implementieren |
| Test 13 | Kombination Suche + Filter | Alles zusammen |

### Tests ausführen

**Mit REST Client Extension:**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 3: Filter nach pending
curl "http://localhost:3003/api/tasks?status=pending"

# Test 6: Filter nach high priority
curl "http://localhost:3003/api/tasks?priority=high"

# Test 9: Kombinierte Filter
curl "http://localhost:3003/api/tasks?status=in_progress&priority=high"

# Test 11: Suche
curl "http://localhost:3003/api/tasks?search=API"
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Siehe dir `HINTS.md` an - dort findest du Code-Beispiele
- Vergleiche mit `Controllers/TasksController.cs` - dort ist ein Kommentar mit Hinweisen

---

## C# Spezifika

### Query-Parameter in ASP.NET Core

```csharp
// Auslesen der Query-Parameter
[HttpGet]
public IActionResult GetAllTasks(
    [FromQuery] string? status,
    [FromQuery] string? priority,
    [FromQuery] string? search)
{
    // Parameter sind null wenn nicht angegeben
    if (!string.IsNullOrEmpty(status))
    {
        // Filter anwenden
    }
}
```

### Dynamische SQL-Query

```csharp
var conditions = new List<string>();
var parameters = new List<SqliteParameter>();

if (!string.IsNullOrEmpty(status))
{
    conditions.Add("status = @status");
    parameters.Add(new SqliteParameter("@status", status));
}

var sql = "SELECT * FROM tasks";
if (conditions.Count > 0)
{
    sql += " WHERE " + string.Join(" AND ", conditions);
}

using var cmd = new SqliteCommand(sql, connection);
cmd.Parameters.AddRange(parameters.ToArray());
```

### SQL LIKE für Suche

```csharp
if (!string.IsNullOrEmpty(search))
{
    // Case-insensitive Suche in title UND description
    conditions.Add("(title LIKE @search OR description LIKE @search)");
    var searchPattern = "%" + search + "%";
    parameters.Add(new SqliteParameter("@search", searchPattern));
}
```

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **TasksController.cs öffnen** - Finde die GetAllTasks Methode
3. **Query-Parameter validieren** - Verwende `_queryValidator.Validate()`
4. **SQL-Query aufbauen** - Dynamisch WHERE clauses hinzufügen
5. **Filter implementieren:**
   - Zuerst `?status=` (einfach)
   - Dann `?priority=` (kombinieren mit AND)
   - Dann `?search=` (SQL LIKE)
6. **Testen** - Führe Tests mit REST Client oder curl aus
7. **Vergleichen** - Mit der Lösung in `hint.http` und `HINTS.md` vergleichen
8. **Weiter** - Wechsle zu weiteren Features

---

## C# Ressourcen

- [[C# Grundlagen]]
- [[ASP.NET Core Web API]]
- [[FluentValidation]]
- [[SQLite WHERE Clause]]
- [[REST API Standards]]

---

## Siehe auch

- [[../feature-2-validation/README|Feature 2: Validation]] - Voraussetzung
- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[HINTS.md|Code-Hinweise und Beispiele]]
