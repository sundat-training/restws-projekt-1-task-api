# Hints - Feature 2: Validierung (PHP)

## Übersicht

Diese Datei enthalt PHP-spezifische Hinweise und Code-Beispiele fur die Validierung in Feature 2.

---

## Validierungs-Funktionen erstellen

### validateCreateTask() - fur POST

```php
/**
 * Validiert die Eingabedaten fur einen neuen Task.
 * 
 * @param array $input Die Request-Body-Daten
 * @return array Array mit Validierungsfehlern (leer wenn gueltig)
 */
function validateCreateTask(array $input): array {
    $errors = [];
    
    // title: Pflichtfeld, max 200 Zeichen
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
    
    // description: Pflichtfeld
    if (empty($input['description'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Description is required',
            'path' => 'description',
            'location' => 'body'
        ];
    }
    
    // priority: Optional, aber muss gueltig sein wenn angegeben
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

### validateUpdateTask() - fur PUT

```php
/**
 * Validiert die Eingabedaten fur ein Task-Update.
 * Alle Felder sind OPTIONAL - nur prufen wenn vorhanden.
 * 
 * @param array $input Die Request-Body-Daten
 * @return array Array mit Validierungsfehlern (leer wenn gueltig)
 */
function validateUpdateTask(array $input): array {
    $errors = [];
    
    // title: Optional, aber wenn vorhanden max 200 Zeichen
    if (isset($input['title']) && strlen($input['title']) > 200) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Title max 200 chars',
            'path' => 'title',
            'location' => 'body'
        ];
    }
    
    // description: Optional (keine Laengenbeschrankung)
    
    // status: Optional, aber wenn vorhanden muss es gueltig sein
    if (isset($input['status']) && !in_array($input['status'], ['pending', 'in_progress', 'completed'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Invalid status',
            'path' => 'status',
            'location' => 'body'
        ];
    }
    
    // priority: Optional, aber wenn vorhanden muss es gueltig sein
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

## Validierung in Endpunkten verwenden

### In createTask() einbauen

```php
function createTask(PDO $db, array $input): void {
    // 1. Validierung aufrufen
    $errors = validateCreateTask($input);
    
    // 2. Bei Fehlern: 400 Bad Request zurueckgeben
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }
    
    // 3. Ab hier: Task erstellen (bereits implementiert)
    // ...
}
```

### In updateTask() einbauen

```php
function updateTask(PDO $db, string $id, array $input): void {
    // 1. Validierung aufrufen
    $errors = validateUpdateTask($input);
    
    // 2. Bei Fehlern: 400 Bad Request zurueckgeben
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }
    
    // 3. Ab hier: Task aktualisieren (bereits implementiert)
    // ...
}
```

---

## Nutzliche PHP-Funktionen

### Leere Werte prufen

```php
// empty() pruft: "", 0, "0", null, false, und leere Arrays
empty($input['title']);           // true wenn nicht vorhanden ODER leer
isset($input['title']);           // true wenn vorhanden (auch leer)
$input['title'] ?? null;          // null wenn nicht vorhanden

//Fuer "darf nicht leer sein":
if (empty(trim($input['title']))) {
    // title ist leer oder nur Whitespace
}
```

### String-Lange prufen

```php
strlen($input['title']);  // Anzahl der Bytes
mb_strlen($input['title']);  // Anzahl der Zeichen (UTF-8)

if (strlen($input['title']) > 200) {
    // Zu lang!
}
```

### Enum-Werte prufen

```php
$validPriorities = ['low', 'medium', 'high'];

in_array($input['priority'], $validPriorities);
// Oder mit striktem Vergleich:
in_array($input['priority'], $validPriorities, true);
```

### JSON Response senden

```php
http_response_code(400);  // Statuscode setzen
echo json_encode(['errors' => $errors]);  // JSON ausgeben

// Wichtig: json_encode konfiguriern fur deutsche Umlaute
header('Content-Type: application/json; charset=utf-8');
```

---

## Komplette Validierungs-Struktur

Platziere die Funktionen am Ende der `index.php` Datei, vor der `generateUuid()` Funktion:

```php
// ============================================================
// VALIDIERUNGS-FUNKTIONEN (diese Gruppe selbst erstellen)
// ============================================================

function validateCreateTask(array $input): array {
    $errors = [];
    
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
    
    if (empty($input['description'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Description is required',
            'path' => 'description',
            'location' => 'body'
        ];
    }
    
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

function validateUpdateTask(array $input): array {
    $errors = [];
    
    if (isset($input['title']) && strlen($input['title']) > 200) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Title max 200 chars',
            'path' => 'title',
            'location' => 'body'
        ];
    }
    
    if (isset($input['status']) && !in_array($input['status'], ['pending', 'in_progress', 'completed'])) {
        $errors[] = [
            'type' => 'field',
            'msg' => 'Invalid status',
            'path' => 'status',
            'location' => 'body'
        ];
    }
    
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

// ============================================================
// Hilfsfunktion: UUID generieren
// ============================================================
```

---

## 400 Bad Request mit Fehlern zuruckgeben

```php
function returnValidationErrors(array $errors): void {
    http_response_code(400);
    header('Content-Type: application/json');
    echo json_encode(['errors' => $errors]);
}

// Verwendung:
$errors = validateCreateTask($input);
if (!empty($errors)) {
    returnValidationErrors($errors);
    return;
}
```

---

## Haufige Fehler vermeiden

### 1. isset() vs empty()

```php
// FALSCH:
if (isset($input['title']) && strlen($input['title']) > 200) {
    // Dieser Code wird auch ausgefuhrt wenn title ein leerer String ist
}

// RICHTIG:
if (isset($input['title']) && $input['title'] !== '' && strlen($input['title']) > 200) {
    // Laengenueberpruefung nur wenn title nicht leer ist
}
```

### 2. empty() bei Strings

```php
// empty("0") ist TRUE in PHP!
//Fuer "title darf nicht '0' sein":
if ($input['title'] === '') {
    // Leerer String
}

//Fuer "title muss vorhanden sein":
if (empty($input['title'])) {
    // title ist null, "", 0, "0", false, oder nicht gesetzt
}
```

### 3. strlen() vs mb_strlen()

```php
// Bei UTF-8 Zeichen (z.B. Umlaute):
$title = "Überschrift";  // 10 Zeichen
strlen($title);           // 11 Bytes (Umlaut sind 2 Bytes)
mb_strlen($title);        // 10 Zeichen

// Fur dieses Projekt ist strlen() in Ordnung,
// da die Laengenbeschraenkung auf Bytes basiert.
```

### 4. in_array mit striktem Vergleich

```php
// Bei enums sollte man strikt vergleichen:
in_array($input['priority'], ['low', 'medium', 'high'], true);
// Verhindert Typ-Mismatch-Fehler
```

### 5. JSON_ENCODE und UTF-8

```php
// Deutsche Umlaute korrekt ausgeben:
header('Content-Type: application/json; charset=utf-8');
echo json_encode($data, JSON_UNESCAPED_UNICODE);
```

---

## Vollstandiges Beispiel: createTask()

```php
function createTask(PDO $db, array $input): void {
    // Validierung aufrufen
    $errors = validateCreateTask($input);
    
    // Bei Fehlern: 400 Bad Request
    if (!empty($errors)) {
        http_response_code(400);
        header('Content-Type: application/json; charset=utf-8');
        echo json_encode(['errors' => $errors]);
        return;
    }
    
    // Ab hier: Validierung bestanden, Task erstellen
    $title = $input['title'];
    $description = $input['description'];
    $priority = $input['priority'] ?? 'medium';
    $status = 'pending';
    
    $id = generateUuid();
    
    try {
        $stmt = $db->prepare("
            INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt)
            VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ");
        $stmt->execute([$id, $title, $description, $status, $priority]);
        
        http_response_code(201);
        header('Content-Type: application/json');
        
        $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
        $stmt->execute([$id]);
        echo json_encode($stmt->fetch());
        
    } catch (PDOException $e) {
        http_response_code(500);
        header('Content-Type: application/json');
        echo json_encode(['error' => 'Failed to create task']);
    }
}
```

---

## Debugging-Tipps

### Fehler anzeigen lassen

```php
// Am Anfang der index.php:
error_reporting(E_ALL);
ini_set('display_errors', 1);

// Oder im DevContainer: Log anschauen
docker logs php-api-1
```

### Request-Body debuggen

```php
// Input debuggen:
var_dump($input);
// Oder:
error_log(print_r($input, true));
```

### Curl zum Testen

```bash
# Mit verbose Output:
curl -v -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "", "description": "Test"}'

# Response formatieren:
curl -s -X POST http://localhost:3002/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "", "description": "Test"}' | jq .
```

---

## Referenz: Gueltige Enum-Werte

### Priority (POST und PUT)
- `low`
- `medium`
- `high`

### Status (nur PUT)
- `pending`
- `in_progress`
- `completed`

---

## Weiterfuhrende Ressourcen

- [PHP empty() Dokumentation](https://www.php.net/manual/de/function.empty.php)
- [PHP strlen() Dokumentation](https://www.php.net/manual/de/function.strlen.php)
- [PHP in_array() Dokumentation](https://www.php.net/manual/de/function.in-array.php)
- [PHP PDO Dokumentation](https://www.php.net/manual/de/book.pdo.php)
