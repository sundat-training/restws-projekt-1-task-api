# Task Management API - Feature 5: Authentication (C#)

## Aufgaben

In diesem Feature erweiterst du die API um **Authentifizierung**. Alle vorherigen Features (CRUD, Validierung, Filtering, Pagination) sind bereits implementiert - jetzt werden die Tasks Benutzer-spezifisch.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/auth/login` | **Selber lösen** |
| Auth-Middleware | **Selber lösen** |
| Tasks mit User verknüpfen | **Selber lösen** |
| User-Isolation | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Login Endpunkt implementieren

**Was du tun musst:**
1. Validiere den LoginRequest (Username und Password)
2. Suche den User in der Datenbank anhand des Username
3. Prüfe ob das Password übereinstimmt
4. Bei Erfolg: return 200 OK mit `{ userId, username }`
5. Bei Fehler: return 401 Unauthorized

**Wo du es findest:**
- Datei: `Controllers/AuthController.cs`
- Methode: `Login`
- Suche nach: `// TODO AUFGABE: POST /api/auth/login implementieren`

**Tipp:**
```csharp
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // Validierung
    var validationResult = _loginValidator.Validate(request);
    if (!validationResult.IsValid)
    {
        return FormatValidationErrors(validationResult);
    }

    // User suchen
    using var connection = _database.GetConnection();
    connection.Open();
    
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
        
        // Password prüfen (einfache Variante: Klartext)
        if (user.Password == request.Password)
        {
            return Ok(new LoginResponse 
            { 
                UserId = user.Id, 
                Username = user.Username 
            });
        }
    }
    
    return Unauthorized(new { error = "Invalid credentials" });
}
```

---

### Aufgabe 2: Auth-Middleware implementieren

**Was du tun musst:**
1. Lies den `Authorization` Header aus dem Request
2. Prüfe ob der Header vorhanden ist (format: "Bearer user-1")
3. Extrahiere die `userId` aus dem Token
4. Speichere die `userId` in `HttpContext.Items["UserId"]`
5. Bei fehlendem/ungültigem Auth: return 401 Unauthorized
6. Öffentliche Endpunkte (Login) sollen ohne Auth funktionieren

**Wo du es findest:**
- Datei: `Middleware/AuthenticationMiddleware.cs`
- Suche nach: `// TODO AUFGABE: Authentication Middleware implementieren`

**Tipp:**
```csharp
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
    
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Authentication required" });
        return;
    }

    // userId extrahieren (einfach: nach "Bearer " kommt die ID)
    var userId = authHeader.Substring("Bearer ".Length).Trim();
    
    // In HttpContext speichern für Controller
    context.Items["UserId"] = userId;
    
    await _next(context);
}

private bool IsPublicPath(string path)
{
    return path.StartsWith("/api/auth/");
}
```

**Middleware registrieren:**
```csharp
// In Program.cs:
app.UseAuthenticationMiddleware();
```

---

### Aufgabe 3: Tasks mit User verknüpfen

**Was du tun musst:**
1. In GET `/api/tasks`: Zeige nur Tasks wo `userId = currentUser`
2. In POST `/api/tasks`: Setze `userId` des eingeloggten Users
3. In GET `/api/tasks/{id}`: Prüfe ob Task dem User gehört

**Wo du es findest:**
- Datei: `Controllers/TasksController.cs`
- Methoden: `GetAllTasks`, `GetTask`, `CreateTask`

**Hilfsmethode:**
```csharp
private string? GetCurrentUserId()
{
    return HttpContext.Items["UserId"] as string;
}
```

**GET mit User-Filter:**
```csharp
[HttpGet]
public IActionResult GetAllTasks(...)
{
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized(new { error = "Authentication required" });
    }

    // userId als ersten Filter hinzufügen
    conditions.Add("userId = @userId");
    parameters.Add(new SqliteParameter("@userId", userId));
    
    // ... restliche Filter
}
```

**POST mit User-Zuordnung:**
```csharp
[HttpPost]
public IActionResult CreateTask([FromBody] CreateTaskRequest request)
{
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized(new { error = "Authentication required" });
    }

    // ... INSERT mit userId
    cmd.Parameters.AddWithValue("@userId", userId);
}
```

---

### Aufgabe 4: Autorisierung bei PUT/DELETE

**Was du tun musst:**
1. Prüfe ob Task existiert UND dem User gehört
2. Wenn nicht: return 403 Forbidden
3. Wenn ja: UPDATE/DELETE durchführen

**Tipp:**
```csharp
[HttpPut("{id}")]
public IActionResult UpdateTask(string id, [FromBody] UpdateTaskRequest request)
{
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized(new { error = "Authentication required" });
    }

    // Prüfe ob Task existiert und User gehört
    var checkSql = "SELECT COUNT(*) FROM tasks WHERE id = @id AND userId = @userId";
    using var checkCmd = new SqliteCommand(checkSql, connection);
    checkCmd.Parameters.AddWithValue("@id", id);
    checkCmd.Parameters.AddWithValue("@userId", userId);
    
    var count = (long)checkCmd.ExecuteScalar();
    if (count == 0)
    {
        // Prüfe ob Task überhaupt existiert (für 404 vs 403)
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

    // ... UPDATE durchführen
}
```

---

## Gegeben (bereits implementiert)

