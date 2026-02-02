<?php
/**
 * Task Management API - Feature 3: Filtering
 * 
 * CRUD + Validation sind implementiert.
 * Filtering fehlt noch: GET /api/tasks?status=&priority=&search=
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
        // Mehr Beispiel-Daten für Filtering
        $tasks = [
            ['task-1', 'PHP lernen', 'PHP Grundlagen verstehen', 'completed', 'high'],
            ['task-2', 'REST API bauen', 'Task API mit PHP erstellen', 'in_progress', 'high'],
            ['task-3', 'Dokumentation schreiben', 'Alle Endpunkte dokumentieren', 'pending', 'medium'],
            ['task-4', 'Tests schreiben', 'Unit Tests für API erstellen', 'pending', 'low'],
            ['task-5', 'Deployment', 'API auf Server deployen', 'in_progress', 'medium']
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
// TODO AUFGABE 1-3: GET /api/tasks mit Query-Parameter-Filtering
// ============================================================
// Was du tun musst:
// 1. Lese Query-Parameter aus $_GET: status, priority, search
// 2. Baue die SQL-Query dynamisch auf:
//    - Basis: SELECT * FROM tasks
//    - Wenn Filter vorhanden: WHERE clauses hinzufügen
//    - Mehrere Filter mit AND verbinden
// 3. Parameterized Queries verwenden (Sicherheit!)
//
// Filter-Typen:
// - ?status=pending - Exakter Match auf status Spalte
// - ?priority=high - Exakter Match auf priority Spalte  
// - ?search=api - Suche in title UND description mit LIKE '%api%'
// - Kombinationen: ?status=pending&priority=high&search=test
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function getAllTasks(PDO $db): void {
    // TODO AUFGABE: Query-Parameter auswerten und dynamische SQL bauen
    // HIER DEIN CODE FÜR FILTERING
    
    // Aktuell: Einfach alle Tasks zurückgeben (ohne Filtering)
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
    
    // priority validieren (optional, aber wenn angegeben muss es gültig sein)
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
    
    // Alle Felder sind optional bei PUT, aber wenn angegeben, müssen sie gültig sein
    
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
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40); // Version 4
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80); // Variant 10
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
