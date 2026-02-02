# Hinweise zur Implementierung - Feature 1: Basics

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Aufgaben in `index.php`.
Die Beispiele zeigen einen Lösungsansatz - es gibt oft mehrere korrekte Wege zum Ziel.

---

## Aufgabe 1: POST /api/tasks implementieren

### Lösungsansatz

1. Extrahiere JSON-Daten aus dem Request-Body
2. Validiere und setze Default-Werte für `title`, `priority`, `status`
3. Generiere eine neue UUID als ID
4. Füge den Task in die SQLite-Datenbank ein
5. Lese den erstellten Task zurück und gebe ihn mit Status `201 Created` zurück

### Wichtige Hinweise

- Verwende `php://input` um JSON-Daten zu lesen
- Priority hat Default-Wert "medium" wenn nicht angegeben
- Status hat Default-Wert "pending" wenn nicht angegeben
- Behandle Datenbankfehler mit 500 Status

### Code-Beispiel

```php
// JSON-Daten aus dem Request-Body extrahieren
$input = json_decode(file_get_contents('php://input'), true) ?? [];

// Felder auslesen und Defaults setzen
$title = $input['title'] ?? '';
$description = $input['description'] ?? '';
$priority = $input['priority'] ?? 'medium';
$status = 'pending';

// UUID generieren
$id = generateUuid();

// Task in Datenbank einfügen
$stmt = $db->prepare("
    INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt)
    VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
");
$stmt->execute([$id, $title, $description, $status, $priority]);

// Den neuen Task zurückholen
$stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
$stmt->execute([$id]);
$task = $stmt->fetch();

// Response senden
http_response_code(201);
echo json_encode($task);
```

### UUID-Hilfsfunktion

```php
function generateUuid(): string {
    $data = random_bytes(16);
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
```

---

## Aufgabe 2: PUT /api/tasks/:id implementieren

### Lösungsansatz

1. Hole `id` aus dem URL-Parameter
2. Extrahiere die zu aktualisierenden Felder aus dem Request-Body
3. Baue dynamisch das UPDATE-Statement (nur übergebene Felder)
4. Füge immer `updatedAt = CURRENT_TIMESTAMP` hinzu
5. Prüfe ob der Task existiert (404 wenn nicht)
6. Gebe den aktualisierten Task zurück

### Wichtige Hinweise

- Nur übergebene Felder aktualisieren (nicht alles)
- Prüfe ob mindestens ein Feld zum Aktualisieren vorhanden ist
- Verwende `rowCount()` um zu prüfen ob etwas aktualisiert wurde
- Aktualisiere immer `updatedAt`

### Code-Beispiel

```php
// ID aus der URL holen
$id = $params['id'] ?? '';

// JSON-Daten aus dem Request-Body extrahieren
$input = json_decode(file_get_contents('php://input'), true) ?? [];

// Dynamisches UPDATE-Statement aufbauen
$updates = [];
$params = [];

if (isset($input['title'])) {
    $updates[] = 'title = ?';
    $params[] = $input['title'];
}
if (isset($input['description'])) {
    $updates[] = 'description = ?';
    $params[] = $input['description'];
}
if (isset($input['status'])) {
    $updates[] = 'status = ?';
    $params[] = $input['status'];
}
if (isset($input['priority'])) {
    $updates[] = 'priority = ?';
    $params[] = $input['priority'];
}

// Prüfen ob überhaupt Felder zum Aktualisieren vorhanden sind
if (count($updates) === 0) {
    http_response_code(400);
    echo json_encode(['error' => 'No fields to update']);
    exit;
}

// updatedAt immer aktualisieren
$updates[] = 'updatedAt = CURRENT_TIMESTAMP';
$params[] = $id; // für WHERE id = ?

// SQL-Statement zusammenbauen
$sql = "UPDATE tasks SET " . implode(', ', $updates) . " WHERE id = ?";

// UPDATE ausführen
$stmt = $db->prepare($sql);
$stmt->execute($params);

// Prüfen ob der Task gefunden wurde
if ($stmt->rowCount() === 0) {
    http_response_code(404);
    echo json_encode(['error' => 'Task not found']);
    exit;
}

// Aktualisierten Task zurückholen
$stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
$stmt->execute([$id]);
$task = $stmt->fetch();

echo json_encode($task);
```

