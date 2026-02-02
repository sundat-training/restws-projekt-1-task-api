<?php
/**
 * Task Management API - Feature 2: Validation
 * 
 * CRUD-Endpunkte sind implementiert, Validierung fehlt noch.
 * POST und PUT brauchen Validierung für die Eingabedaten.
 */

// CORS Headers setzen
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

// Preflight-Requests beantworten
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

// Datenbankverbindung herstellen
$dbPath = __DIR__ . '/task-api.db';
try {
    $db = new PDO('sqlite:' . $dbPath);
    $db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    $db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Datenbankverbindung fehlgeschlagen', 'message' => $e->getMessage()]);
    exit;
}

// Datenbank initialisieren
initializeDatabase($db);

// Request-Daten parsen
$method = $_SERVER['REQUEST_METHOD'];
$uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);
$input = json_decode(file_get_contents('php://input'), true) ?? [];

// Routing
if ($uri === '/api/tasks' && $method === 'GET') {
    getAllTasks($db);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'GET') {
    getTask($db, $matches[1]);
} elseif ($uri === '/api/tasks' && $method === 'POST') {
    createTask($db, $input);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'PUT') {
    updateTask($db, $matches[1], $input);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'DELETE') {
    deleteTask($db, $matches[1]);
} else {
    http_response_code(404);
    echo json_encode(['error' => 'Not found']);
}

