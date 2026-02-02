# PHP Pagination Hints

## Query-Parameter auslesen

```php
// page Parameter (Default: 1)
$page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;

// limit Parameter (Default: 10, Maximum: 100)
$limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 10;
```

---

## Offset berechnen

```php
// Offset ist die Anzahl der Eintraege, die uebersprungen werden
$offset = ($page - 1) * $limit;
```

**Beispiel:**
- page=1, limit=5 → offset=0 (erste 5 Eintraege)
- page=2, limit=5 → offset=5 (Eintraege 6-10)
- page=3, limit=5 → offset=10 (Eintraege 11-15)

---

## SQL mit LIMIT und OFFSET

```php
// Vorbereitete Statement mit LIMIT und OFFSET
$stmt = $db->prepare("SELECT * FROM tasks LIMIT ? OFFSET ?");
$stmt->execute([$limit, $offset]);
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);
```

**Wichtig:** Verwende immer vorbereitete Statements, um SQL-Injection zu verhindern!

---

## Gesamtanzahl ermitteln

```php
// COUNT Query fuer totalItems
$totalStmt = $db->query("SELECT COUNT(*) as total FROM tasks");
$totalResult = $totalStmt->fetch(PDO::FETCH_ASSOC);
$totalItems = $totalResult['total'];

// totalPages berechnen
$totalPages = ceil($totalItems / $limit);
```

**Mit Filter kombiniert:**

```php
// Filterbedingung aufbauen
$conditions = [];
$params = [];

if (!empty($_GET['status'])) {
    $conditions[] = "status = ?";
    $params[] = $_GET['status'];
}

if (!empty($_GET['search'])) {
    $conditions[] = "(title LIKE ? OR description LIKE ?)";
    $params[] = '%' . $_GET['search'] . '%';
    $params[] = '%' . $_GET['search'] . '%';
}

// WHERE Klausel zusammenbauen
$where = '';
if (!empty($conditions)) {
    $where = 'WHERE ' . implode(' AND ', $conditions);
}

// COUNT Query mit WHERE
$countSql = "SELECT COUNT(*) as total FROM tasks " . $where;
$totalStmt = $db->prepare($countSql);
$totalStmt->execute($params);
$totalItems = $totalStmt->fetch(PDO::FETCH_ASSOC)['total'];

// Daten Query mit LIMIT und OFFSET
$dataSql = "SELECT * FROM tasks " . $where . " LIMIT ? OFFSET ?";
$params[] = $limit;
$params[] = $offset;

$stmt = $db->prepare($dataSql);
$stmt->execute($params);
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);
```

---

## Pagination Response bauen

```php
// Response zusammenstellen
$response = [
    'data' => $tasks,
    'pagination' => [
        'page' => $page,
        'limit' => $limit,
        'totalItems' => $totalItems,
        'totalPages' => $totalPages,
        'hasNextPage' => $page < $totalPages,
        'hasPreviousPage' => $page > 1
    ]
];

// JSON Response senden
header('Content-Type: application/json');
echo json_encode($response);
```

---

## Randfälle behandeln

### page=0 oder negative Werte

```php
// Negative Werte und 0 auf 1 setzen
$page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
```

### limit=0 oder negative Werte

```php
// Negative Werte und 0 auf Standard-Limit setzen
$limit = isset($_GET['limit']) ? max(1, intval($_GET['limit'])) : 10;
```

### limit zu gross

```php
// Maximum von 100 erzwingen
$limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 10;
```

### page groesser als totalPages

```php
// page > totalPages korrigieren
if ($page > $totalPages && $totalPages > 0) {
    $page = $totalPages;
    $offset = ($page - 1) * $limit;
}
```

### Keine Ergebnisse (leere Seite)

```php
// Auch bei 0 Ergebnissen Pagination-Info zurueckgeben
$totalItems = 0;
$totalPages = 0;
$hasNextPage = false;
$hasPreviousPage = ($page > 1);
```

