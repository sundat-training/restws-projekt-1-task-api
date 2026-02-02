# HINTS.md - PHP-spezifische Hinweise fur Feature 3: Filtering

## Query-Parameter in PHP auslesen

In PHP werden Query-Parameter (URL-Parameter nach dem `?`) im superglobalen Array `$_GET` gespeichert.

```php
// Einfaches Auslesen
if (isset($_GET['status'])) {
    $status = $_GET['status'];
}

// Alle moglichen Parameter auf einmal
$status = $_GET['status'] ?? null;
$priority = $_GET['priority'] ?? null;
$search = $_GET['search'] ?? null;
```

### Wichtige Unterschiede zu TypeScript

| Aspekt | TypeScript (Express) | PHP |
|--------|---------------------|-----|
| Parameter auslesen | `req.query.status` | `$_GET['status']` |
| Prufung vorhanden | `if (status)` | `if (isset($_GET['status']))` |
| Default-Wert | `req.query.status ?? null` | `$_GET['status'] ?? null` |

---

## Dynamischen SQL-Query aufbauen

Das Herzstuck der Filterung ist das dynamische Zusammenbauen des SQL-Querys. Hier ist das grundlegende Muster:

```php
$query = "SELECT * FROM tasks";
$conditions = [];
$params = [];

// Status-Filter
if (isset($_GET['status'])) {
    $conditions[] = "status = ?";
    $params[] = $_GET['status'];
}

// Priority-Filter
if (isset($_GET['priority'])) {
    $conditions[] = "priority = ?";
    $params[] = $_GET['priority'];
}

// WHERE-Klausel nur hinzufugen wenn es Bedingungen gibt
if (!empty($conditions)) {
    $query .= " WHERE " . implode(' AND ', $conditions);
}

// Query ausfuhren
$stmt = $db->prepare($query);
$stmt->execute($params);
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);
```

### Warum `implode(' AND ', $conditions)`?

Diese Methode ist flexibel und sicher:

1. **Sicher:** Jeder Parameter wird als Prepared Statement mit `?` gebunden - keine SQL-Injection moglich

2. **Flexibel:** Egal ob du 0, 1, 2 oder 3 Filter hast, der Query wird immer korrekt aufgebaut

3. **Lesbar:** Die Bedingungen werden spater mit `AND` verbunden

### Beispiele fur verschiedene Kombinationen

```php
// Kein Filter
// Query: SELECT * FROM tasks

// Nur status
// Query: SELECT * FROM tasks WHERE status = ?
// Params: ['pending']

// Nur priority
// Query: SELECT * FROM tasks WHERE priority = ?
// Params: ['high']

// Beide
// Query: SELECT * FROM tasks WHERE status = ? AND priority = ?
// Params: ['pending', 'high']
```

---

## Suche mit LIKE

Die Suche erfordert eine spezielle SQL-Syntax mit dem `LIKE` Operator und Wildcards:

```php
if (isset($_GET['search'])) {
    $conditions[] = "(title LIKE ? OR description LIKE ?)";
    $searchPattern = "%" . $_GET['search'] . "%";
    $params[] = $searchPattern;
    $params[] = $searchPattern;
}
```

### Wildcards erklarung

- `%` bedeutet "beliebige Zeichenfolge" (auch leer)
- `?` bedeutet "genau ein Zeichen" (wird hier NICHT verwendet)

**Beispiele:**
- `'%API%'` findet alles was "API" irgendwo enthalt
- `'API%'` findet alles was mit "API" beginnt
- `'%API'` findet alles was mit "API" endet

### Case-Sensitivity (Gro-/Kleinschreibung)

**Gute Nachricht:** SQLite ist standardmaig **case-insensitive** fur LIKE-Suchen.

```php
// Das findet "API", "api", "Api", usw.
$searchPattern = "%" . $_GET['search'] . "%";
```

Wenn du case-sensitive suchen mochtest (was in diesem Projekt NICHT gewunscht ist), musst du `COLLATE NOCASE` verwenden oder die Vergleichsfunktionen anpassen.

