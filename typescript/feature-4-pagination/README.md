# Task Management API - Feature 4: Pagination

## Aufgaben

In diesem Feature erweiterst du die API um **Pagination**. Die CRUD-Operationen, Validierung und Filterung aus den vorherigen Features sind bereits implementiert.

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
1. Erweitere den GET `/api/tasks` Endpunkt
2. Extrahiere `page` und `limit` aus `req.query`
3. Berechne `offset = (page - 1) * limit`
4. Füge `LIMIT` und `OFFSET` zur SQL-Query hinzu
5. Hole die Gesamtanzahl der Tasks für `totalItems`
6. Baue das Pagination-Objekt

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: Implementiere Pagination mit Query-Parametern`

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
```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  // Pagination-Parameter extrahieren
  const page = parseInt(req.query.page as string) || 1;
  const limit = parseInt(req.query.limit as string) || 10;
  const offset = (page - 1) * limit;
  
  // Gesamtanzahl holen
  db.get('SELECT COUNT(*) as total FROM tasks', (err, result: { total: number }) => {
    const totalItems = result.total;
    const totalPages = Math.ceil(totalItems / limit);
    
    // Tasks mit LIMIT und OFFSET holen
    db.all(`SELECT * FROM tasks LIMIT ? OFFSET ?`, [limit, offset], (err, tasks) => {
      const response = {
        data: tasks,
        pagination: {
          page,
          limit,
          totalItems,
          totalPages,
          hasNextPage: page < totalPages,
          hasPreviousPage: page > 1
        }
      };
      res.json(response);
    });
  });
});
```

---

### Aufgabe 2: Randfälle behandeln

**Was du tun musst:**
1. `page=0` oder negative Werte → als `page=1` behandeln
2. `limit=0` oder negative Werte → Standard-Limit verwenden
3. `page` größer als `totalPages` → letzte Seite zurückgeben
4. Keine Ergebnisse → leere `data` aber vollständige `pagination`

**Tipp:**
```typescript
// Seite validieren
let currentPage = Math.max(1, parseInt(req.query.page as string) || 1);
let currentLimit = Math.max(1, parseInt(req.query.limit as string) || 10);

// page > totalPages korrigieren
if (currentPage > totalPages && totalPages > 0) {
  currentPage = totalPages;
}
```

---

### Aufgabe 3: Pagination mit Filter kombinieren

**Was du tun musst:**
1. Pagination läuft NACH der Filterung
2. `COUNT(*)` muss die gefilterten Tasks zählen
3. LIMIT/OFFSET auf die gefilterten Ergebnisse anwenden

**Tipp:**
```typescript
// Zuerst Filter aufbauen wie in Feature 3
let query = 'SELECT * FROM tasks';
const params: any[] = [];
const conditions: string[] = [];

// Filter hinzufuegen...
if (conditions.length > 0) {
  query += ' WHERE ' + conditions.join(' AND ');
}

// Gesamtanzahl der gefilterten Tasks holen
let countQuery = 'SELECT COUNT(*) as total FROM tasks';
if (conditions.length > 0) {
  countQuery += ' WHERE ' + conditions.join(' AND ');
}

// Dann LIMIT und OFFSET anhaengen
query += ` LIMIT ? OFFSET ?`;
params.push(limit, offset);

db.all(query, params, (err, tasks) => {
  // Response bauen...
});
```

---

## Gegeben (bereits implementiert)

- Alle CRUD-Endpunkte (GET, POST, PUT, DELETE)
- Validierung mit express-validator
- Query-Parameter-Filterung (status, priority, search)
- SQLite-Datenbank mit **15 Beispiel-Tasks** (für Pagination benötigt)
- Alles in **einer Datei**: `src/index.ts` (keine Unterordner!)

### Zu implementieren

Erweitere den GET `/api/tasks` Endpunkt um Pagination-Funktionalität.

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
      "title": "Learn TypeScript basics",
      "description": "Complete TypeScript fundamentals course",
      "status": "completed",
      "priority": "high"
    },
    {
      "id": "task-2",
      "title": "Build REST API",
      "description": "Create Task API with Express",
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
      "description": "Implement Jest tests for API",
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
    {
      "id": "task-6",
      "title": "Setup CI/CD pipeline",
      "description": "Configure GitHub Actions for deployment",
      "status": "pending",
      "priority": "high"
    },
    {
      "id": "task-7",
      "title": "Add authentication",
      "description": "Implement JWT-based auth",
      "status": "pending",
      "priority": "high"
    },
    ...
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
    {
      "id": "task-3",
      "title": "Write documentation",
      "status": "pending",
      "priority": "medium"
    },
    {
      "id": "task-4",
      "title": "Write unit tests",
      "status": "pending",
      "priority": "low"
    },
    {
      "id": "task-6",
      "title": "Setup CI/CD pipeline",
      "status": "pending",
      "priority": "high"
    }
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

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → feature-4-pagination`
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
curl http://localhost:3004/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

