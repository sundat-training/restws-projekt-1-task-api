# Task Management API - Feature 2: Validation

## Aufgaben

In diesem Feature erweiterst du die API um **Request-Validierung**. Die CRUD-Endpunkte aus Feature 1 sind bereits implementiert.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/tasks` mit Validierung | **Selber lösen** |
| PUT `/api/tasks/:id` mit Validierung | **Selber lösen** |
| Fehlerbehandlung für Validierungsfehler | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: POST Validierung implementieren

**Was du tun musst:**
1. Installiere `express-validator`: `npm install express-validator`
2. Importiere `body` und `validationResult` aus `express-validator`
3. Erstelle Validierungs-Regeln:
   - `title`: Pflichtfeld, max 200 Zeichen
   - `description`: Pflichtfeld
   - `priority`: Optional, nur `low`, `medium`, `high` erlaubt
4. Wende Validierung auf POST `/api/tasks` an
5. Prüfe auf Validierungsfehler und gib `400 Bad Request` zurück

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: POST Validierung implementieren`

**Tipp:**
```typescript
import { body, validationResult } from 'express-validator';

const validateCreateTask = [
  body('title').notEmpty().withMessage('Title is required')
               .isLength({ max: 200 }).withMessage('Title max 200 chars'),
  body('description').notEmpty().withMessage('Description is required'),
  body('priority').optional().isIn(['low', 'medium', 'high'])
                  .withMessage('Invalid priority')
];

app.post('/api/tasks', validateCreateTask, (req, res) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }
  // ... create task (aus Feature 1)
});
```

---

### Aufgabe 2: PUT Validierung implementieren

**Was du tun musst:**
1. Erstelle Validierungs-Regeln für PUT (alle Felder optional):
   - `title`: Optional, max 200 Zeichen
   - `description`: Optional
   - `status`: Optional, nur `pending`, `in_progress`, `completed` erlaubt
   - `priority`: Optional, nur `low`, `medium`, `high` erlaubt
2. Wende Validierung auf PUT `/api/tasks/:id` an
3. Prüfe auf Validierungsfehler

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: PUT Validierung implementieren`

**Tipp:**
```typescript
const validateUpdateTask = [
  body('title').optional().isLength({ max: 200 }),
  body('status').optional().isIn(['pending', 'in_progress', 'completed']),
  body('priority').optional().isIn(['low', 'medium', 'high'])
];
```

---

### Aufgabe 3: Validierungsfehler formatieren

**Was du tun musst:**
1. Formatiere die Fehlerausgabe konsistent
2. Jedes Fehlerobjekt sollte enthalten:
   - `type`: "field"
   - `msg`: Fehlermeldung
   - `path`: Feldname
   - `location`: "body"

**Erwartetes Format:**
```json
{
  "errors": [
    {
      "type": "field",
      "msg": "Title is required",
      "path": "title",
      "location": "body"
    }
  ]
}
```

---

## Gegeben (bereits implementiert)

- Alle CRUD-Endpunkte aus Feature 1 (GET, POST, PUT, DELETE)
- SQLite-Datenbank mit Tasks
- Basis-Express-Setup
- Alles in **einer Datei**: `src/index.ts` (keine Unterordner!)

### Zu implementieren

Erweitere `src/index.ts` um express-validator Middleware für POST und PUT. Schreibe die Validierung direkt in die index.ts - für dieses Feature brauchst du keine extra Dateien oder Ordner.

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
      "type": "field",
      "msg": "Title is required",
      "path": "title",
      "location": "body"
    }
  ]
}
```

### POST mit ungültigem Status

**Request:**
```bash
POST http://localhost:3002/api/tasks
Content-Type: application/json

{
  "title": "Valid Title",
  "description": "Valid description",
  "status": "invalid_status"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "type": "field",
      "msg": "Invalid status",
      "path": "status",
      "location": "body"
    }
  ]
}
```

### PUT mit zu langem Titel

**Request:**
```bash
PUT http://localhost:3002/api/tasks/task-1
Content-Type: application/json

{
  "title": "Dieser Titel ist viel zu lang und überschreitet die maximal erlaubten 200 Zeichen deutlich. Das ist nicht erlaubt und sollte einen Validierungsfehler auslösen..."
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "type": "field",
      "msg": "Title max 200 chars",
      "path": "title",
      "location": "body"
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
  "id": "uuid-hier",
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

- [ ] `express-validator` ist installiert und importiert
- [ ] POST `/api/tasks` validiert title (Pflicht, max 200)
- [ ] POST `/api/tasks` validiert description (Pflicht)
- [ ] POST `/api/tasks` validiert priority (optional, enum)
- [ ] PUT `/api/tasks/:id` validiert alle Felder als optional
- [ ] Bei Validierungsfehlern wird 400 Bad Request zurückgegeben
- [ ] Fehlerformat enthält type, msg, path, location
- [ ] Bei fehlenden Pflichtfeldern wird spezifische Fehlermeldung angezeigt
- [ ] Bei zu langem Titel (>200 Zeichen) wird Fehler angezeigt
- [ ] Bei ungültigem Enum-Wert wird Fehler angezeigt

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → feature-2-validation`
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
curl http://localhost:3002/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

