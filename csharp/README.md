# REST Web Services - C# Projektübersicht

## Überblick

Dieses Verzeichnis enthält eine vollständige REST API Schulungsreihe in **C# / .NET 8.0**. Die Features bauen aufeinander auf und führen die Teilnehmer von den Grundlagen bis zur vollständigen Implementierung mit Authentifizierung.

## Struktur

```
csharp/projekt-1-task-api/
├── feature-1-basics/           # Port 3001
├── feature-2-validation/       # Port 3002
├── feature-3-filtering/        # Port 3003
├── feature-4-pagination/       # Port 3004
├── feature-5-auth/             # Port 3005
└── feature-complete/           # Port 3006
```

## Features im Überblick

### Feature 1: Basics (Port 3001)
**Thema:** CRUD-Grundlagen

**Was die Teilnehmer lernen:**
- GET Endpunkte implementieren
- POST, PUT, DELETE hinzufügen
- SQLite Datenbankzugriff
- Guid-Generierung
- Status Codes (200, 201, 204, 404)

**Vorhanden:**
- GET `/api/tasks` - Alle Tasks
- GET `/api/tasks/{id}` - Einzelnen Task

**Zu implementieren:**
- POST `/api/tasks` - Task erstellen
- PUT `/api/tasks/{id}` - Task aktualisieren
- DELETE `/api/tasks/{id}` - Task löschen

**Technologien:**
- ASP.NET Core 8.0
- SQLite (Microsoft.Data.Sqlite)
- Swagger/OpenAPI

---

### Feature 2: Validation (Port 3002)
**Thema:** Request-Validierung

**Was die Teilnehmer lernen:**
- FluentValidation einsetzen
- Regeln definieren (NotEmpty, MaxLength, Enum)
- Validierungsfehler formatieren
- 400 Bad Request zurückgeben

**Validierungsregeln:**
```csharp
// POST
- Title: Pflicht, max 200 Zeichen
- Description: Pflicht
- Priority: Optional, nur low/medium/high

// PUT
- Alle Felder optional
- Status: nur pending/in_progress/completed
```

**Neu hinzugekommen:**
- `Validators/CreateTaskRequestValidator.cs`
- `Validators/UpdateTaskRequestValidator.cs`
- Validierung in Controller aufrufen

---

### Feature 3: Filtering (Port 3003)
**Thema:** Query-Parameter

**Was die Teilnehmer lernen:**
- Query-Parameter auslesen
- Dynamische SQL-Queries bauen
- WHERE-Bedingungen kombinieren (AND)
- SQL LIKE für Suche

**Filter-Parameter:**
```
GET /api/tasks?status=pending
GET /api/tasks?priority=high
GET /api/tasks?search=TypeScript
GET /api/tasks?status=pending&priority=high
```

**Neu hinzugekommen:**
- `TaskQueryParameters` DTO
- Query-Parameter Validator
- Dynamische WHERE-Clauses

---

### Feature 4: Pagination (Port 3004)
**Thema:** Pagination

**Was die Teilnehmer lernen:**
- Offset berechnen
- COUNT(*) für Gesamtanzahl
- LIMIT/OFFSET in SQLite
- Pagination-Metadaten berechnen

**Response-Format:**
```json
{
  "data": [...],
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

**Neu hinzugekommen:**
- `PagedResult<T>` Wrapper
- `PaginationInfo` Metadaten
- Offset-Berechnung: `(page - 1) * limit`

---

### Feature 5: Authentication (Port 3005)
**Thema:** Authentifizierung

**Was die Teilnehmer lernen:**
- Login-Endpunkt implementieren
- Middleware erstellen
- HttpContext.Items für User-Speicherung
- 401 vs 403 Status Codes
- User-Isolation

**Auth-Endpoints:**
```
POST /api/auth/login
Authorization: Bearer user-1
```

**Neu hinzugekommen:**
- `AuthController.cs`
- `AuthenticationMiddleware.cs`
- Users-Tabelle
- User-Filter in allen Queries

---

### Feature Complete (Port 3006)
**Thema:** Gesamtlösung

**Was enthalten ist:**
- Alle Features vollständig implementiert
- Saubere Architektur (MVC)
- Business-Logik in Services
- Zentrale Middleware
- Register + Login + Profile
- Saubere Validierung

**Architektur:**
```
Controllers/
  → AuthController, TasksController
Services/
  → AuthService, TaskService
Middleware/
  → AuthenticationMiddleware
```

## Schnellstart

### Einzelnes Feature starten

```bash
cd feature-X

# Mit DevContainer
# → VSCode: "In Container neu öffnen"

# Docker starten
docker compose up -d

# API testen
curl http://localhost:300X/api/tasks
```

### Alle Features gleichzeitig

Die Features können parallel laufen (verschiedene Ports):

```bash
# Terminal 1
cd feature-1-basics && docker compose up -d        # Port 3001

# Terminal 2
cd feature-2-validation && docker compose up -d    # Port 3002

# usw.
```

## Übergang zwischen Features

### Von Feature 1 zu Feature 2

```diff
# Neue Abhängigkeit
+ <PackageReference Include="FluentValidation.AspNetCore" />

# Neue Validatoren
+ Validators/CreateTaskRequestValidator.cs
+ Validators/UpdateTaskRequestValidator.cs