---

## Tests ausführen

### Test-Struktur

| Test-Kategorie | Beschreibung |
|----------------|--------------|
| **~1/3 mit TODOs** | Teilnehmer müssen Request-URL mit Query-Parametern schreiben |
| **~1/3 API-Aufgaben** | Tests vollständig, API muss implementiert werden |
| **~1/3 Referenz** | Bereits funktionsfähig, dienen als Beispiel |

**Hinweis:** Suche nach `TODO:` in `tests.http`

### Option A: REST Client Extension (empfohlen)

1. **Datei öffnen:** `tests.http` (Aufgaben) oder `hint.http` (Lösungen als Referenz)
2. **Auf "Send Request" klicken** (erscheint über jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Option B: VSCode Command Palette (falls "Send Request" nicht erscheint)

Falls die "Send Request" Links nicht angezeigt werden:

1. **Datei öffnen:** `tests.http`
2. **Cursor auf die Anfrage setzen**
3. **Command Palette öffnen:** `Ctrl+Shift+P` (oder `Cmd+Shift+P` auf Mac)
4. **Tippen:** `Rest Client: Send Request`
5. **Auswählen** und die Anfrage wird ausgeführt

### Option C: Mit curl im Terminal

```bash
# Alle Tasks mit Pagination:
curl http://localhost:3004/api/tasks?page=1&limit=5

# Zweite Seite:
curl "http://localhost:3004/api/tasks?page=2&limit=5"

# Mit Filter und Pagination:
curl "http://localhost:3004/api/tasks?status=pending&page=1&limit=3"

# Mit Suche und Pagination:
curl "http://localhost:3004/api/tasks?search=TypeScript&page=1&limit=2"
```

> **Wichtig:** Bei URLs mit Query-Parametern immer Anführungszeichen verwenden!

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- Node.js 23.x mit npm 11.x
- Docker & Docker Compose
- VSCode Extensions (REST Client, ESLint, etc.)
- SQLite Datenbank
- express-validator (bereits installiert)
- 15 Beispiel-Tasks für Pagination-Tests

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 3-4 | Default + Limit Pagination | Implementiere ?page= & ?limit= |
| Test 5-6 | Seitennavigation | Implementiere offset Berechnung |
| Test 7-8 | Filter + Pagination | Kombiniere mit bestehender Filterung |
| Test 9-11 | Randfälle | Behandle ungültige Werte |
| Test 12-14 | hasNext/PreviousPage | Berechne boolean Flags |

### Tests ausführen

**Mit REST Client Extension:**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 4: Erste Seite mit limit=5
curl "http://localhost:3004/api/tasks?page=1&limit=5"

# Test 5: Zweite Seite
curl "http://localhost:3004/api/tasks?page=2&limit=5"

# Test 7: Filter + Pagination
curl "http://localhost:3004/api/tasks?status=pending&page=1&limit=3"
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Sieh dir `src/index.ts` an - die TODO-Kommentare helfen dir

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **src/index.ts öffnen** - Finde den GET /api/tasks Endpunkt
3. **Pagination-Parameter auslesen** - Verwende `req.query.page` und `req.query.limit`
4. **Offset berechnen** - `(page - 1) * limit`
5. **COUNT Query ausführen** - Für totalItems
6. **LIMIT/OFFSET Query ausführen** - Für die Daten
7. **Response bauen** - Mit pagination Objekt
8. **Testen** - Führe Tests mit REST Client oder curl aus
9. **Vergleichen** - Mit der Lösung in `hint.http` vergleichen

---

## Nächste Schritte

Nach diesem Feature:
- [[../feature-complete/README|Feature Complete]] - Vollständige Implementierung mit ausgelagerten Funktionen

---

## Vergleich mit feature-complete

In `feature-complete` wird die Pagination in separate Funktionen ausgelagert:
- `buildPaginationQuery()` - Baut Query mit LIMIT und OFFSET
- `getTotalCount()` - Holte die Gesamtanzahl
- `buildPaginationResponse()` - Baut das Response-Objekt

Deine Lösung darf direkt im GET Endpunkt implementiert sein.

---

## Siehe auch

- [[Express.js Query Parameters]]
- [[SQLite LIMIT OFFSET]]
- [[../feature-3-filtering/README|Feature 3: Filtering]] - Voraussetzung
- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[REST API Standards]]