---

## Tests ausführen

### Test-Struktur

Die Tests in `tests.http` sind in drei Kategorien eingeteilt:

| Kategorie | Anteil | Beschreibung |
|-----------|--------|--------------|
| TODO-Tests | ~1/3 | Request-Body/URL ist nicht vollständig - du musst die TODO-Kommentare im Code lesen und ergänzen |
| API-Aufgaben | ~1/3 | Tests sind vollständig, aber die API fehlt noch - du implementierst den Endpunkt |
| Referenz | ~1/3 | Bereits funktionsfähig - dienen als Referenz für das erwartete Verhalten |

**Hinweis:** Suche in `tests.http` nach `TODO:` Kommentaren, um zu sehen welche Requests du selbst vervollständigen musst.

1. **Datei öffnen:** `tests.http` (Aufgaben) oder `hint.http` (Lösungen als Referenz)
2. **Auf "Send Request" klicken** (erscheint über jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Option B: VSCode Command Palette (falls "Send Request" nicht erscheint)

Falls die "Send Request" Links nicht angezeigt werden:

1. **Datei öffnen:** `tests.http`
2. **Cursor auf die Anfrage setzen** (z.B. auf die Zeile mit `GET {{baseUrl}}/api/tasks`)
3. **Command Palette öffnen:** `Ctrl+Shift+P` (oder `Cmd+Shift+P` auf Mac)
4. **Tippen:** `Rest Client: Send Request`
5. **Auswählen** und die Anfrage wird ausgeführt
6. **Response wird angezeigt** (rechts im Panel)

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
  -d '{"title": "Dieser Titel ist viel zu lang und überschreitet die maximal erlaubten 200 Zeichen deutlich und sollte einen Fehler auslösen weil er einfach viel zu lang ist", "description": "Test"}'

# Task mit ungültigem status:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Valid", "description": "Valid", "status": "invalid"}'

# Validen Task erstellen:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Valid Task", "description": "Valid description", "priority": "high"}'
```

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- Node.js 23.x mit npm 11.x
- Docker & Docker Compose
- VSCode Extensions (REST Client, ESLint, etc.)
- SQLite Datenbank
- express-validator (muss installiert werden: `npm install express-validator`)

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 3 | POST ohne title | Validierung implementieren |
| Test 4 | POST ohne description | Validierung implementieren |
| Test 5 | POST mit zu langem title | Längen-Validierung |
| Test 6 | POST mit ungültigem status | Enum-Validierung |
| Test 7 | POST mit ungültigem priority | Enum-Validierung |
| Test 8 | PUT mit ungültigem status | PUT Validierung |
| Test 9 | PUT mit zu langem title | PUT Längen-Validierung |
| Test 10 | Valide Requests | Sollten weiterhin funktionieren |

### Tests ausführen

**Mit REST Client Extension (VS Code):**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 3: POST ohne title
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"description": "Missing title"}'
# Erwartet: 400 Bad Request

# Test 5: Zu langer title
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "'$(python3 -c "print('A'*201)")'", "description": "Test"}'
# Erwartet: 400 Bad Request
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Sieh dir `src/index.ts` an - die TODO-Kommentare helfen dir

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **src/index.ts öffnen** - Sieh dir die bereits implementierten Endpunkte an
3. **express-validator installieren** - `npm install express-validator`
4. **POST Validierung implementieren** - validateCreateTask Middleware
5. **PUT Validierung implementieren** - validateUpdateTask Middleware
6. **Testen** - Führe Tests mit REST Client Extension (`tests.http`) oder curl aus
7. **Vergleichen** - Mit der Lösung in `hint.http` vergleichen
8. **Weiter** - Wechsle zu `feature-3-filtering` für Query-Filtering

---

## Nächste Schritte

Nach diesem Feature:
- [[../feature-3-filtering/README|Feature 3: Filtering]] - Query-Filter hinzufügen

---

## Vergleich mit feature-complete

In `feature-complete` wird die Validierung anders strukturiert:
- Separate Validierungs-Dateien (`src/middleware/validation.ts`)
- Wiederverwendbare Validierungs-Regeln
- Zentrale Fehlerbehandlung

Deine Lösung in diesem Feature darf noch "einfacher" sein - Validierung direkt in `src/index.ts` ist erlaubt.

---

## Siehe auch

- [[express-validator|express-validator Dokumentation]]
- [[../feature-1-basics/README|Feature 1: Basics]] - Voraussetzung
- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[REST API Standards]]