### Alternative: Einfache Variante (nur Status aktualisieren)

```php
$id = $params['id'] ?? '';
$input = json_decode(file_get_contents('php://input'), true) ?? [];
$status = $input['status'] ?? '';

$stmt = $db->prepare("
    UPDATE tasks SET status = ?, updatedAt = CURRENT_TIMESTAMP WHERE id = ?
");
$stmt->execute([$status, $id]);

if ($stmt->rowCount() === 0) {
    http_response_code(404);
    echo json_encode(['error' => 'Task not found']);
    exit;
}

$stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
$stmt->execute([$id]);
$task = $stmt->fetch();

echo json_encode($task);
```

---

## Aufgabe 3: DELETE /api/tasks/:id implementieren

### Lösungsansatz

1. Hole `id` aus dem URL-Parameter
2. Lösche den Task aus der Datenbank
3. Prüfe mit `rowCount()` ob ein Task gelöscht wurde
4. Gebe `204 No Content` bei Erfolg zurück
5. Gebe `404` zurück wenn der Task nicht gefunden wurde

### Wichtige Hinweise

- DELETE gibt typischerweise 204 No Content zurück (kein Body)
- Verwende `rowCount()` um zu prüfen ob die Löschung erfolgreich war
- Kein SELECT nötig vor dem DELETE

### Code-Beispiel

```php
// ID aus der URL holen
$id = $params['id'] ?? '';

// DELETE ausführen
$stmt = $db->prepare("DELETE FROM tasks WHERE id = ?");
$stmt->execute([$id]);

// Prüfen ob der Task gefunden und gelöscht wurde
if ($stmt->rowCount() === 0) {
    http_response_code(404);
    echo json_encode(['error' => 'Task not found']);
    exit;
}

// 204 No Content = Erfolg, kein Body
http_response_code(204);
```

---

## Nützliche PHP-Funktionen

### JSON Input verarbeiten

```php
$input = json_decode(file_get_contents('php://input'), true);
if ($input === null) {
    http_response_code(400);
    echo json_encode(['error' => 'Invalid JSON']);
    exit;
}
```

### PDO Fetch-Muster

```php
// Einzelne Zeile holen
$stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
$stmt->execute([$id]);
$task = $stmt->fetch();

// Alle Zeilen holen
$stmt = $db->prepare("SELECT * FROM tasks");
$stmt->execute();
$tasks = $stmt->fetchAll();
```

### HTTP Status Codes setzen

```php
http_response_code(201); // Created
http_response_code(200); // OK
http_response_code(204); // No Content
http_response_code(400); // Bad Request
http_response_code(404); // Not Found
http_response_code(500); // Internal Server Error
```

### JSON Response senden

```php
header('Content-Type: application/json');
echo json_encode($task);
```

---

## Häufige Fehler vermeiden

1. **updatedAt vergessen:** Bei PUT muss `updatedAt` immer aktualisiert werden
2. **JSON parsen:** `file_get_contents('php://input')` gibt einen String zurück - `json_decode()` ist nötig
3. **Status Codes:**
   - 201 für POST (Created)
   - 200 für PUT/GET (OK)
   - 204 für DELETE (No Content)
   - 404 wenn nicht gefunden
   - 500 bei Datenbankfehlern
4. **rowCount():** Funktioniert bei PDO-Statements nach `execute()` um die Anzahl betroffener Zeilen zu ermitteln
5. **isset() vs null:** Bei optionalen Feldern `isset()` verwenden um zwischen "nicht gesendet" und "absichtlich null" zu unterscheiden
6. **SQL Injection:** Immer Prepared Statements mit `?` Platzhaltern verwenden
