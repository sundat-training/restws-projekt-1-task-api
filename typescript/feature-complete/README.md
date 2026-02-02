# Task Management API - Feature Complete

## Übersicht

Dieses Feature enthält die **Gesamtlösung** mit allen Funktionalitäten:
- CRUD-Operationen für Tasks
- Request-Validierung
- Query-Filtering (status, priority, search)
- Pagination
- JWT-Authentifizierung

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → feature-complete`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist (npm install läuft automatisch)

### Schritt 2: API starten

```bash
# Im VSCode Terminal (DevContainer):
docker compose up -d
```

**Prüfen ob es läuft:**
```bash
docker ps
# Oder:
curl http://localhost:3006/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

## Alternative: Ohne DevContainer

### Voraussetzungen

- Node.js 20+
- npm oder yarn
- SQLite (wird automatisch erstellt)

### Installation

```bash
npm install
```

### Starten

```bash
# Development (mit ts-node)
npm run dev

# Production
npm run build
npm start
```

## API Endpunkte

### Authentication

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| POST | `/api/auth/register` | Benutzer registrieren |
| POST | `/api/auth/login` | Benutzer anmelden |
| GET | `/api/auth/profile` | Profil abrufen |

### Tasks

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| GET | `/api/tasks` | Alle Tasks abrufen (mit Filter & Pagination) |
| GET | `/api/tasks/:id` | Single Task abrufen |
| POST | `/api/tasks` | Task erstellen |
| PUT | `/api/tasks/:id` | Task aktualisieren |
| DELETE | `/api/tasks/:id` | Task löschen |

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

## Authentifizierung

Alle Task-Endpunkte erfordern einen JWT-Token im Header:

```
Authorization: Bearer <token>
```

## Test-Benutzer

| Username | Email | Password |
|----------|-------|----------|
| admin | admin@example.com | password123 |

## Testausführung

```bash
# Tests mit REST Client Extension
# Öffne tests.http und führe Requests aus

# Oder mit curl
curl -X GET http://localhost:3006/api/tasks \
  -H "Authorization: Bearer <token>"
```

## Aufgaben

Dieses Feature dient als **Gesamtlösung zum Vergleich**. Löse zuerst die vorherigen Features, bevor du hier nachschaust.

### Selber lösen

Vergleiche deine Lösung mit der Gesamtlösung:
- Code-Organisation
- Validierung
- Fehlerbehandlung
- API-Design

### Lösungsansätze analysieren

1. **Controller-Logik**: Wie werden CRUD-Operationen implementiert?
2. **Middleware**: Wofür wird `auth.ts` und `validation.ts` verwendet?
3. **Datenbank**: Wie werden Filter und Pagination realisiert?

## Vergleich mit vorherigen Features

| Feature | Auth | Validation | Filtering | Pagination |
|---------|------|------------|-----------|------------|
| basics | Nein | Nein | Nein | Nein |
| validation | Nein | Ja | Nein | Nein |
| filtering | Nein | Ja | Ja | Nein |
| pagination | Nein | Ja | Ja | Ja |
| auth | Ja | Ja | Ja | Ja |
| **complete** | **Ja** | **Ja** | **Ja** | **Ja** |

## Weiterführende Aufgaben

1. **Rate Limiting**: Implementiere Rate Limiting pro Benutzer
2. **Refresh Tokens**: Füge Refresh Token Mechanism hinzu
3. **API Versioning**: Implementiere `/api/v1/tasks`
4. **Logging**: Füge Request-Logging hinzu
5. **Unit Tests**: Schreibe Jest Tests für Controller

## Siehe auch

- [[OpenAPI Spezifikation]]
- [[REST API Standards]]
- [[JWT Authentifizierung]]
- [[Express.js]]
