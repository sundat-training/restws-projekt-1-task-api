# Task Management API - Feature 3: Filtering

## Aufgaben

In diesem Feature erweiterst du die API um **Query-Parameter-Filterung**. Die CRUD-Operationen und Validierung aus den vorherigen Features sind bereits implementiert.

| Deine Aufgabe | Status |
|---------------|--------|
| GET `/api/tasks?status=` | **Selber lösen** |
| GET `/api/tasks?priority=` | **Selber lösen** |
| GET `/api/tasks?search=` | **Selber lösen** |
| Kombinierte Filter | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Filter nach Status implementieren

**Was du tun musst:**
1. Erweitere den GET `/api/tasks` Endpunkt
2. Prüfe ob `req.query.status` vorhanden ist
3. Wenn ja, filtere die Tasks nach diesem Status
4. SQL-Query dynamisch aufbauen mit WHERE clause

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: Implementiere Filterung mit Query-Parametern`

**Tipp:**
```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status, priority, search } = req.query;
  
  let query = 'SELECT * FROM tasks';
  const params: any[] = [];
  const conditions: string[] = [];
  
  if (status) {
    conditions.push('status = ?');
    params.push(status);
  }
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  db.all(query, params, (err, tasks) => {
    // ...
  });
});
```

---

### Aufgabe 2: Filter nach Priority implementieren

**Was du tun musst:**
1. Erweitere die Filter-Logik um `?priority=`
2. Kombiniere mit Status-Filter (AND)
3. Beide Parameter sollen gleichzeitig funktionieren

**Wo du es findest:**
- Datei: `src/index.ts`
- Gleiche Stelle wie Aufgabe 1

**Tipp:**
```typescript
if (priority) {
  conditions.push('priority = ?');
  params.push(priority);
}
```

---

### Aufgabe 3: Suche implementieren

**Was du tun musst:**
1. Implementiere `?search=` Parameter
2. Suche sollte in `title` UND `description` suchen
3. Verwende SQL LIKE mit % Wildcards
4. Case-insensitive Suche (SQLite ist per default case-insensitive)

**Wo du es findest:**
- Datei: `src/index.ts`
- Gleiche Stelle wie Aufgabe 1

**Tipp:**
```typescript
if (search) {
  conditions.push('(title LIKE ? OR description LIKE ?)');
  const searchPattern = `%${search}%`;
  params.push(searchPattern, searchPattern);
}
```

---

## Gegeben (bereits implementiert)

- Alle CRUD-Endpunkte (GET, POST, PUT, DELETE)
- Validierung mit express-validator
- SQLite-Datenbank mit 5 Beispiel-Tasks
- Alles in **einer Datei**: `src/index.ts` (keine Unterordner!)

### Zu implementieren

Erweitere den GET `/api/tasks` Endpunkt um Query-Parameter-Filterung.

---

## Erwartetes Ergebnis

### Filter nach Status

**Request:**
```bash
GET http://localhost:3003/api/tasks?status=pending
```

**Response:**
```json
[
  {
    "id": "task-3",
    "title": "Write docs",
    "status": "pending",
    "priority": "medium"
  },
  {
    "id": "task-4", 
    "title": "Test API",
    "status": "pending",
    "priority": "low"
  }
]
```

### Filter nach Priority

**Request:**
```bash
GET http://localhost:3003/api/tasks?priority=high
```

**Response:**
```json
[
  {
    "id": "task-1",
    "title": "Learn TypeScript",
    "status": "completed",
    "priority": "high"
  },
  {
    "id": "task-2",
    "title": "Build REST API", 
    "status": "in_progress",
    "priority": "high"
  }
]
```

### Kombinierte Filter

**Request:**
```bash
GET http://localhost:3003/api/tasks?status=in_progress&priority=high
```

**Response:**
```json
[
  {
    "id": "task-2",
    "title": "Build REST API",
    "status": "in_progress",
    "priority": "high"
  }
]
```

### Suche

**Request:**
```bash
GET http://localhost:3003/api/tasks?search=TypeScript
```

**Response:**
```json
[
  {
    "id": "task-1",
    "title": "Learn TypeScript",
    "description": "Complete TypeScript basics",
    "status": "completed",
    "priority": "high"
  }
]
```

---

## Akzeptanzkriterien

- [ ] `?status=pending` filtert nach pending Tasks
- [ ] `?status=completed` filtert nach completed Tasks
- [ ] `?status=in_progress` filtert nach in_progress Tasks
- [ ] `?priority=high|medium|low` filtert nach Priority
- [ ] Kombinierte Filter funktionieren: `?status=X&priority=Y`
- [ ] `?search=keyword` sucht in title und description
- [ ] Suche ist case-insensitive
- [ ] Suche verwendet Wildcards (LIKE %keyword%)
- [ ] Kombination Filter + Suche funktioniert
- [ ] Ohne Parameter werden alle Tasks zurückgegeben

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → feature-3-filtering`
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
curl http://localhost:3003/api/tasks
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
# Alle Tasks:
curl http://localhost:3003/api/tasks

# Filter nach status:
curl "http://localhost:3003/api/tasks?status=pending"

# Filter nach priority:
curl "http://localhost:3003/api/tasks?priority=high"

# Kombinierte Filter:
curl "http://localhost:3003/api/tasks?status=in_progress&priority=high"

# Suche:
curl "http://localhost:3003/api/tasks?search=TypeScript"
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

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 4-6 | Filter nach Status | Implementiere ?status= |
| Test 7-9 | Filter nach Priority | Implementiere ?priority= |
| Test 10-11 | Kombinierte Filter | Beide Parameter zusammen |
| Test 12-14 | Suche | Implementiere ?search= |

### Tests ausführen

**Mit REST Client Extension:**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 4: Filter nach pending
curl "http://localhost:3003/api/tasks?status=pending"

# Test 7: Filter nach high priority
curl "http://localhost:3003/api/tasks?priority=high"

# Test 10: Kombinierte Filter
curl "http://localhost:3003/api/tasks?status=in_progress&priority=medium"
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Sieh dir `src/index.ts` an - die TODO-Kommentare helfen dir

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **src/index.ts öffnen** - Finde den GET /api/tasks Endpunkt
3. **Query-Parameter auslesen** - Verwende `req.query`
4. **SQL-Query aufbauen** - Dynamisch WHERE clauses hinzufügen
5. **Testen** - Führe Tests mit REST Client oder curl aus
6. **Vergleichen** - Mit der Lösung in `hint.http` vergleichen
7. **Weiter** - Wechsle zu `feature-4-pagination` für Pagination

---

## Nächste Schritte

Nach diesem Feature:
- [[../feature-4-pagination/README|Feature 4: Pagination]] - Pagination hinzufügen

---

## Vergleich mit feature-complete

In `feature-complete` wird die Filterung in separate Funktionen ausgelagert:
- `buildWhereClause()` - Baut die WHERE Bedingungen
- `buildQuery()` - Konstruiert die komplette SQL Query

Deine Lösung darf direkt im GET Endpunkt implementiert sein.

---

## Siehe auch

- [[Express.js Query Parameters]]
- [[SQLite WHERE Clause]]
- [[../feature-2-validation/README|Feature 2: Validation]] - Voraussetzung
- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[REST API Standards]]
