# Task Management API - Feature Complete (PHP)

## Overview

Dieses Projekt ist eine vollständige Implementierung einer RESTful Task Management API in PHP. Es demonstriert alle wesentlichen Konzepte einer modernen Web-API:

- **CRUD-Operationen** für Tasks (Create, Read, Update, Delete)
- **Request-Validierung** für sichere Eingabeverarbeitung
- **Query-Filtering** nach status, priority und Suchbegriffen
- **Pagination** für effiziente Datennutzung
- **Authentifizierung** mit User-Isolation für Datensicherheit

Die Implementierung besteht aus einer einzigen Datei (`index.php`) und zeigt, wie man eine professionelle API mit minimalem Code realisieren kann.

## Architektur

Das Projekt nutzt eine Single-File-Architektur für maximale Übersichtlichkeit und einfaches Debugging. Alle Endpunkte, Validierungen und Geschäftslogik sind in einer Datei zusammengefasst. Dies eignet sich hervorragend für Lernzwecke und schnelle Prototypen.

## Features

### Authentication Endpoints

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| POST | `/api/auth/login` | Benutzer-Login und Token-Generierung |

### Tasks Endpoints

| Methode | Endpoint | Beschreibung |
|---------|----------|--------------|
| GET | `/api/tasks` | Alle Tasks des eingeloggten Users abrufen |
| POST | `/api/tasks` | Neuen Task erstellen |
| GET | `/api/tasks/{id}` | Einzelnen Task abrufen |
| PUT | `/api/tasks/{id}` | Task aktualisieren |
| DELETE | `/api/tasks/{id}` | Task löschen |

## Query-Parameter

### Filtering

Alle Tasks können nach verschiedenen Kriterien gefiltert werden:

| Parameter | Werte | Beschreibung |
|-----------|-------|--------------|
| `status` | pending, in_progress, completed | Filtert nach Status |
| `priority` | low, medium, high | Filtert nach Priorität |
| `search` | Text | Freitextsuche im Titel |

### Pagination

Für große Datenmengen steht Pagination zur Verfügung:

| Parameter | Standardwert | Beschreibung |
|-----------|--------------|--------------|
| `page` | 1 | Aktuelle Seitennummer |
| `limit` | 10 | Elemente pro Seite |

### Kombiniertes Beispiel

```http
GET /api/tasks?status=pending&priority=high&page=1&limit=5
```

Dieser Request gibt die ersten 5 ausstehenden Tasks mit hoher Priorität zurück.

## Response Format

Die API liefert alle Responses in einem konsistenten JSON-Format:

```json
{
    "data": [
        {
            "id": 1,
            "title": "Task Titel",
            "description": "Task Beschreibung",
            "status": "pending",
            "priority": "high",
            "user_id": 1,
            "created_at": "2024-01-15T10:30:00Z",
            "updated_at": "2024-01-15T10:30:00Z"
        }
    ],
    "pagination": {
        "page": 1,
        "limit": 10,
        "total": 25,
        "total_pages": 3
    }
}
```

## Projekt starten

### Mit DevContainer

Das Projekt enthält eine DevContainer-Konfiguration für einfaches Setup:

1. Projekt in VS Code öffnen
2. DevContainer neu starten (STRG+SHIFT+P → "Dev Containers: Rebuild Container")
3. API ist automatisch unter Port 3006 verfügbar

### Manuell mit Docker

```bash
# Container bauen
docker build -t task-api-php .

# Container starten
docker run -p 3006:80 task-api-php
```

Die API ist dann unter `http://localhost:3006` erreichbar.

## Test-Benutzer

Für Entwicklungs- und Testzwecke stehen folgende Benutzer zur Verfügung:

| Benutzername | Passwort | User-ID |
|--------------|----------|---------|
| alice | password123 | 1 |
| bob | password456 | 2 |

## Curl-Beispiele für Testing

### Login

```bash
curl -X POST http://localhost:3006/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "password123"}'
```

### Alle Tasks abrufen

```bash
curl -X GET http://localhost:3006/api/tasks \
  -H "Authorization: Bearer user-1"
```

### Tasks mit Pagination

```bash
curl -X GET "http://localhost:3006/api/tasks?page=1&limit=5" \
  -H "Authorization: Bearer user-1"
```

### Tasks filtern

```bash
curl -X GET "http://localhost:3006/api/tasks?status=pending&priority=high" \
  -H "Authorization: Bearer user-1"
```

### Task erstellen

```bash
curl -X POST http://localhost:3006/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer user-1" \
  -d '{"title": "Neuer Task", "description": "Beschreibung", "priority": "high"}'
```

### Task aktualisieren

```bash
curl -X PUT http://localhost:3006/api/tasks/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer user-1" \
  -d '{"title": "Aktualisierter Task", "status": "completed"}'
```

### Task löschen

```bash
curl -X DELETE http://localhost:3006/api/tasks/1 \
  -H "Authorization: Bearer user-1"
```

## Hinweis

Dies ist die Referenzlösung für das Projekt. Alle Features sind vollständig implementiert und funktionsfähig. Die Implementierung dient als Blaupause für eigene Erweiterungen und demonstriert Best Practices für PHP REST APIs.
