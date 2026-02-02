# Task Management API - Feature 2: Validierung

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
1. Erstelle eine Validierungs-Funktion `validateCreateTask(array $input): array`
2. Validiere folgende Felder:
   - `title`: Pflichtfeld, max 200 Zeichen
   - `description`: Pflichtfeld
   - `priority`: Optional, nur `low`, `medium`, `high` erlaubt
3. Rufe die Validierung am Anfang von `createTask()` auf
4. Bei Fehlern: `400 Bad Request` mit errors-Array zuruckgeben

**Wo du es findest:**
- Datei: `index.php`
- Suche nach: `// TODO AUFGABE 1: POST Validierung implementieren`

**Tipp:**
```php
function validateCreateTask(array $input): array {
    $errors = [];
    
    // title validieren
    if (empty($input['title'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Title is required',
            'path' => 'title',
            'location' => 'body'
        ];
    } elseif (strlen($input['title']) > 200) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Title max 200 chars',
            'path' => 'title',
            'location' => 'body'
        ];
    }
    
    // description validieren
    if (empty($input['description'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Description is required',
            'path' => 'description',
            'location' => 'body'
        ];
    }
    
    // priority validieren (optional)
    if (isset($input['priority']) && !in_array($input['priority'], ['low', 'medium', 'high'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Invalid priority',
            'path' => 'priority',
            'location' => 'body'
        ];
    }
    
    return $errors;
}
```

---

### Aufgabe 2: PUT Validierung implementieren

**Was du tun musst:**
1. Erstelle eine Validierungs-Funktion `validateUpdateTask(array $input): array`
2. Alle Felder sind **optional** bei PUT:
   - `title`: Optional, max 200 Zeichen
   - `description`: Optional
   - `status`: Optional, nur `pending`, `in_progress`, `completed` erlaubt
   - `priority`: Optional, nur `low`, `medium`, `high` erlaubt
3. Rufe die Validierung am Anfang von `updateTask()` auf
4. Bei Fehlern: `400 Bad Request` mit errors-Array zuruckgeben

**Wo du es findest:**
- Datei: `index.php`
- Suche nach: `// TODO AUFGABE 2: PUT Validierung implementieren`

**Tipp:**
```php
function validateUpdateTask(array $input): array {
    $errors = [];
    
    // title validieren (optional)
    if (isset($input['title']) && strlen($input['title']) > 200) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Title max 200 chars',
            'path' => 'title',
            'location' => 'body'
        ];
    }
    
    // status validieren (optional)
    if (isset($input['status']) && !in_array($input['status'], ['pending', 'in_progress', 'completed'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Invalid status',
            'path' => 'status',
            'location' => 'body'
        ];
    }
    
    // priority validieren (optional)
    if (isset($input['priority']) && !in_array($input['priority'], ['low', 'medium', 'high'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Invalid priority',
            'path' => 'priority',
            'location' => 'body'
        ];
    }
    
    return $errors;
}
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
- Basis-PHP-Setup mit PDO
- Alles in **einer Datei**: `index.php` (keine Unterordner!)

### Zu implementieren

Erweitere `index.php` um Validierungs-Funktionen fur POST und PUT. Schreibe die Validierung direkt in die index.php - fur dieses Feature brauchst du keine extra Dateien oder Ordner.

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

### POST mit ungültigem Priority

**Request:**
```bash
POST http://localhost:3002/api/tasks
Content-Type: application/json

{
  "title": "Valid Title",
  "description": "Valid description",
  "priority": "invalid_priority"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "type": "field",
      "msg": "Invalid priority",
      "path": "priority",
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

- [ ] POST `/api/tasks` validiert title (Pflicht, max 200)
- [ ] POST `/api/tasks` validiert description (Pflicht)
- [ ] POST `/api/tasks` validiert priority (optional, enum)
- [ ] PUT `/api/tasks/:id` validiert alle Felder als optional
- [ ] PUT `/api/tasks/:id` validiert status (enum)
- [ ] Bei Validierungsfehlern wird 400 Bad Request zuruckgegeben
- [ ] Fehlerformat enthalt type, msg, path, location
- [ ] Bei fehlenden Pflichtfeldern wird spezifische Fehlermeldung angezeigt
- [ ] Bei zu langem Titel (>200 Zeichen) wird Fehler angezeigt
- [ ] Bei ungultigem Enum-Wert wird Fehler angezeigt

---

## Projekt starten

### Schritt 1: DevContainer offnen

1. VSCode starten
2. `File → Open Folder → php/feature-2-validation`
3. "In Container neu offnen?" - **Ja**
4. Warten bis Container bereit ist

### Schritt 2: API starten

```bash
# Im VSCode Terminal (DevContainer):
docker compose up -d
```

**Prüfen ob es lauft:**
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

## Tests ausfuhren

### Test-Struktur

Die Tests in `tests.http` sind in drei Kategorien eingeteilt:

| Kategorie | Anteil | Beschreibung |
|-----------|--------|--------------|
| Referenz | ~2/13 | Bereits funktionsfahig - dienen als Referenz |
| AUFGABEN | ~9/13 | Tests sind geschrieben, Validierung fehlt noch |
| TODO | ~2/13 | Request-Body ist nicht vollstandig - du musst die TODO-Kommentare lesen und erganzen |

**Hinweis:** Suche in `tests.http` nach `TODO:` Kommentaren, um zu sehen welche Requests du selbst vervollstandigen musst.

### REST Client Extension (VS Code)

1. Datei offnen: `tests.http` (Aufgaben) oder `hint.http` (Losungen als Referenz)
2. Auf "Send Request" klicken (erscheint uber jedem HTTP-Request)
3. Response wird angezeigt (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Mit curl im Terminal

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

# Task mit ungultigem priority:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Valid", "description": "Valid", "priority": "invalid"}'

# Validen Task erstellen:
curl -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Valid Task", "description": "Valid description", "priority": "high"}'

# PUT mit ungultigem status:
curl -X PUT http://localhost:3002/api/tasks/task-1 \
  -H "Content-Type: application/json" \
  -d '{"status": "not_valid"}'
```

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **index.php offnen** - Sieh dir die bereits implementierten Endpunkte an
3. **POST Validierung implementieren** - `validateCreateTask()` Funktion erstellen
4. **PUT Validierung implementieren** - `validateUpdateTask()` Funktion erstellen
5. **Validierung in Endpunkten aufrufen** - In `createTask()` und `updateTask()`
6. **Testen** - Führe Tests mit REST Client Extension (`tests.http`) oder curl aus
7. **Vergleichen** - Mit der Losung in `hint.http` vergleichen
8. **Weiter** - Wechsle zu `feature-3-filtering` fur Query-Filtering

---

## Nachste Schritte

Nach diesem Feature:
- `../feature-3-filtering/README` - Feature 3: Filtering - Query-Filter hinzufugen

---

## Siehe auch

- `../feature-1-basics/README` - Feature 1: Basics - Voraussetzung
- `tests.http` - Test-Szenarien ausfuhren
- `hint.http` - Losungen anzeigen
- `HINTS.md` - PHP-spezifische Hinweise und Code-Beispiele
