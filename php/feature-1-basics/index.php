<?php
/**
 * Task Management API - Feature 1: Basics
 * 
 * Einfache REST API mit PHP und SQLite
 * GET ist implementiert, POST/PUT/DELETE sind TODOs
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
// TODO AUFGABE 1: POST /api/tasks implementieren
// ============================================================
// Was du tun musst:
// 1. Generiere eine neue UUID mit generateUuid()
// 2. Extrahiere title, description, priority aus $input
// 3. Setze default status = "pending"
// 4. Setze default priority = "medium" (falls nicht angegeben)
// 5. Füge Task in Datenbank ein (INSERT INTO tasks ...)
// 6. Hole den neuen Task aus der DB und gib ihn zurück
// 7. Status-Code: 201 Created
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function createTask(PDO $db, array $input): void {
    // HIER IMPLEMENTIEREN
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet - implement POST here']);
}

// ============================================================
// TODO AUFGABE 2: PUT /api/tasks/:id implementieren
// ============================================================
// Was du tun musst:
// 1. Prüfe welche Felder in $input vorhanden sind
// 2. Baue dynamisches UPDATE-Statement (nur übergebene Felder)
// 3. Setze updatedAt = CURRENT_TIMESTAMP
// 4. Prüfe ob Task existiert (404 wenn nicht)
// 5. Führe UPDATE aus
// 6. Hole aktualisierten Task und gib ihn zurück
// 7. Status-Code: 200 OK
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function updateTask(PDO $db, string $id, array $input): void {
    // HIER IMPLEMENTIEREN
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet - implement PUT here']);
}

// ============================================================
// TODO AUFGABE 3: DELETE /api/tasks/:id implementieren
// ============================================================
// Was du tun musst:
// 1. Lösche Task aus Datenbank (DELETE FROM tasks WHERE id = ...)
// 2. Prüfe ob Task gelöscht wurde (rowCount() gibt Anzahl betroffener Zeilen zurück)
// 3. Gib 204 No Content bei Erfolg zurück
// 4. Gib 404 wenn Task nicht gefunden
//
// Tipp: Siehe HINTS.md für Code-Beispiele
// ============================================================
function deleteTask(PDO $db, string $id): void {
    // HIER IMPLEMENTIEREN
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet - implement DELETE here']);
}

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