# Controller erweitern
public IActionResult CreateTask([FromBody] CreateTaskRequest request)
{
+   var validationResult = _createValidator.Validate(request);
+   if (!validationResult.IsValid)
+   {
+       return FormatValidationErrors(validationResult);
+   }
    // ...
}
```

### Von Feature 4 zu Feature 5

```diff
# Neue Struktur
+ Controllers/AuthController.cs
+ Middleware/AuthenticationMiddleware.cs

# Datenbank erweitern
+ Users-Tabelle
+ Tasks-Tabelle: +userId Spalte

# Alle Endpunkte geschützt
+ [Authorization Middleware]

# User-Filter in GET
conditions.Add("userId = @userId");
```

## Technologie-Stack

| Komponente | Technologie |
|------------|-------------|
| Framework | ASP.NET Core 8.0 |
| Datenbank | SQLite (Microsoft.Data.Sqlite) |
| Validierung | FluentValidation |
| API-Doku | Swagger/OpenAPI |
| Container | Docker + Docker Compose |
| Dev-Umgebung | VSCode DevContainer |

## Ports

| Feature | Port | Container-Name |
|---------|------|----------------|
| feature-1-basics | 3001 | task-api-basics-csharp |
| feature-2-validation | 3002 | task-api-validation-csharp |
| feature-3-filtering | 3003 | task-api-filtering-csharp |
| feature-4-pagination | 3004 | task-api-pagination-csharp |
| feature-5-auth | 3005 | task-api-auth-csharp |
| feature-complete | 3006 | task-api-complete-csharp |

## Test-Daten

### Feature 1-4
- 3-5 Beispiel-Tasks (ohne User-Verknüpfung)

### Feature 5 & Complete
**Users:**
- alice (user-1) / password123
- bob (user-2) / password456

**Tasks:**
- Alice: task-1, task-2, task-3
- Bob: task-4, task-5

## Unterschiede zu TypeScript

| Aspekt | TypeScript | C# |
|--------|-----------|-----|
| Struktur | Alles in index.ts | Getrennte Dateien/Ordner |
| Validierung | express-validator | FluentValidation |
| DI | Manuell | Built-in IoC Container |
| Middleware | Funktionen | Klassen mit InvokeAsync |
| Models | Interfaces | Klassen |
| SQLite | sqlite3 | Microsoft.Data.Sqlite |

## C# Spezifika

### Dependency Injection

```csharp
// Registrieren in Program.cs
builder.Services.AddScoped<AuthService>();

// Injizieren im Controller
public class TasksController : ControllerBase
{
    private readonly AuthService _authService;
    
    public TasksController(AuthService authService)
    {
        _authService = authService;
    }
}
```

### Middleware

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    
    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Vor dem Controller
        await _next(context);
        // Nach dem Controller
    }
}
```

### HttpContext.Items

```csharp
// Middleware setzt
context.Items["UserId"] = userId;

// Controller liest
var userId = HttpContext.Items["UserId"] as string;
```

## Lernpfad für Teilnehmer

### Empfohlene Reihenfolge

1. **Feature 1** (Basics)
   - GET verstehen
   - POST/PUT/DELETE implementieren
   - SQLite lernen

2. **Feature 2** (Validation)
   - FluentValidation einführen
   - Regeln definieren
   - 400 Bad Request

3. **Feature 3** (Filtering)
   - Query-Parameter
   - Dynamisches SQL
   - WHERE-Bedingungen

4. **Feature 4** (Pagination)
   - Offset berechnen
   - LIMIT/OFFSET
   - PagedResult

5. **Feature 5** (Auth)
   - Middleware verstehen
   - User-Isolation
   - 401 vs 403

6. **Feature Complete**
   - Architektur-Review
   - Best Practices
   - Erweiterungen planen

### Zeitaufwand (Schätzung)

| Feature | Zeit |
|---------|------|
| Feature 1 | 2-3 Stunden |
| Feature 2 | 1-2 Stunden |
| Feature 3 | 2 Stunden |
| Feature 4 | 2 Stunden |
| Feature 5 | 3-4 Stunden |
| **Gesamt** | **10-13 Stunden** |

## Erweiterungsmöglichkeiten

Nach dem Durchlaufen aller Features können Teilnehmer:

1. **JWT implementieren**
   - NuGet: System.IdentityModel.Tokens.Jwt
   - Token-Generierung mit Secret
   - Token-Validierung in Middleware

2. **Password Hashing**
   - NuGet: BCrypt.Net-Next
   - Passwörter nicht im Klartext speichern

3. **Refresh Tokens**
   - Tokens erneuern ohne neuen Login
   - Token-Blacklist

4. **Erweiterte Filter**
   - Datumsbereiche (createdAt)
   - Sortierung (sortBy, order)
   - Komplexe Suche (Full-Text)

5. **Ressourcen-Verknüpfung**
   - Kategorien für Tasks
   - Tags
   - Beziehungen zwischen Tasks

6. **Performance**
   - Caching (In-Memory oder Redis)
   - Database Indexe
   - Async/Await Optimierung

## Ressourcen

### Dokumentation
- [[C# Grundlagen]]
- [[ASP.NET Core Web API]]
- [[FluentValidation]]
- [[SQLite in .NET]]
- [[REST API Standards]]

### Externe Links
- [.NET 8 Dokumentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/)
- [FluentValidation Docs](https://docs.fluentvalidation.net/)

---

## Lizenz & Nutzung

Diese Schulungsmaterialien sind für den persönlichen und schulischen Gebrauch bestimmt.

## Support

Bei Fragen oder Problemen:
- README.md in jedem Feature-Ordner lesen
- HINTS.md für Code-Beispiele
- tests.http für Test-Szenarien
