# Task Management API - Feature 1: Basics (C#)

## Aufgaben

In dieser Aufgabe implementierst du **3 Endpunkte** selber in C#. Die GET-Endpunkte sind bereits fertig.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/tasks` | **Selber lösen** |
| PUT `/api/tasks/{id}` | **Selber lösen** |
| DELETE `/api/tasks/{id}` | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: POST /api/tasks implementieren

**Was du tun musst:**
1. Erstelle eine neue Guid mit `Guid.NewGuid().ToString()`
2. Extrahiere `title` und `description` aus dem Request (`CreateTaskRequest`)
3. Setze default `status = "pending"`
4. Setze default `priority = "medium"` (wenn nicht angegeben)
5. Füge Task in SQLite-Datenbank ein (INSERT)
6. Gebe den neuen Task mit Status `201 Created` zurück

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `CreateTask`
- Suche nach: `// TODO AUFGABE 1`

**Tipp:**
```csharp
// So generierst du eine Guid
var id = Guid.NewGuid().ToString();

// So fügst du in die DB ein
var sql = "INSERT INTO tasks (id, title, ...) VALUES (@id, @title, ...)";
using var cmd = new SqliteCommand(sql, connection);
cmd.Parameters.AddWithValue("@id", id);
// ... weitere Parameter
cmd.ExecuteNonQuery();
```

---

### Aufgabe 2: PUT /api/tasks/{id} implementieren

**Was du tun musst:**
1. Hole `id` aus dem Route-Parameter
2. Nimm die zu aktualisierenden Felder aus `UpdateTaskRequest`
3. Baue dynamisch das UPDATE-Statement (nur übergebene Felder)
4. Setze `updatedAt = CURRENT_TIMESTAMP`
5. Prüfe ob Task existiert (404 wenn nicht)
6. Gebe den aktualisierten Task zurück

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `UpdateTask`
- Suche nach: `// TODO AUFGABE 2`

**Tipp:**
```csharp
// Dynamisch Felder sammeln
var updates = new List<string>();
var parameters = new List<SqliteParameter>();

if (request.Title != null) {
    updates.Add("title = @title");
    parameters.Add(new SqliteParameter("@title", request.Title));
}
// ... weitere Felder
```

---

### Aufgabe 3: DELETE /api/tasks/{id} implementieren

**Was du tun musst:**
1. Hole `id` aus dem Route-Parameter
2. Lösche den Task aus der Datenbank (DELETE)
3. Prüfe ob ein Task gelöscht wurde (ExecuteNonQuery gibt Anzahl zurück)
4. Gebe `204 No Content` zurück
5. Oder `404` wenn nicht gefunden

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `DeleteTask`
- Suche nach: `// TODO AUFGABE 3`

**Tipp:**
```csharp
var sql = "DELETE FROM tasks WHERE id = @id";
using var cmd = new SqliteCommand(sql, connection);
cmd.Parameters.AddWithValue("@id", id);

var rowsAffected = cmd.ExecuteNonQuery();
if (rowsAffected == 0) {
    return NotFound(new { error = "Task not found" });
}
return NoContent();
```

---

## Gegeben (bereits implementiert)

- GET `/api/tasks` - Alle Tasks abrufen (bereits fertig)
- GET `/api/tasks/{id}` - Einzelnen Task abrufen (bereits fertig)
- SQLite Datenbank-Verbindung in `Data/DatabaseConfig.cs`
- Task Model in `Models/Task.cs`
- Controller-Setup in `Controllers/TasksController.cs`

### Zu implementieren

Erweitere `Controllers/TasksController.cs` um die fehlenden Endpunkt-Methoden.

---

## Projektstruktur (C#)

```
feature-1-basics/
├── Controllers/
│   └── TasksController.cs    # Hier implementierst du POST, PUT, DELETE
├── Models/
│   └── Task.cs               # Task Model + Request DTOs
├── Data/
│   └── DatabaseConfig.cs     # SQLite Setup (fertig)
├── Program.cs                # App-Setup (fertig)
├── feature-1-basics.csproj   # Projekt-Datei
├── tests.http                # Tests (ca. 1/3 als TODO)
├── hint.http                 # Vollständige Lösungen
├── HINTS.md                  # Code-Hinweise (diese Datei!)
└── README.md                 # Diese Dokumentation
```

**Wichtig:** Im Gegensatz zu TypeScript liegen bei C# die Dateien in Ordnern (`Controllers/`, `Models/`, `Data/`).

---

## C# Spezifika

### Wichtige Unterschiede zu TypeScript

| TypeScript | C# |
|------------|-----|
| `const id = uuidv4()` | `var id = Guid.NewGuid().ToString()` |
| `import sqlite3` | `using Microsoft.Data.Sqlite` |
| `app.post()` | `[HttpPost]` Attribute |
| `req.body` | `[FromBody] RequestType request` |
| `res.status(201)` | `StatusCode(201, object)` |
| `async/await` | `async Task<IActionResult>` |

### Status Codes in ASP.NET Core

```csharp
return Ok(task);                    // 200
return StatusCode(201, task);       // 201 Created
return NoContent();                 // 204
return BadRequest(new { ... });     // 400
return NotFound(new { ... });       // 404
return StatusCode(500, new { ... }); // 500
```

---

## Erwartetes Ergebnis

### POST /api/tasks

