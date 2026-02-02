<?php
/**
 * Task Management API - Feature 5: Authentication
 * 
 * CRUD + Validation + Filtering + Pagination sind implementiert.
 * Jetzt fehlt: Auth (Login, Middleware, User-Isolation)
 */

// CORS Headers setzen
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');

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

// Auth Header auslesen
$authHeader = $_SERVER['HTTP_AUTHORIZATION'] ?? '';

// Routing
if ($uri === '/api/auth/login' && $method === 'POST') {
    login($db, $input);
} elseif ($uri === '/api/tasks' && $method === 'GET') {
    // TODO: Auth-Middleware hinzufügen und userId verwenden
    getAllTasks($db);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'GET') {
    // TODO: Auth-Prüfung und User-Isolation
    getTask($db, $matches[1]);
} elseif ($uri === '/api/tasks' && $method === 'POST') {
    // TODO: Auth-Middleware hinzufügen
    createTask($db, $input);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'PUT') {
    // TODO: Auth-Prüfung (nur eigene Tasks dürfen geändert werden)
    updateTask($db, $matches[1], $input);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'DELETE') {
    // TODO: Auth-Prüfung (nur eigene Tasks dürfen gelöscht werden)
    deleteTask($db, $matches[1]);
} else {
    http_response_code(404);
    echo json_encode(['error' => 'Not found']);
}

// ============================================================
// DATENBANK INITIALISIERUNG
// ============================================================
function initializeDatabase(PDO $db): void {
    // Tasks table mit userId
    $db->exec("
        CREATE TABLE IF NOT EXISTS tasks (
            id TEXT PRIMARY KEY,
            title TEXT NOT NULL,
            description TEXT NOT NULL,
            status TEXT DEFAULT 'pending',
            priority TEXT DEFAULT 'medium',
            userId TEXT,
            createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
            updatedAt TEXT DEFAULT CURRENT_TIMESTAMP
        )
    ");

    // Users table für Auth
    $db->exec("
        CREATE TABLE IF NOT EXISTS users (
            id TEXT PRIMARY KEY,
            username TEXT UNIQUE NOT NULL,
            password TEXT NOT NULL,
            createdAt TEXT DEFAULT CURRENT_TIMESTAMP
        )
    ");

    // Sample Users einfügen (wenn leer)
    $userCount = $db->query("SELECT COUNT(*) FROM users")->fetchColumn();
    if ($userCount == 0) {
        $stmt = $db->prepare("INSERT INTO users (id, username, password, createdAt) VALUES (?, ?, ?, CURRENT_TIMESTAMP)");
        $stmt->execute(['user-1', 'alice', 'password123']);
        $stmt->execute(['user-2', 'bob', 'password456']);
    }

    // Sample Tasks einfügen (wenn leer)
    $taskCount = $db->query("SELECT COUNT(*) FROM tasks")->fetchColumn();
    if ($taskCount == 0) {
        $tasks = [
            ['task-1', 'PHP lernen', 'PHP Grundlagen verstehen', 'completed', 'high', 'user-1'],
            ['task-2', 'REST API bauen', 'Task API mit PHP erstellen', 'in_progress', 'high', 'user-1'],
            ['task-3', 'Dokumentation schreiben', 'Alle Endpunkte dokumentieren', 'pending', 'medium', 'user-1'],
            ['task-4', 'Tests schreiben', 'Unit Tests für API erstellen', 'pending', 'low', 'user-2'],
            ['task-5', 'Deployment', 'API auf Server deployen', 'in_progress', 'medium', 'user-2']
        ];

        $stmt = $db->prepare("
            INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt)
            VALUES (?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ");

        foreach ($tasks as $task) {
            $stmt->execute($task);
        }
    }
}

// ============================================================
// TODO AUFGABE 1: Login Endpunkt implementieren
// ============================================================
// Was du tun musst:
// 1. Extrahiere username und password aus $input
// 2. Suche User in der Datenbank: SELECT * FROM users WHERE username = ?
// 3. Prüfe ob Passwort übereinstimmt (plaintext comparison für dieses Lab)
// 4. Bei Erfolg: return 200 mit { userId, username }
// 5. Bei Fehler: return 401 Unauthorized mit { error: 'Invalid credentials' }
//
// Test-User:
// - alice / password123
// - bob / password456
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function login(PDO $db, array $input): void {
    // TODO AUFGABE 1: Login implementieren
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet - implement login here']);
}

// ============================================================
// TODO AUFGABE 2: Auth-Middleware und User-Isolation
// ============================================================
// Was du tun musst:
// 1. Auth-Middleware Funktion erstellen:
//    - Lese Authorization Header aus $_SERVER['HTTP_AUTHORIZATION']
//    - Prüfe auf "Bearer user-X" Format
//    - Bei fehlendem/ungültigem Auth: return 401
//    - Bei Erfolg: return userId aus dem Token
//
// 2. In getAllTasks(): Zeige nur Tasks wo userId = eingeloggter User
//    - Füge WHERE userId = ? zur SQL-Query hinzu
//
// 3. In getTask(): Prüfe ob Task dem eingeloggten User gehört
//    - Wenn nicht: return 403 Forbidden
//
// 4. In createTask(): Setze userId des eingeloggten Users
//    - Füge userId zum INSERT Statement hinzu
//
// 5. In updateTask() und deleteTask(): Prüfe Eigentümerschaft
//    - SELECT task mit id UND userId
//    - Wenn nicht gefunden: return 403 (oder 404 - entscheide selbst)
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================

// BEREITS IMPLEMENTIERT - GET Alle Tasks (TODO: Auth hinzufügen)
function getAllTasks(PDO $db): void {
    try {
        // TODO: WHERE userId = ? hinzufügen
        $stmt = $db->query("SELECT * FROM tasks");
        $tasks = $stmt->fetchAll();
        echo json_encode($tasks);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to fetch tasks', 'message' => $e->getMessage()]);
    }
}

// BEREITS IMPLEMENTIERT - GET Einzelner Task (TODO: Auth-Prüfung)
function getTask(PDO $db, string $id): void {
    try {
        // TODO: Auth-Prüfung hinzufügen (nur eigene Tasks anzeigen)
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

// BEREITS IMPLEMENTIERT - POST /api/tasks (TODO: Auth + userId)
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
    
    // TODO: userId des eingeloggten Users holen und hinzufügen
    $userId = null; // Hier sollte der eingeloggte User stehen
    
    try {
        $stmt = $db->prepare("
            INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt)
            VALUES (?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
        ");
        $stmt->execute([$id, $title, $description, $status, $priority, $userId]);
        
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

// BEREITS IMPLEMENTIERT - PUT /api/tasks/:id (TODO: Auth-Prüfung)
function updateTask(PDO $db, string $id, array $input): void {
    // Validierung
    $errors = validateUpdateTask($input);
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }

    // TODO: Auth-Prüfung (nur eigene Tasks dürfen geändert werden)
    
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

// BEREITS IMPLEMENTIERT - DELETE /api/tasks/:id (TODO: Auth-Prüfung)
function deleteTask(PDO $db, string $id): void {
    try {
        // TODO: Auth-Prüfung (nur eigene Tasks dürfen gelöscht werden)
        
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