---

## Vollstandiges Beispiel fur den GET /api/tasks Endpunkt

```php
case 'GET':
    if (preg_match('#/api/tasks$#', $uri)) {
        try {
            $query = "SELECT * FROM tasks";
            $conditions = [];
            $params = [];

            // Filter nach Status
            if (isset($_GET['status'])) {
                $conditions[] = "status = ?";
                $params[] = $_GET['status'];
            }

            // Filter nach Priority
            if (isset($_GET['priority'])) {
                $conditions[] = "priority = ?";
                $params[] = $_GET['priority'];
            }

            // Suche
            if (isset($_GET['search'])) {
                $conditions[] = "(title LIKE ? OR description LIKE ?)";
                $searchPattern = "%" . $_GET['search'] . "%";
                $params[] = $searchPattern;
                $params[] = $searchPattern;
            }

            // WHERE-Klausel hinzufugen wenn Bedingungen vorhanden
            if (!empty($conditions)) {
                $query .= " WHERE " . implode(' AND ', $conditions);
            }

            // Query ausfuhren
            $stmt = $db->prepare($query);
            $stmt->execute($params);
            $tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);

            http_response_code(200);
            echo json_encode($tasks);

        } catch (PDOException $e) {
            http_response_code(500);
            echo json_encode(['error' => $e->getMessage()]);
        }
    }
    break;
```

---

## Haufige Fehler und wie man sie vermeidet

### Fehler 1: isset() vergessen

```php
// FALSCH - kann Notice-Warnung erzeugen
if ($_GET['status']) { }

// RICHTIG
if (isset($_GET['status'])) { }
```

### Fehler 2: empty() statt isset()

```php
// PROBLEM: empty() ist TRUE fur "0", was ein gultiger Status sein konnte
if (!empty($_GET['status'])) { }

// BESSER: isset() kombiniert mit Pruefung auf leeren String
if (isset($_GET['status']) && $_GET['status'] !== '') { }

// AM BESTEN: isset() reicht fur dieses Projekt
if (isset($_GET['status'])) { }
```

### Fehler 3: SQL-Injection durch String-Concatenation

```php
// SEHR FAHRLASSIG - NIEMALS SO MACHEN!
$query = "SELECT * FROM tasks WHERE status = '$status'";

// SICHER - Immer Prepared Statements verwenden
$stmt = $db->prepare("SELECT * FROM tasks WHERE status = ?");
$stmt->execute([$status]);
```

### Fehler 4: LIKE-Wildcards vergessen

```php
// FALSCH - findet nur exakte Ubereinstimmung
$conditions[] = "title LIKE ?";
$params[] = $_GET['search'];

// RICHTIG - Wildcards fur Teilstring-Suche
$conditions[] = "title LIKE ?";
$params[] = "%" . $_GET['search'] . "%";
```

### Fehler 5: Falsche Reihenfolge der Parameter

```php
// PROBLEM: Params werden in anderer Reihenfolge an PDO uebergeben
$conditions[] = "status = ?";
$conditions[] = "(title LIKE ? OR description LIKE ?)";
$params = [$status, $searchPattern, $searchPattern]; // Passt

// ABER: Wenn du die Bedingungen in anderer Reihenfolge hinzufugst...
if (isset($_GET['search'])) {
    $conditions[] = "(title LIKE ? OR description LIKE ?)";
}
if (isset($_GET['status'])) {
    $conditions[] = "status = ?";
}
// ...dann muss die Reihenfolge in $params auch angepasst werden!
// Besser: Immer in derselben Reihenfolge hinzufugen
```

### Fehler 6: Fehlendes ELSE oder Fehlerbehandlung

```php
// PROBLEM: Wenn keine Tasks gefunden werden, wird nichts zuruckgegeben
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);
echo json_encode($tasks);

// BESSER: Immer ein Array zuruckgeben, auch wenn leer
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC) ?: [];
http_response_code(200);
echo json_encode($tasks);
```

