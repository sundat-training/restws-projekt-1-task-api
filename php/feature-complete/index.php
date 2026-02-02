<?php
/**
 * Task Management API - Feature Complete
 * 
 * Vollständige Implementierung mit:
 * - CRUD-Operationen für Tasks
 * - Request-Validierung
 * - Query-Filtering (status, priority, search)
 * - Pagination
 * - Authentifizierung mit Bearer Token
 */

// ============================================================
// KONFIGURATION
// ============================================================
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type, Authorization');

// Preflight-Requests beantworten
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

// ============================================================
// DATENBANK INITIALISIERUNG
// ============================================================
$dbPath = __DIR__ . '/task-api.db';
try {
    $db = new PDO('sqlite:' . $dbPath);
    $db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    $db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
    initializeDatabase($db);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Datenbankverbindung fehlgeschlagen', 'message' => $e->getMessage()]);
    exit;
}

function initializeDatabase(PDO $db): void {
    // Users table für Auth
    $db->exec("
        CREATE TABLE IF NOT EXISTS users (
            id TEXT PRIMARY KEY,
            username TEXT UNIQUE NOT NULL,
            password TEXT NOT NULL,
            createdAt TEXT DEFAULT CURRENT_TIMESTAMP
        )
    ");

    // Tasks table mit userId für User-Isolation
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

    // Sample Users
    $userCount = $db->query("SELECT COUNT(*) FROM users")->fetchColumn();
    if ($userCount == 0) {
        $stmt = $db->prepare("INSERT INTO users (id, username, password, createdAt) VALUES (?, ?, ?, CURRENT_TIMESTAMP)");
        $stmt->execute(['user-1', 'alice', 'password123']);
        $stmt->execute(['user-2', 'bob', 'password456']);
    }

    // Sample Tasks
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
// REQUEST PARSING
// ============================================================
$method = $_SERVER['REQUEST_METHOD'];
$uri = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH);
$input = json_decode(file_get_contents('php://input'), true) ?? [];
$authHeader = $_SERVER['HTTP_AUTHORIZATION'] ?? '';

// ============================================================
// ROUTING
// ============================================================
if ($uri === '/api/auth/login' && $method === 'POST') {
    login($db, $input);
} elseif ($uri === '/api/tasks' && $method === 'GET') {
    $userId = authenticate($authHeader);
    getAllTasks($db, $userId);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'GET') {
    $userId = authenticate($authHeader);
    getTask($db, $matches[1], $userId);
} elseif ($uri === '/api/tasks' && $method === 'POST') {
    $userId = authenticate($authHeader);
    createTask($db, $input, $userId);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'PUT') {
    $userId = authenticate($authHeader);
    updateTask($db, $matches[1], $input, $userId);
} elseif (preg_match('#^/api/tasks/([^/]+)$#', $uri, $matches) && $method === 'DELETE') {
    $userId = authenticate($authHeader);
    deleteTask($db, $matches[1], $userId);
} else {
    http_response_code(404);
    echo json_encode(['error' => 'Not found']);
}

// ============================================================
// AUTHENTICATION
// ============================================================
function login(PDO $db, array $input): void {
    $username = $input['username'] ?? '';
    $password = $input['password'] ?? '';

    $stmt = $db->prepare("SELECT * FROM users WHERE username = ?");
    $stmt->execute([$username]);
    $user = $stmt->fetch();

    if (!$user || $user['password'] !== $password) {
        http_response_code(401);
        echo json_encode(['error' => 'Invalid credentials']);
        return;
    }

    echo json_encode([
        'userId' => $user['id'],
        'username' => $user['username']
    ]);
}

function authenticate(string $authHeader): string {
    if (empty($authHeader)) {
        http_response_code(401);
        echo json_encode(['error' => 'Authentication required']);
        exit;
    }

    if (strpos($authHeader, 'Bearer ') !== 0) {
        http_response_code(401);
        echo json_encode(['error' => 'Invalid authorization format']);
        exit;
    }

    $userId = substr($authHeader, 7);
    
    if (empty($userId)) {
        http_response_code(401);
        echo json_encode(['error' => 'Invalid token']);
        exit;
    }

    return $userId;
}

// ============================================================
// TASKS - GET ALL (mit Filtering und Pagination)
// ============================================================
function getAllTasks(PDO $db, string $userId): void {
    try {
        // Query-Parameter auslesen
        $status = $_GET['status'] ?? null;
        $priority = $_GET['priority'] ?? null;
        $search = $_GET['search'] ?? null;
        $page = isset($_GET['page']) ? max(1, intval($_GET['page'])) : 1;
        $limit = isset($_GET['limit']) ? min(100, max(1, intval($_GET['limit']))) : 10;
        $offset = ($page - 1) * $limit;

        // SQL Query dynamisch aufbauen
        $conditions = ['userId = ?'];
        $params = [$userId];

        if ($status) {
            $conditions[] = 'status = ?';
            $params[] = $status;
        }

        if ($priority) {
            $conditions[] = 'priority = ?';
            $params[] = $priority;
        }

        if ($search) {
            $conditions[] = '(title LIKE ? OR description LIKE ?)';
            $searchPattern = '%' . $search . '%';
            $params[] = $searchPattern;
            $params[] = $searchPattern;
        }

        // Gesamtanzahl ermitteln
        $countQuery = "SELECT COUNT(*) FROM tasks WHERE " . implode(' AND ', $conditions);
        $stmt = $db->prepare($countQuery);
        $stmt->execute($params);
        $totalItems = $stmt->fetchColumn();

        // Daten abfragen mit Pagination
        $dataQuery = "SELECT * FROM tasks WHERE " . implode(' AND ', $conditions) . " ORDER BY createdAt DESC LIMIT ? OFFSET ?";
        $stmt = $db->prepare($dataQuery);
        $stmt->execute(array_merge($params, [$limit, $offset]));
        $tasks = $stmt->fetchAll();

        // Pagination-Info berechnen
        $totalPages = ceil($totalItems / $limit);
        $hasNextPage = $page < $totalPages;
        $hasPreviousPage = $page > 1;

        // Response
        echo json_encode([
            'data' => $tasks,
            'pagination' => [
                'page' => $page,
                'limit' => $limit,
                'totalItems' => $totalItems,
                'totalPages' => $totalPages,
                'hasNextPage' => $hasNextPage,
                'hasPreviousPage' => $hasPreviousPage
            ]
        ]);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to fetch tasks', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// TASKS - GET SINGLE
// ============================================================
function getTask(PDO $db, string $id, string $userId): void {
    try {
        $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ? AND userId = ?");
        $stmt->execute([$id, $userId]);
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
// TASKS - CREATE
// ============================================================
function createTask(PDO $db, array $input, string $userId): void {
    // Validierung
    $errors = validateCreateTask($input);
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }

    $title = $input['title'];
    $description = $input['description'];
    $priority = $input['priority'] ?? 'medium';
    $status = 'pending';
    $id = generateUuid();

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

// ============================================================
// TASKS - UPDATE
// ============================================================
function updateTask(PDO $db, string $id, array $input, string $userId): void {
    // Validierung
    $errors = validateUpdateTask($input);
    if (!empty($errors)) {
        http_response_code(400);
        echo json_encode(['errors' => $errors]);
        return;
    }

    // Prüfen ob Task existiert und dem User gehört
    $checkStmt = $db->prepare("SELECT * FROM tasks WHERE id = ? AND userId = ?");
    $checkStmt->execute([$id, $userId]);
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
    $params[] = $userId;

    try {
        $sql = "UPDATE tasks SET " . implode(', ', $updates) . " WHERE id = ? AND userId = ?";
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
// TASKS - DELETE
// ============================================================
function deleteTask(PDO $db, string $id, string $userId): void {
    try {
        // Prüfen ob Task existiert und dem User gehört
        $checkStmt = $db->prepare("SELECT COUNT(*) FROM tasks WHERE id = ? AND userId = ?");
        $checkStmt->execute([$id, $userId]);
        $count = $checkStmt->fetchColumn();

        if ($count == 0) {
            http_response_code(404);
            echo json_encode(['error' => 'Task not found']);
            return;
        }

        $stmt = $db->prepare("DELETE FROM tasks WHERE id = ? AND userId = ?");
        $stmt->execute([$id, $userId]);

        http_response_code(204);
    } catch (PDOException $e) {
        http_response_code(500);
        echo json_encode(['error' => 'Failed to delete task', 'message' => $e->getMessage()]);
    }
}

// ============================================================
// VALIDIERUNG
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
// HILFSFUNKTIONEN
// ============================================================
function generateUuid(): string {
    $data = random_bytes(16);
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