---

## Vollständiges Beispiel

```php
<?php
header('Content-Type: application/json');

require 'vendor/autoload.php';

use PDO;

$db = new PDO('sqlite:database.sqlite');

// Pagination Parameter auslesen
$page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
$limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 10;
$offset = ($page - 1) * $limit;

// Filter aufbauen
$conditions = [];
$params = [];

if (!empty($_GET['status'])) {
    $conditions[] = "status = ?";
    $params[] = $_GET['status'];
}

if (!empty($_GET['search'])) {
    $conditions[] = "(title LIKE ? OR description LIKE ?)";
    $params[] = '%' . $_GET['search'] . '%';
    $params[] = '%' . $_GET['search'] . '%';
}

$where = '';
if (!empty($conditions)) {
    $where = 'WHERE ' . implode(' AND ', $conditions);
}

// COUNT Query
$countSql = "SELECT COUNT(*) as total FROM tasks " . $where;
$totalStmt = $db->prepare($countSql);
$totalStmt->execute($params);
$totalItems = $totalStmt->fetch(PDO::FETCH_ASSOC)['total'];
$totalPages = $totalItems > 0 ? ceil($totalItems / $limit) : 0;

// Daten Query mit LIMIT und OFFSET
$dataSql = "SELECT * FROM tasks " . $where . " LIMIT ? OFFSET ?";
$params[] = $limit;
$params[] = $offset;

$stmt = $db->prepare($dataSql);
$stmt->execute($params);
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);

// Response bauen
$response = [
    'data' => $tasks,
    'pagination' => [
        'page' => $page,
        'limit' => $limit,
        'totalItems' => $totalItems,
        'totalPages' => $totalPages,
        'hasNextPage' => $page < $totalPages,
        'hasPreviousPage' => $page > 1
    ]
];

echo json_encode($response);
```

---

## Häufige Fehler

### 1. Falsche OFFSET-Berechnung

**Falsch:**
```php
$offset = $page * $limit;  // page 1 -> offset 5, statt 0
```

**Richtig:**
```php
$offset = ($page - 1) * $limit;  // page 1 -> offset 0
```

### 2. COUNT ohne Filter

**Falsch:**
```php
// COUNT erfasst nicht die gefilterten Ergebnisse
$totalItems = $db->query("SELECT COUNT(*) FROM tasks")->fetchColumn();
```

**Richtig:**
```php
// COUNT mit der gleichen WHERE-Klausel wie die Daten-Query
$countSql = "SELECT COUNT(*) as total FROM tasks " . $where;
```

### 3. Fehlende Validierung

**Falsch:**
```php
// Keine Pruefung auf gueltige Werte
$page = intval($_GET['page']);
$limit = intval($_GET['limit']);
```

**Richtig:**
```php
// Werte validieren und default setzen
$page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
$limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 10;
```

### 4. SQL Injection

**Falsch:**
```php
// Direkte Eingabe in SQL
$status = $_GET['status'];
$tasks = $db->query("SELECT * FROM tasks WHERE status = '$status'");
```

**Richtig:**
```php
// Prepared Statements verwenden
$stmt = $db->prepare("SELECT * FROM tasks WHERE status = ?");
$stmt->execute([$_GET['status']]);
$tasks = $stmt->fetchAll(PDO::FETCH_ASSOC);
```

---

## Debugging Tipps

1. **Response-Struktur prüfen:** Hat die Response die erwarteten Felder?
2. **totalItems prüfen:** Stimmt die Anzahl mit der Datenbank überein?
3. **Offset testen:** Werden die richtigen Tasks zurückgegeben?
4. **Randfälle:** Was passiert bei page=0, limit=0, page=999?

```php
// Debug-Ausgabe (nur fuer Entwicklung!)
error_log("Page: $page, Limit: $limit, Offset: $offset, Total: $totalItems");
```
