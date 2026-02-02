# Task Management API - Feature 1: Basics

## Aufgaben

In dieser Aufgabe implementierst du **3 Endpunkte** selber. Die GET-Endpunkte sind bereits fertig.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/tasks` | **Selber lösen** |
| PUT `/api/tasks/:id` | **Selber lösen** |
| DELETE `/api/tasks/:id` | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: POST /api/tasks

**Was du tun musst:**
1. Nimm `title`, `description`, `priority` aus `req.body`
2. Generiere eine neue ID mit `uuidv4()`
3. Default-Werte setzen: `status = "pending"`
4. Füge den Task in die SQLite-Datenbank ein
5. Gib den neuen Task mit Status `201 Created` zurück

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: POST /api/tasks implementieren`

**Tipp:**
```typescript
// So kannst du die Parameter holen
const { title, description, priority = 'medium' } = req.body;

// So generierst du eine UUID
const id = uuidv4();

// So fügst du in die DB ein
db.run(`INSERT INTO tasks ...`, [id, title, description, status, priority], callback);
```

---

### Aufgabe 2: PUT /api/tasks/:id

**Was du tun musst:**
1. Hole die `id` aus `req.params.id`
2. Nimm die zu aktualisierenden Felder aus `req.body`
3. Baue dynamisch das UPDATE-Statement (nur übergebene Felder)
4. Setze `updatedAt = CURRENT_TIMESTAMP`
5. Gib den aktualisierten Task zurück oder `404` wenn nicht gefunden

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: PUT /api/tasks/:id implementieren`

**Tipp:**
```typescript
// Dynamisch Felder sammeln
const updates = [];
const params = [];
if (title) { updates.push('title = ?'); params.push(title); }
if (status) { updates.push('status = ?'); params.push(status); }
// ...

// UPDATE Statement bauen
const query = `UPDATE tasks SET ${updates.join(', ')} WHERE id = ?`;
```

---

### Aufgabe 3: DELETE /api/tasks/:id

**Was du tun musst:**
1. Hole die `id` aus `req.params.id`
2. Lösche den Task aus der Datenbank
3. Gib `204 No Content` zurück
4. Oder `404` wenn nicht gefunden

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: DELETE /api/tasks/:id implementieren`

**Tipp:**
```typescript
db.run(`DELETE FROM tasks WHERE id = ?`, [id], function(err) {
  if (this.changes === 0) {
    // Task nicht gefunden
  } else {
    // Erfolg
  }
});
```

---

## Gegeben (bereits implementiert)

- GET `/api/tasks` - Alle Tasks abrufen
- GET `/api/tasks/:id` - Einzelner Task abrufen
- Datenbankschema und SQLite-Verbindung
- Alles in **einer Datei**: `src/index.ts` (keine Unterordner!)

### Zu implementieren

Erweitere `src/index.ts` um die fehlenden Endpunkte. Schreibe den Code direkt in die index.ts - für dieses Feature brauchst du keine extra Dateien oder Ordner.

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
  "id": "uuid-hier",
  "title": "Neuer Task",
  "description": "Beschreibung des Tasks",
  "status": "pending",
  "priority": "high",
  "createdAt": "2024-01-15T10:30:00.000Z",
  "updatedAt": "2024-01-15T10:30:00.000Z"
}
```

### PUT /api/tasks/:id

**Request:**
```bash
PUT http://localhost:3001/api/tasks/task-1
Content-Type: application/json

{
  "status": "completed",
  "priority": "low"
}
```

**Response (200 OK):**
```json
{
  "id": "task-1",
  "title": "Learn TypeScript",
  "description": "Complete TypeScript basics",
  "status": "completed",
  "priority": "low",
  "createdAt": "2024-01-15T10:00:00.000Z",
  "updatedAt": "2024-01-15T10:30:00.000Z"
}
```

### DELETE /api/tasks/:id

**Response (204 No Content):**
```
(leerer Body)
```

### Fehlerfälle

| Status | Bedingung |
|--------|-----------|
| 400 | Keine Felder zum Aktualisieren (PUT) |
| 404 | Task mit ID nicht gefunden |
| 500 | Datenbankfehler |

---

## Akzeptanzkriterien

- [ ] POST erstellt neuen Task mit uuidv4 als ID
- [ ] POST setzt default Status auf "pending"
- [ ] POST setzt default Priority auf "medium"
- [ ] PUT aktualisiert nur übergebene Felder
- [ ] PUT aktualisiert updatedAt Timestamp
- [ ] DELETE gibt 204 No Content zurück
- [ ] Alle Endpunkte geben bei nicht-existentem Task 404 zurück
- [ ] Alle Endpunkte geben bei Datenbankfehler 500 zurück

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → feature-1-basics`
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
curl http://localhost:3001/api/tasks
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

1. **Datei öffnen:** `tests.http` oder `hint.http`
2. **Auf "Send Request" klicken** (erscheint über jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Option B: Mit curl im Terminal

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

### Option C: VSCode Command Palette (falls "Send Request" nicht erscheint)

Falls die "Send Request" Links in der `tests.http` Datei nicht angezeigt werden:

1. **Datei öffnen:** `tests.http`
2. **Cursor auf die Anfrage setzen** (z.B. auf die Zeile mit `GET {{baseUrl}}/api/tasks`)
3. **Command Palette öffnen:** `Ctrl+Shift+P` (oder `Cmd+Shift+P` auf Mac)
4. **Tippen:** `Rest Client: Send Request`
5. **Auswählen** und die Anfrage wird ausgeführt
6. **Response wird angezeigt** (rechts im Panel)

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- Node.js 23.x mit npm 11.x
- Docker & Docker Compose
- VSCode Extensions (REST Client, ESLint, etc.)
- SQLite Datenbank

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 3 | POST neuen Task | Implementiere POST |
| Test 4 | PUT Status update | Implementiere PUT |
| Test 5 | PUT mehrere Felder | Implementiere PUT |
| Test 6 | DELETE Task | Implementiere DELETE |
| Test 7-11 | Fehlerbehandlung | Sollte funktionieren |

### Tests ausführen

**Mit REST Client Extension (VS Code):**
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
- Sieh dir `src/index.ts` an - die TODO-Kommentare helfen dir

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **src/index.ts öffnen** - Sieh dir die bereits implementierten GET-Endpunkte an
3. **Selber lösen** - Implementiere POST, PUT, DELETE
4. **Testen** - Führe Tests mit REST Client Extension (`tests.http`) oder curl aus
5. **Vergleichen** - Mit der Lösung in `hint.http` vergleichen
6. **Weiter** - Wechsle zu `feature-2-validation` für Validierung

---

## Vergleich mit feature-complete

In `feature-complete` wird der Code anders strukturiert sein:
- Separate Controller-Dateien (`src/controllers/`)
- Separate Router-Dateien (`src/routes/`)
- Middleware für Fehlerbehandlung (`src/middleware/`)
- TypeScript Interfaces für Models (`src/models/`)

Deine Lösung in diesem Feature darf noch "einfacher" sein - alles in einer Datei ist erlaubt.

---

## Nächste Schritte

Nach diesem Feature:
- [[../feature-2-validation/README|Feature 2: Validation]] - Request-Validierung hinzufügen

---

## Siehe auch

- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[REST API Standards]]
- [[Express.js]]