### Struktur
```
feature-5-auth/
├── Controllers/
│   ├── AuthController.cs           # Login Endpunkt (TODO)
│   └── TasksController.cs          # Mit Auth-Integration (TODOs)
├── Models/
│   └── Task.cs                     # + User, LoginRequest/Response
├── Data/
│   └── DatabaseConfig.cs           # + Users-Tabelle
├── Middleware/
│   └── AuthenticationMiddleware.cs # Auth Middleware (TODO)
├── Validators/
│   └── TaskValidators.cs           # + LoginRequestValidator
├── Program.cs                      # + Middleware-Registrierung (TODO)
└── feature-5-auth.csproj           # + FluentValidation
```

### Was bereits fertig ist:
- Datenbank mit Users- und Tasks-Tabelle
- 2 Beispiel-User: alice (user-1) und bob (user-2)
- 5 Tasks mit userId-Verknüpfung (3 für alice, 2 für bob)
- Alle CRUD-Endpunkte (aber ohne Auth-Prüfung)
- Validierung, Filtering, Pagination
- LoginRequest/Response Models
- AuthenticationMiddleware (Grundstruktur)

### Zu implementieren

- **AuthController.Login()** - User-Login
- **AuthenticationMiddleware** - Auth-Prüfung
- **TasksController.GetCurrentUserId()** - User aus Context lesen
- **User-Filter** in GET Endpunkten
- **User-Zuordnung** in POST
- **Autorisierung** in PUT/DELETE
- **Middleware-Registrierung** in Program.cs

---

## Erwartetes Ergebnis

### Login

**Request:**
```bash
POST http://localhost:3005/api/auth/login
Content-Type: application/json

{
  "username": "alice",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "userId": "user-1",
  "username": "alice"
}
```

### Ohne Auth

**Request:**
```bash
GET http://localhost:3005/api/tasks
```

**Response (401 Unauthorized):**
```json
{
  "error": "Authentication required"
}
```

### Mit Auth - Eigene Tasks

**Request:**
```bash
GET http://localhost:3005/api/tasks
Authorization: Bearer user-1
```

**Response:**
```json
{
  "data": [
    { "id": "task-1", "title": "Learn C#", "userId": "user-1" },
    { "id": "task-2", "title": "Build REST API", "userId": "user-1" },
    { "id": "task-3", "title": "Write docs", "userId": "user-1" }
  ],
  "pagination": { "totalItems": 3, ... }
}
```

### Fremden Task ansehen (verboten)

**Request:**
```bash
GET http://localhost:3005/api/tasks/task-4
Authorization: Bearer user-1
```

**Response (403 Forbidden):**
```json
{
  "error": "Not authorized to access this task"
}
```

---

## Akzeptanzkriterien

- [ ] POST `/api/auth/login` prüft username/password
- [ ] Login gibt bei Erfolg `{ userId, username }` zurück
- [ ] Login gibt bei Fehler 401 zurück
- [ ] Auth-Middleware prüft Authorization Header
- [ ] Ohne Auth wird 401 zurückgegeben
- [ ] GET `/api/tasks` zeigt nur Tasks des eingeloggten Users
- [ ] POST `/api/tasks` setzt automatisch userId des eingeloggten Users
- [ ] PUT `/api/tasks/{id}` prüft ob Task dem User gehört
- [ ] DELETE `/api/tasks/{id}` prüft ob Task dem User gehört
- [ ] Bei fremden Tasks wird 403 zurückgegeben
- [ ] Alice sieht nur ihre Tasks (task-1, 2, 3)
- [ ] Bob sieht nur seine Tasks (task-4, 5)

---

## Projekt starten (im DevContainer)

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → csharp/projekt-1-task-api/feature-5-auth`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist

### Schritt 2: API starten

```bash
docker compose up -d
```

**Prüfen ob es läuft:**
```bash
curl http://localhost:3005/api/auth/login -X POST -H "Content-Type: application/json" -d '{"username":"alice","password":"password123"}'
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

### Option B: Mit curl im Terminal

```bash
# Login:
curl -X POST http://localhost:3005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "password123"}'

# Mit Auth:
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-1"

# Als bob:
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-2"
```

---

## C# Spezifika

### HttpContext.Items

```csharp
// In Middleware setzen:
context.Items["UserId"] = userId;

// Im Controller lesen:
var userId = HttpContext.Items["UserId"] as string;
```

### Authorization Header

```csharp
// Header lesen
var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

// "Bearer " entfernen
var userId = authHeader?.Substring("Bearer ".Length).Trim();
```

### 401 vs 403

```csharp
// 401 Unauthorized - Kein Auth/ungültiges Auth
return Unauthorized(new { error = "Authentication required" });

// 403 Forbidden - Auth OK, aber keine Berechtigung
return StatusCode(403, new { error = "Not authorized to access this task" });
```

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **AuthController.Login()** implementieren
3. **AuthenticationMiddleware** implementieren
4. **Program.cs** - Middleware registrieren: `app.UseAuthenticationMiddleware()`
5. **TasksController** anpassen:
   - `GetCurrentUserId()` Methode
   - GET mit User-Filter
   - POST mit User-Zuordnung
   - PUT/DELETE mit Autorisierung
6. **Testen** - Mit REST Client oder curl
7. **Vergleichen** - Mit `hint.http` vergleichen

---

## C# Ressourcen

- [[C# Grundlagen]]
- [[ASP.NET Core Middleware]]
- [[REST API Authentication]]
- [[HTTP Status Codes]]

---

## Siehe auch

- [[../feature-4-pagination/README|Feature 4: Pagination]] - Voraussetzung
- [[HINTS.md|Code-Hinweise und Beispiele]]
- [[tests.http|Test-Szenarien]]
- [[hint.http|Lösungen]]