**Request:**
```bash
POST http://localhost:3001/api/tasks
Content-Type: application/json

{
  "title": "Neuer Task",
  "description": "Beschreibung des Tasks",
  "priority": "high"
}
```

**Response (201 Created):**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "title": "Neuer Task",
  "description": "Beschreibung des Tasks",
  "status": "pending",
  "priority": "high",
  "createdAt": "2024-01-15T10:30:00.000Z",
  "updatedAt": "2024-01-15T10:30:00.000Z"
}
```

### PUT /api/tasks/{id}

**Request:**
```bash
PUT http://localhost:3001/api/tasks/task-1
Content-Type: application/json

{
  "status": "completed"
}
```

**Response (200 OK):**
```json
{
  "id": "task-1",
  "title": "Learn C#",
  "description": "Complete C# basics",
  "status": "completed",
  "priority": "high",
  "createdAt": "2024-01-15T10:00:00.000Z",
  "updatedAt": "2024-01-15T10:30:00.000Z"
}
```

### DELETE /api/tasks/{id}

**Response (204 No Content):** (leerer Body)

---

## Akzeptanzkriterien

- [ ] POST erstellt neuen Task mit `Guid.NewGuid()` als ID
- [ ] POST setzt default Status auf "pending"
- [ ] POST setzt default Priority auf "medium"
- [ ] POST gibt 201 Created zurück
- [ ] PUT aktualisiert nur übergebene Felder
- [ ] PUT aktualisiert `updatedAt` Timestamp
- [ ] DELETE gibt 204 No Content zurück
- [ ] Alle Endpunkte geben bei nicht-existentem Task 404 zurück
- [ ] Alle Endpunkte geben bei Datenbankfehler 500 zurück
- [ ] Verwendung von Parameterized Queries (@param) gegen SQL Injection

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → csharp/projekt-1-task-api/feature-1-basics`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist (`dotnet restore` läuft automatisch)

### Schritt 2: API starten

**Wichtiger Hinweis - Permission-Problem beheben:**

Falls beim ersten Start Permission-Fehler auftreten (z.B. "Access denied" für obj/ oder bin/ Ordner):

```bash
# Im VSCode Terminal (DevContainer) - einmalig ausführen:
sudo rm -rf obj bin && dotnet restore && dotnet run --urls http://0.0.0.0:3000
```

**Danach normal starten:**

```bash
# Im VSCode Terminal (DevContainer):
docker compose up -d
```

**Prüfen ob es läuft:**
```bash
# Container Status
docker ps

# Oder direkt testen:
curl http://localhost:3001/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

### Alternative: Ohne Docker (direkt in DevContainer)

```bash
# Direkt im Terminal im DevContainer
dotnet run --urls http://0.0.0.0:3001
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
# Alle Tasks abrufen:
curl http://localhost:3001/api/tasks

# Einzelnen Task abrufen:
curl http://localhost:3001/api/tasks/task-1

# Task erstellen:
curl -X POST http://localhost:3001/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Neuer Task", "description": "Test", "priority": "high"}'

# Task aktualisieren:
curl -X PUT http://localhost:3001/api/tasks/task-1 \
  -H "Content-Type: application/json" \
  -d '{"status": "completed"}'

# Task löschen:
curl -X DELETE http://localhost:3001/api/tasks/task-3
```

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- .NET 8.0 SDK
- C# Dev Kit Extension für VSCode
- Docker & Docker Compose
- SQLite (Microsoft.Data.Sqlite)
- REST Client Extension

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 3 | POST neuen Task | Implementiere CreateTask |
| Test 4 | PUT Status update | Implementiere UpdateTask |
| Test 5 | PUT mehrere Felder | Erweitere UpdateTask |
| Test 6 | DELETE Task | Implementiere DeleteTask |
| Test 7-11 | Fehlerbehandlung | Sollte funktionieren |

### Tests ausführen

**Mit REST Client Extension:**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 3: POST
curl -X POST http://localhost:3001/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Test","description":"Beschreibung","priority":"high"}'

# Test 4: PUT
curl -X PUT http://localhost:3001/api/tasks/task-1 \
  -H "Content-Type: application/json" \
  -d '{"status":"completed"}'

# Test 6: DELETE
curl -X DELETE http://localhost:3001/api/tasks/task-3
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Sieh dir `HINTS.md` an - dort findest du Code-Beispiele
- Vergleiche mit `Controllers/TasksController.cs` - dort sind Referenz-Implementierungen (GET)

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **TasksController.cs öffnen** - Sieh dir die bereits implementierten GET-Methoden an
3. **Erstes TODO finden** - Suche nach `// TODO AUFGABE 1`
4. **HINTS.md lesen** - Dort findest du Code-Beispiele für C#
5. **Selber lösen** - Implementiere POST, PUT, DELETE
6. **Testen** - Führe Tests mit REST Client Extension oder curl aus
7. **Vergleichen** - Mit der Lösung in `hint.http` und `HINTS.md` vergleichen
8. **Weiter** - Wechsle zu `feature-2-validation` für Validierung

---

## C# Ressourcen

- [[C# Grundlagen]]
- [[ASP.NET Core Web API]]
- [[SQLite in .NET]]
- [[REST API Standards]]

---

## Nächste Schritte

Nach diesem Feature:
- [[../feature-2-validation/README|Feature 2: Validation]] - Request-Validierung hinzufügen

---

## Siehe auch

- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[HINTS.md|Code-Hinweise und Beispiele]]
- [[REST API Standards]]
