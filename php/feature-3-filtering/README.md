# Task Management API - Feature 3: Filtering

## Aufgaben

In diesem Feature erweiterst du die API um **Query-Parameter-Filterung**. Die CRUD-Operationen und Validierung aus den vorherigen Features sind bereits implementiert.

| Deine Aufgabe | Status |
|---------------|--------|
| GET `/api/tasks?status=` | **Selber losen** |
| GET `/api/tasks?priority=` | **Selber losen** |
| GET `/api/tasks?search=` | **Selber losen** |
| Kombinierte Filter | **Selber losen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Filter nach Status implementieren

**Was du tun musst:**
1. Erweitere den GET `/api/tasks` Endpunkt in `index.php`
2. Pruefe ob `$_GET['status']` vorhanden ist
3. Wenn ja, filtere die Tasks nach diesem Status
4. SQL-Query dynamisch aufbauen mit WHERE clause

**Wo du es findest:**
- Datei: `index.php`
- Suche nach: `// TODO: Implementiere Filterung mit Query-Parametern`

**Tipp:**
```php
if (isset($_GET['status'])) {
    $conditions[] = "status = ?";
    $params[] = $_GET['status'];
}
```

---

### Aufgabe 2: Filter nach Priority implementieren

**Was du tun musst:**
1. Erweitere die Filter-Logik um `$_GET['priority']`
2. Kombiniere mit Status-Filter (AND)
3. Beide Parameter sollen gleichzeitig funktionieren

**Wo du es findest:**
- Datei: `index.php`
- Gleiche Stelle wie Aufgabe 1

**Tipp:**
```php
if (isset($_GET['priority'])) {
    $conditions[] = "priority = ?";
    $params[] = $_GET['priority'];
}
```

---

### Aufgabe 3: Suche implementieren

**Was du tun musst:**
1. Implementiere `$_GET['search']` Parameter
2. Suche sollte in `title` UND `description` suchen
3. Verwende SQL LIKE mit % Wildcards
4. Case-insensitive Suche (SQLite ist per default case-insensitive)

**Wo du es findest:**
- Datei: `index.php`
- Gleiche Stelle wie Aufgabe 1

**Tipp:**
```php
if (isset($_GET['search'])) {
    $conditions[] = "(title LIKE ? OR description LIKE ?)";
    $searchPattern = "%" . $_GET['search'] . "%";
    $params[] = $searchPattern;
    $params[] = $searchPattern;
}
```

---

## Gegeben (bereits implementiert)

- Alle CRUD-Endpunkte (GET, POST, PUT, DELETE)
- Validierung
- SQLite-Datenbank mit 5 Beispiel-Tasks
- Alles in **einer Datei**: `index.php` (keine Unterordner!)

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
GET http://localhost:3003/api/tasks?search=API
```

**Response:**
```json
[
  {
    "id": "task-2",
    "title": "Build REST API",
    "description": "Create REST API with PHP",
    "status": "in_progress",
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
- [ ] Ohne Parameter werden alle Tasks zuruckgegeben

---

## Projekt starten

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgefuhrt (innerhalb des DevContainers).

### Schritt 1: DevContainer offnen

1. VSCode starten
2. `File -> Open Folder -> php/feature-3-filtering`
3. "In Container neu offnen?" -> **Ja**
4. Warten bis Container bereit ist

### Schritt 2: API starten

```bash
# Im VSCode Terminal (DevContainer):
docker compose up -d
```

**Prufen ob es lauft:**
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

## Tests ausfuhren

### Test-Struktur

| Test-Kategorie | Beschreibung |
|----------------|--------------|
| **~1/3 mit TODOs** | Teilnehmer mussen Request-URL mit Query-Parametern schreiben |
| **~1/3 Losungen** | Fertige Requests in hint.http |
| **Referenz** | Bereits funktionsfahig, dienen als Beispiel |

**Hinweis:** Suche nach `TODO:` in `tests.http`

### Option A: REST Client Extension (empfohlen)

1. **Datei offnen:** `tests.http` (Aufgaben) oder `hint.http` (Losungen als Referenz)
2. **Auf "Send Request" klicken** (erscheint uber jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Option B: VSCode Command Palette (falls "Send Request" nicht erscheint)

Falls die "Send Request" Links nicht angezeigt werden:

1. **Datei offnen:** `tests.http`
2. **Cursor auf die Anfrage setzen**
3. **Command Palette offnen:** `Ctrl+Shift+P` (oder `Cmd+Shift+P` auf Mac)
4. **Tippen:** `Rest Client: Send Request`
5. **Auswahlen** und die Anfrage wird ausgefuhrt

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
curl "http://localhost:3003/api/tasks?search=API"
```

> **Wichtig:** Bei URLs mit Query-Parametern immer Anfuhrungszeichen verwenden!

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 4-5 | Filter nach Status | Implementiere ?status= |
| Test 6-7 | Filter nach Priority | Implementiere ?priority= |
| Test 8-9 | Kombinierte Filter | Beide Parameter zusammen |
| Test 10-12 | Suche | Implementiere ?search= |

### Tests ausfuhren

**Mit REST Client Extension:**
1. Offne `tests.http`
2. Klick auf "Send Request" uber jedem Test

**Mit curl:**
```bash
# Test 4: Filter nach pending
curl "http://localhost:3003/api/tasks?status=pending"

# Test 6: Filter nach high priority
curl "http://localhost:3003/api/tasks?priority=high"

# Test 8: Kombinierte Filter
curl "http://localhost:3003/api/tasks?status=pending&priority=high"
```

### Losungen anzeigen

Wenn du nicht weiterkommst:
- Offne `hint.http` fur die fertigen Requests
- Sieh dir `HINTS.md` an mit PHP-spezifischen Hinweisen
- Sieh dir `index.php` an - die TODO-Kommentare helfen dir

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **index.php offnen** - Finde den GET /api/tasks Endpunkt
3. **Query-Parameter auslesen** - Verwende `$_GET`
4. **SQL-Query aufbauen** - Dynamisch WHERE clauses hinzufugen
5. **Testen** - Fuhre Tests mit REST Client oder curl aus
6. **Vergleichen** - Mit der Losung in `hint.http` vergleichen
7. **Weiter** - Wechsle zu `feature-4-pagination` fur Pagination

---

## Nachste Schritte

Nach diesem Feature:
- [[../feature-4-pagination/README|Feature 4: Pagination]] - Pagination hinzufugen

---

## Vergleich mit feature-complete

In `feature-complete` wird die Filterung in separate Funktionen ausgelagert:
- `buildWhereClause()` - Baut die WHERE Bedingungen
- `buildQuery()` - Konstruiert die komplette SQL Query

Deine Losung darf direkt im GET Endpunkt implementiert sein.

---

## Siehe auch

- [[PHP $_GET]]
- [[SQLite WHERE Clause]]
- [[../feature-2-validation/README|Feature 2: Validation]] - Voraussetzung
- [[tests.http|Test-Szenarien ausfuhren]]
- [[hint.http|Losungen anzeigen]]
- [[HINTS.md|Php-spezifische Hinweise]]
- [[REST API Standards]]
