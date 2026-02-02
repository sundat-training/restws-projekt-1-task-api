<?php
/**
 * Task Management API - Feature 4: Pagination
 * 
 * CRUD + Validation + Filtering sind implementiert.
 * Pagination fehlt noch: GET /api/tasks?page=&limit=
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
        // 15 Beispiel-Daten für Pagination
        $tasks = [
            ['task-1', 'PHP lernen', 'PHP Grundlagen verstehen', 'completed', 'high'],
            ['task-2', 'REST API bauen', 'Task API mit PHP erstellen', 'in_progress', 'high'],
            ['task-3', 'Dokumentation schreiben', 'Alle Endpunkte dokumentieren', 'pending', 'medium'],
            ['task-4', 'Tests schreiben', 'Unit Tests für API erstellen', 'pending', 'low'],
            ['task-5', 'Deployment', 'API auf Server deployen', 'in_progress', 'medium'],
            ['task-6', 'CI/CD Pipeline', 'GitHub Actions konfigurieren', 'pending', 'high'],
            ['task-7', 'Authentifizierung', 'JWT-basierte Auth implementieren', 'pending', 'high'],
            ['task-8', 'Datenbank Schema', 'SQLite Schema designen', 'completed', 'medium'],
            ['task-9', 'Dev Umgebung', 'Docker und VSCode einrichten', 'completed', 'low'],
            ['task-10', 'Code Review', 'Code Review für PRs durchführen', 'in_progress', 'low'],
            ['task-11', 'Dependencies', 'Composer Pakete aktuell halten', 'completed', 'low'],
            ['task-12', 'Bugfixing', 'Gemeldete Issues beheben', 'pending', 'medium'],
            ['task-13', 'Performance', 'API Response Zeiten verbessern', 'pending', 'medium'],
            ['task-14', 'Logging', 'Strukturiertes Logging implementieren', 'pending', 'low'],
            ['task-15', 'API Beispiele', 'Verwendungsbeispiele schreiben', 'completed', 'low']
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
// TODO: GET /api/tasks mit Pagination
// ============================================================
// Was du tun musst:
// 1. Lese Query-Parameter aus $_GET: page, limit
// 2. Validiere und setze Defaults:
//    - page: min 1, default 1
//    - limit: min 1, max 100, default 10
// 3. Berechne offset = (page - 1) * limit
// 4. Baue SQL-Query mit LIMIT und OFFSET
// 5. Hole Gesamtanzahl (totalItems) mit COUNT(*)
// 6. Berechne totalPages = ceil(totalItems / limit)
// 7. Baue Response mit data und pagination-Objekt
//
// Erwartetes Response-Format:
// {
//   "data": [...],
//   "pagination": {
//     "page": 1,
//     "limit": 10,
//     "totalItems": 15,
//     "totalPages": 2,
//     "hasNextPage": true,
//     "hasPreviousPage": false
//   }
// }
//
// Optional: Kombiniere mit Filterung (status, priority, search)
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function getAllTasks(PDO $db): void {
    // TODO: Pagination implementieren
    // Aktuell: Einfach alle Tasks zurückgeben (ohne Pagination)
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
// BEREITS IMPLEMENTIERT - POST /api/tasks mit Validierung
// ============================================================
function createTask(PDO $db, array $input): void {
    // Validierung
    $errors = validateCreateTask($input);
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }

    // Task erstellen
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
// BEREITS IMPLEMENTIERT - PUT /api/tasks/:id mit Validierung
// ============================================================
function updateTask(PDO $db, string $id, array $input): void {
    // Validierung
    $errors = validateUpdateTask($input);
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }

    // Prüfen ob Task existiert
    $checkStmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
    $checkStmt->execute([$id]);
    $existingTask = $checkStmt->fetch();
    
    if (!$existingTask) {
        http_response_code(404);
        echo json_encode(['error' => 'Task not found']);
        return;
    }
    
    // Dynamisches UPDATE
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
    
    if (empty($updates)) {
        echo json_encode($existingTask);
        return;
    }
    
    $updates[] = 'updatedAt = CURRENT_TIMESTAMP';
    $params[] = $id;
    
    try {
        $sql = "UPDATE tasks SET " . implode(', ', $updates) . " WHERE id = ?";
        $stmt = $db->prepare($sql);
        $stmt->execute($params);
        
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
        $checkStmt = $db->prepare("SELECT COUNT(*) FROM tasks WHERE id = ?");
        $checkStmt->execute([$id]);
        $count = $checkStmt->fetchColumn();
        
        if ($count == 0) {
            http_response_code(404);
            echo json_encode(['error' => 'Task not found']);
            return;
        }
        
        $stmt = $db->prepare("DELETE FROM tasks WHERE id = ?");
        $stmt->execute([$id]);
        
        http_response_code(204);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to delete task', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// BEREITS IMPLEMENTIERT - VALIDIERUNGS-FUNKTIONEN
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
function generateUuid(): string {
    $data = random_bytes(16);
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