---

## PDO Prepared Statements in PHP

PHP verwendet PDO (PHP Data Objects) fur Datenbankzugriffe:

```php
// Verbindung aufbauen
$db = new PDO('sqlite:database.db');
$db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

// Statement vorbereiten
$stmt = $db->prepare("SELECT * FROM tasks WHERE status = ?");

// Parameter binden und ausfuhren
$stmt->execute([$status]);

// Ergebnisse holen
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);
```

### Vorteile von Prepared Statements

1. **Sicherheit:** Schutzt vor SQL-Injection
2. **Performance:** Query wird einmal kompiliert
3. **Typ-Prufung:** Parameter werden korrekt behandelt

---

## Debugging-Tipps

### 1. Den gebauten Query anzeigen (fur Debugging)

```php
$query = "SELECT * FROM tasks";
// ... Bedingungen hinzufugen ...

// Zum Debuggen: Query und Params ausgeben
error_log("Query: " . $query);
error_log("Params: " . print_r($params, true));

$stmt = $db->prepare($query);
// ...
```

### 2. Errors abfangen

```php
try {
    // Datenbank-Code
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => $e->getMessage()]);
}
```

### 3. SQL-Befehle direkt testen

```bash
# SQLite Datenbank offnen
sqlite3 database.db

# Alle Tasks anzeigen
SELECT * FROM tasks;

# Filter testen
SELECT * FROM tasks WHERE status = 'pending';

# Kombinierte Filter testen
SELECT * FROM tasks WHERE status = 'pending' AND priority = 'high';

# Suche testen
SELECT * FROM tasks WHERE title LIKE '%API%' OR description LIKE '%API%';
```

---

## Referenz: Alle moglichen Query-Kombinationen

| URL | Query | Params |
|-----|-------|--------|
| `/api/tasks` | `SELECT * FROM tasks` | `[]` |
| `/api/tasks?status=pending` | `SELECT * FROM tasks WHERE status = ?` | `['pending']` |
| `/api/tasks?priority=high` | `SELECT * FROM tasks WHERE priority = ?` | `['high']` |
| `/api/tasks?search=API` | `SELECT * FROM tasks WHERE (title LIKE ? OR description LIKE ?)` | `['%API%', '%API%']` |
| `/api/tasks?status=pending&priority=high` | `SELECT * FROM tasks WHERE status = ? AND priority = ?` | `['pending', 'high']` |
| `/api/tasks?status=pending&search=docs` | `SELECT * FROM tasks WHERE status = ? AND (title LIKE ? OR description LIKE ?)` | `['pending', '%docs%', '%docs%']` |

---

## Nachte: PHP isset() vs empty()

| Ausdruck | isset() | empty() |
|----------|---------|---------|
| `$_GET['status']` nicht vorhanden | false | true |
| `$_GET['status'] = ""` | true | true |
| `$_GET['status'] = "pending"` | true | false |
| `$_GET['status'] = "0"` | true | true |

**Fur dieses Projekt:** `isset()` ist die richtige Wahl, da wir auch nach dem Status "0" suchen konnten (falls das jemals notig ware).

---

## Zusammenfassung der SQL-Patterns

```php
// Pattern 1: Einzelne Bedingung
if (isset($_GET['status'])) {
    $conditions[] = "status = ?";
    $params[] = $_GET['status'];
}

// Pattern 2: LIKE-Suche (immer mit Wildcards)
if (isset($_GET['search'])) {
    $conditions[] = "(title LIKE ? OR description LIKE ?)";
    $params[] = "%" . $_GET['search'] . "%";
    $params[] = "%" . $_GET['search'] . "%";
}

// Pattern 3: WHERE zusammenbauen
if (!empty($conditions)) {
    $query .= " WHERE " . implode(' AND ', $conditions);
}
```

Diese drei Patterns kannst du kombinieren und wiederholen, um jeden gewunschten Filter zu implementieren.
