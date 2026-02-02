# Task Management API - Feature 2: Validation (C#)

## Aufgaben

In diesem Feature erweiterst du die API aus Feature 1 um **Request-Validierung** mit **FluentValidation**. Die CRUD-Endpunkte sind vorhanden, aber ohne Validierung.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/tasks` mit Validierung | **Selber lösen** |
| PUT `/api/tasks/{id}` mit Validierung | **Selber lösen** |
| DELETE `/api/tasks/{id}` mit Existenz-Prüfung | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: POST Validierung implementieren

**Was du tun musst:**
1. Führe die Validierung mit `_createValidator.Validate(request)` durch
2. Bei Fehlern: Gib `400 Bad Request` mit formatierten Fehlern zurück
3. Bei Erfolg: Erstelle den Task wie in Feature 1
4. Gebe den neuen Task mit `201 Created` zurück

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `CreateTask`
- Suche nach: `// TODO AUFGABE 1`

**Die Validatoren sind bereits erstellt:**
- `CreateTaskRequestValidator` prüft:
  - `title`: Pflichtfeld, max 200 Zeichen
  - `description`: Pflichtfeld
  - `priority`: Optional, nur `low`, `medium`, `high` erlaubt

**Tipp:**
```csharp
[HttpPost]
public IActionResult CreateTask([FromBody] CreateTaskRequest request)
{
    // 1. Validieren
    var validationResult = _createValidator.Validate(request);
    
    // 2. Bei Fehlern: 400 zurückgeben
    if (!validationResult.IsValid)
    {
        return FormatValidationErrors(validationResult);
    }
    
    // 3. Task erstellen (wie in Feature 1)
    // ...
    
    // 4. 201 Created zurückgeben
    return StatusCode(201, task);
}
```

---

### Aufgabe 2: PUT Validierung implementieren

**Was du tun musst:**
1. Prüfe ob der Task existiert (404 wenn nicht)
2. Führe die Validierung durch
3. Bei Fehlern: Gib `400 Bad Request` zurück
4. Bei Erfolg: Aktualisiere den Task

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `UpdateTask`
- Suche nach: `// TODO AUFGABE 2`

**Der Validator ist bereits erstellt:**
- `UpdateTaskRequestValidator` prüft:
  - `title`: Optional, max 200 Zeichen
  - `description`: Optional
  - `status`: Optional, nur `pending`, `in_progress`, `completed` erlaubt
  - `priority`: Optional, nur `low`, `medium`, `high` erlaubt

**Wichtig:** Bei PUT sind alle Felder optional - nur angegebene Felder werden aktualisiert.

---

### Aufgabe 3: DELETE mit Existenz-Prüfung implementieren

**Was du tun musst:**
1. Prüfe ob der Task existiert (404 wenn nicht)
2. Lösche den Task aus der Datenbank
3. Gebe `204 No Content` bei Erfolg zurück

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methode: `DeleteTask`
- Suche nach: `// TODO AUFGABE 3`

**Zusätzliche Validierung:**
- Prüfe vor dem Löschen mit `SELECT COUNT(*) FROM tasks WHERE id = @id`
- Gib 404 zurück wenn nicht gefunden

---

## Gegeben (bereits implementiert)

### Struktur
```
feature-2-validation/
├── Controllers/
│   └── TasksController.cs          # Hier implementierst du (mit TODOs)
├── Models/
│   └── Task.cs                     # Task Model + Request DTOs
├── Data/
│   └── DatabaseConfig.cs           # SQLite Setup
├── Validators/
│   ├── CreateTaskRequestValidator.cs   # POST Validierung (fertig)
│   └── UpdateTaskRequestValidator.cs   # PUT Validierung (fertig)
├── Program.cs                      # DI-Setup mit FluentValidation
└── feature-2-validation.csproj     # + FluentValidation.AspNetCore
```

### Was bereits fertig ist:
- GET `/api/tasks` - Alle Tasks abrufen
- GET `/api/tasks/{id}` - Einzelnen Task abrufen
- SQLite-Datenbank mit Seed-Daten
- FluentValidation NuGet-Paket
- Validatoren (`CreateTaskRequestValidator`, `UpdateTaskRequestValidator`)
- Dependency Injection Setup in `Program.cs`

### Zu implementieren

Erweitere `Controllers/TasksController.cs` um:
1. POST-Validierung aufrufen
2. PUT-Validierung aufrufen
3. DELETE mit Existenz-Prüfung

---

## Erwartetes Ergebnis

### POST mit Validierungsfehler

**Request:**
```bash
POST http://localhost:3002/api/tasks
Content-Type: application/json

{
  "description": "Missing title"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "field": "title",
      "message": "Title is required"
    }
  ]
}
```

### POST mit zu langem Titel

**Request:**
```bash
POST http://localhost:3002/api/tasks
Content-Type: application/json

{
  "title": "Dieser Titel ist viel zu lang und überschreitet die maximal erlaubten 200 Zeichen deutlich. Das ist nicht erlaubt und sollte einen Validierungsfehler auslösen...",
  "description": "Test"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "field": "title",
      "message": "Title must not exceed 200 characters"
    }
  ]
}
```

### POST mit ungültigem Priority