// ============================================================
// DATENBANK INITIALISIERUNG
// ============================================================
function initializeDatabase(PDO $db): void {
    // Tabelle erstellen
    $db->exec("
        CREATE TABLE IF NOT EXISTS tasks (
            id TEXT PRIMARY KEY,
            title TEXT NOT NULL,
            description TEXT NOT NULL,
            status TEXT DEFAULT 'pending',
            priority TEXT DEFAULT 'medium',
            createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
            updatedAt TEXT DEFAULT CURRENT_TIMESTAMP
        )
    ");

    // Prüfen ob bereits Daten vorhanden sind
    $count = $db->query("SELECT COUNT(*) FROM tasks")->fetchColumn();
    
    if ($count == 0) {
        // Beispiel-Daten einfügen
        $tasks = [
            ['task-1', 'PHP lernen', 'PHP Grundlagen verstehen', 'completed', 'high'],
            ['task-2', 'REST API bauen', 'Task API mit PHP erstellen', 'in_progress', 'high'],
            ['task-3', 'Dokumentation schreiben', 'Alle Endpunkte dokumentieren', 'pending', 'medium']
        ];

        $stmt = $db->prepare("
            INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt)
            VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ");

        foreach ($tasks as $task) {
            $stmt->execute($task);
        }
    }
}

// ============================================================
// BEREITS IMPLEMENTIERT - GET Alle Tasks
// ============================================================
function getAllTasks(PDO $db): void {
    try {
        $stmt = $db->query("SELECT * FROM tasks");
        $tasks = $stmt->fetchAll();
        echo json_encode($tasks);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to fetch tasks', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// BEREITS IMPLEMENTIERT - GET Einzelner Task
// ============================================================
function getTask(PDO $db, string $id): void {
    try {
        $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
        $stmt->execute([$id]);
        $task = $stmt->fetch();

        if ($task) {
            echo json_encode($task);
        } else {
            http_response_code(404);
            echo json_encode(['error' => 'Task not found']);
        }
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to fetch task', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// TODO AUFGABE 1: POST /api/tasks mit Validierung
// ============================================================
// Was du tun musst:
// 1. Erstelle eine Validierungs-Funktion für POST-Daten
//    - title: Pflichtfeld, max 200 Zeichen
//    - description: Pflichtfeld
//    - priority: Optional, muss 'low', 'medium' oder 'high' sein
// 2. Rufe die Validierung am Anfang der Funktion auf
// 3. Bei Fehlern: return 400 Bad Request mit errors-Array
// 4. Bei Erfolg: Task erstellen wie bisher
//
// Erwartetes Fehler-Format:
// {
//   "errors": [
//     {
//       "type": "field",
//       "msg": "Title is required",
//       "path": "title",
//       "location": "body"
//     }
//   ]
// }
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function createTask(PDO $db, array $input): void {
    // TODO AUFGABE 1: Validierung implementieren
    // 1. $errors = validateCreateTask($input);
    // 2. if (!empty($errors)) { http_response_code(400); echo json_encode(['errors' => $errors]); return; }
    // 3. Dann Task erstellen (Code ist bereits unten implementiert)

    // BEREITS IMPLEMENTIERT - Task erstellen (aus Feature 1)
    $title = $input['title'] ?? '';
    $description = $input['description'] ?? '';
    $priority = $input['priority'] ?? 'medium';
    
    // Status wird immer auf 'pending' gesetzt
    $status = 'pending';
    
    // UUID generieren
    $id = generateUuid();
    
    try {
        $stmt = $db->prepare("
            INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt)
            VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ");
        $stmt->execute([$id, $title, $description, $status, $priority]);
        
        // Neuen Task zurückgeben
        $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
        $stmt->execute([$id]);
        $newTask = $stmt->fetch();
        
        http_response_code(201);
        echo json_encode($newTask);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to create task', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// TODO AUFGABE 2: PUT /api/tasks/:id mit Validierung
// ============================================================
// Was du tun musst:
// 1. Erstelle eine Validierungs-Funktion für PUT-Daten (alle optional):
//    - title: Optional, max 200 Zeichen
//    - status: Optional, muss 'pending', 'in_progress' oder 'completed' sein
//    - priority: Optional, muss 'low', 'medium' oder 'high' sein
// 2. Rufe die Validierung am Anfang der Funktion auf
// 3. Bei Fehlern: return 400 Bad Request mit errors-Array
// 4. Bei Erfolg: Task aktualisieren wie bisher
// 5. Prüfe ob Task existiert (404 wenn nicht)
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function updateTask(PDO $db, string $id, array $input): void {
    // TODO AUFGABE 2: Validierung implementieren
    // 1. $errors = validateUpdateTask($input);
    // 2. if (!empty($errors)) { http_response_code(400); echo json_encode(['errors' => $errors]); return; }
    // 3. Dann Task aktualisieren (Code ist bereits unten implementiert)

    // BEREITS IMPLEMENTIERT - Task aktualisieren (aus Feature 1)
    
    // Prüfen ob Task existiert
    $checkStmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
    $checkStmt->execute([$id]);
    $existingTask = $checkStmt->fetch();
    
    if (!$existingTask) {
        http_response_code(404);
        echo json_encode(['error' => 'Task not found']);
        return;
    }
    
    // Dynamisches UPDATE bauen
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
    
    // Wenn keine Felder zum Updaten
    if (empty($updates)) {
        // Task zurückgeben ohne Änderung
        echo json_encode($existingTask);
        return;
    }
    
    // updatedAt hinzufügen
    $updates[] = 'updatedAt = CURRENT_TIMESTAMP';
    $params[] = $id;
    
    try {
        $sql = "UPDATE tasks SET " . implode(', ', $updates) . " WHERE id = ?";
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        
        // Aktualisierten Task zurückgeben
        $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
        $stmt->execute([$id]);
        $updatedTask = $stmt->fetch();
        
        echo json_encode($updatedTask);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to update task', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// BEREITS IMPLEMENTIERT - DELETE /api/tasks/:id
// ============================================================
function deleteTask(PDO $db, string $id): void {
    try {
        // Prüfen ob Task existiert
        $checkStmt = $db->prepare("SELECT COUNT(*) FROM tasks WHERE id = ?");
        $checkStmt->execute([$id]);
        $count = $checkStmt->fetchColumn();
        
        if ($count == 0) {
            http_response_code(404);
            echo json_encode(['error' => 'Task not found']);
            return;
        }
        
        // Task löschen
        $stmt = $db->prepare("DELETE FROM tasks WHERE id = ?");
        $stmt->execute([$id]);
        
        http_response_code(204);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to delete task', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// TODO: VALIDIERUNGS-FUNKTIONEN
// ============================================================
// Erstelle hier deine Validierungs-Funktionen:
//
// function validateCreateTask(array $input): array {
//     $errors = [];
//     
//     // title validieren
//     if (empty($input['title'])) {
//         $errors[] = [
//             'type' => 'field',
//             'msg' => 'Title is required',
//             'path' => 'title',
//             'location' => 'body'
//         ];
//     } elseif (strlen($input['title']) > 200) {
//         $errors[] = [
//             'type' => 'field',
//             'msg' => 'Title max 200 chars',
//             'path' => 'title',
//             'location' => 'body'
//         ];
//     }
//     
//     // ... weitere Validierungen
//     
//     return $errors;
// }
//
// function validateUpdateTask(array $input): array {
//     // Ähnlich wie validateCreateTask, aber alle Felder optional
// }
// ============================================================

// ============================================================
// Hilfsfunktion: UUID generieren
// ============================================================
function generateUuid(): string {
    // Generiert einen RFC 4122 kompatiblen UUID
    $data = random_bytes(16);
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40); // Version 4
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80); // Variant 10
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
