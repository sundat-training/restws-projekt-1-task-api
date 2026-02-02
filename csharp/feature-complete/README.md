# Task Management API - Feature Complete (C#)

## Übersicht

Dieses Feature enthält die **Gesamtlösung** mit allen Funktionalitäten:
- CRUD-Operationen für Tasks
- Request-Validierung
- Query-Filtering (status, priority, search)
- Pagination
- Authentifizierung mit User-Isolation
- Saubere Architektur (Services, Controllers, Middleware)

## Architektur

```
feature-complete/
├── Controllers/
│   ├── AuthController.cs      # Login, Register, Profile
│   └── TasksController.cs     # CRUD für Tasks
├── Services/
│   ├── AuthService.cs         # Authentifizierungs-Logik
│   └── TaskService.cs         # Business-Logik für Tasks
├── Middleware/
│   └── AuthenticationMiddleware.cs  # JWT-Token-Validierung
├── Models/
│   └── Task.cs                # Alle DTOs und Models
├── Validators/
│   └── TaskValidators.cs      # FluentValidation-Regeln
├── Data/
│   └── DatabaseConfig.cs      # SQLite-Datenbank
└── Program.cs                 # DI-Configuration
```

## Features

### Authentication

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| POST | `/api/auth/register` | Benutzer registrieren |
| POST | `/api/auth/login` | Benutzer anmelden |
| GET | `/api/auth/profile` | Profil abrufen |

### Tasks

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| GET | `/api/tasks` | Alle Tasks (mit Filter & Pagination) |
| GET | `/api/tasks/{id}` | Einzelnen Task abrufen |
| POST | `/api/tasks` | Task erstellen |
| PUT | `/api/tasks/{id}` | Task aktualisieren |
| DELETE | `/api/tasks/{id}` | Task löschen |

## Query-Parameter

### Filtering
```
?status=pending|in_progress|completed
?priority=low|medium|high
?search=Suchbegriff
```

### Pagination
```
?page=1
?limit=10
```

### Kombiniert
```
?status=pending&priority=high&page=1&limit=5
```

## Projekt starten

### Mit DevContainer (empfohlen)

1. VSCode starten
2. `File → Open Folder → csharp/projekt-1-task-api/feature-complete`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist

```bash
# API starten
docker compose up -d

# Testen
curl http://localhost:3006/api/auth/login \
  -X POST \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"password123"}'
```

### Ohne DevContainer

**Voraussetzungen:**
- .NET 8.0 SDK
- Docker (optional)

```bash
# Abhängigkeiten installieren
dotnet restore

# Starten
dotnet run --urls http://localhost:3006
```

## API Beispiele

### 1. Registrieren

```bash
curl -X POST http://localhost:3006/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"neuerUser","password":"password123"}'
```

**Response:**
```json
{
  "userId": "...",
  "username": "neuerUser",
  "token": "..."
}
```

### 2. Login

```bash
curl -X POST http://localhost:3006/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"password123"}'
```

**Response:**
```json
{
  "userId": "user-1",
  "username": "alice",
  "token": "eyJ..."
}
```

### 3. Tasks abrufen (mit Auth)

```bash
curl http://localhost:3006/api/tasks \
  -H "Authorization: Bearer user-1"
```

**Response:**
```json
{
  "data": [
    {
      "id": "task-1",
      "title": "Learn C#",
      "status": "completed",
      "priority": "high"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "totalItems": 3,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

### 4. Task erstellen

```bash
curl -X POST http://localhost:3006/api/tasks \
  -H "Authorization: Bearer user-1" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Neuer Task",
    "description": "Beschreibung",
    "priority": "high"
  }'
```

### 5. Mit Filtern und Pagination

```bash
curl "http://localhost:3006/api/tasks?status=pending&page=1&limit=5" \
  -H "Authorization: Bearer user-1"
```

## Test-Daten

**Users:**
- alice (user-1) / password123
- bob (user-2) / password456

**Tasks:**
- Alice: 3 Tasks
- Bob: 2 Tasks

## Architektur-Details

### Services

Die Business-Logik ist in Services ausgelagert:

- **AuthService**: Login, Register, Token-Validierung
- **TaskService**: CRUD-Operationen, Filtering, Pagination

### Middleware

**AuthenticationMiddleware**:
- Prüft Authorization Header
- Validiert Token
- Speichert UserId in HttpContext.Items

### Validierung

Alle Eingaben werden mit FluentValidation validiert:
- Username/Password Regeln
- Task-Validierung
- Query-Parameter-Validierung

## Unterschiede zu den Feature-Versionen

| Feature 1-5 | Feature Complete |
|-------------|------------------|
| Alles in einer Datei | Saubere Struktur (MVC) |
| Keine Services | Business-Logik in Services |
| Einfache Middleware | Vollständige Middleware |
| Teilweise TODOs | Vollständig implementiert |
| Nur Login | Login + Register + Profile |
| Einfaches Token | Token-Validierung |

## Nächste Schritte

Dies ist die Referenzimplementierung. Du kannst:
1. Den Code als Vorlage für eigene Projekte verwenden
2. Erweiterungen hinzufügen (z.B. Password-Hashing mit BCrypt)
3. JWT-Authentifizierung implementieren
4. Weitere Endpunkte hinzufügen

## Ressourcen

- [[C# Grundlagen]]
- [[ASP.NET Core Web API]]
- [[Clean Architecture]]
- [[REST API Standards]]