**Request:**
```bash
POST http://localhost:3002/api/tasks
Content-Type: application/json

{
  "title": "Valid Title",
  "description": "Valid description",
  "priority": "urgent"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "field": "priority",
      "message": "Priority must be low, medium, or high"
    }
  ]
}
```

### Validierung besteht

**Request:**
```bash
POST http://localhost:3002/api/tasks
Content-Type: application/json

{
  "title": "Valid Task",
  "description": "Valid description",
  "priority": "high"
}
```

**Response (201 Created):**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "title": "Valid Task",
  "description": "Valid description",
  "status": "pending",
  "priority": "high",
  "createdAt": "2024-01-15T10:30:00.000Z",
  "updatedAt": "2024-01-15T10:30:00.000Z"
}
```

---

## Akzeptanzkriterien

- [ ] POST `/api/tasks` validiert `title` (Pflicht, max 200)
- [ ] POST `/api/tasks` validiert `description` (Pflicht)
- [ ] POST `/api/tasks` validiert `priority` (optional, enum)
- [ ] PUT `/api/tasks/{id}` validiert alle Felder als optional
- [ ] Bei Validierungsfehlern wird `400 Bad Request` zurückgegeben
- [ ] Fehlerformat enthält `field` und `message`
- [ ] Bei fehlenden Pflichtfeldern wird spezifische Fehlermeldung angezeigt
- [ ] Bei zu langem Titel (>200 Zeichen) wird Fehler angezeigt
- [ ] Bei ungültigem Enum-Wert wird Fehler angezeigt
- [ ] DELETE prüft Existenz und gibt `404` wenn nicht gefunden
- [ ] DELETE gibt `204 No Content` bei Erfolg zurück

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → csharp/projekt-1-task-api/feature-2-validation`
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
curl http://localhost:3002/api/tasks
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
# Alle Tasks abrufen:
curl http://localhost:3002/api/tasks

# Task ohne title (Validierungsfehler):
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"description": "Missing title"}'

# Task mit zu langem title:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "'$(python3 -c "print('A'*201)")'", "description": "Test"}'

# Task mit ungültigem priority:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Valid", "description": "Valid", "priority": "urgent"}'

# Validen Task erstellen:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Valid Task", "description": "Valid description", "priority": "high"}'
```

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- .NET 8.0 SDK
- C# Dev Kit Extension für VSCode
- Docker & Docker Compose
- SQLite (Microsoft.Data.Sqlite)
- FluentValidation.AspNetCore (wird via NuGet installiert)
- REST Client Extension

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 3 | POST ohne title | Validierung + Test schreiben |
| Test 4 | POST ohne description | Validierung + Test schreiben |
| Test 5 | POST mit zu langem title | Längen-Validierung |
| Test 6 | POST mit ungültigem priority | Enum-Validierung |
| Test 7 | PUT mit ungültigem status | PUT Validierung |
| Test 8 | PUT mit zu langem title | PUT Längen-Validierung |
| Test 9 | DELETE nicht-existenter Task | Existenz-Prüfung |
| Test 10 | Valide Requests | Sollten weiterhin funktionieren |

### Tests ausführen

**Mit REST Client Extension:**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 3: POST ohne title
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"description": "Missing title"}'
# Erwartet: 400 Bad Request

# Test 9: DELETE nicht-existenter Task
curl -X DELETE http://localhost:3002/api/tasks/non-existent-id
# Erwartet: 404 Not Found
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Siehe dir `HINTS.md` an - dort findest du Code-Beispiele
- Vergleiche mit den GET-Methoden in `TasksController.cs`

---

## C# Spezifika

### FluentValidation vs express-validator

| express-validator (TypeScript) | FluentValidation (C#) |
|-------------------------------|----------------------|
| `body('title').notEmpty()` | `RuleFor(x => x.Title).NotEmpty()` |
| `validationResult(req)` | `validator.Validate(request)` |
| `errors.array()` | `validationResult.Errors` |
| Middleware-Pattern | DI + Explicit Validation |

### Wichtige Unterschiede zu TypeScript

| TypeScript | C# |
|------------|-----|
| `import { body } from 'express-validator'` | `using FluentValidation;` |
| Middleware in Route | Explicit validation in Controller |
| `req.body` | `[FromBody] RequestType request` |
| `res.status(400)` | `return BadRequest(...)` |

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **TasksController.cs öffnen** - Sieh dir die TODOs an
3. **Erstes TODO finden** - Suche nach `// TODO AUFGABE 1`
4. **HINTS.md lesen** - Dort findest du Code-Beispiele für C#
5. **POST Validierung implementieren** - `_createValidator.Validate()` aufrufen
6. **PUT Validierung implementieren** - `_updateValidator.Validate()` aufrufen
7. **DELETE implementieren** - Mit Existenz-Prüfung
8. **Testen** - Führe Tests mit REST Client Extension oder curl aus
9. **Vergleichen** - Mit der Lösung in `hint.http` und `HINTS.md` vergleichen
10. **Weiter** - Wechsle zu weiteren Features

---

## C# Ressourcen

- [[C# Grundlagen]]
- [[ASP.NET Core Web API]]
- [[FluentValidation]]
- [[REST API Standards]]

---

## Siehe auch

- [[../feature-1-basics/README|Feature 1: Basics]] - Voraussetzung
- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[HINTS.md|Code-Hinweise und Beispiele]]
