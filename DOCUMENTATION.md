# REST Web Services Programming Lab - Dokumentation

## 1. Ordner-Struktur

```
restws-programming-lab/
├── projekt-1-task-api/
│   ├── csharp/
│   │   ├── feature-1-basics/
│   │   ├── feature-2-validation/
│   │   ├── feature-3-filtering/
│   │   ├── feature-4-pagination/
│   │   ├── feature-5-auth/
│   │   └── feature-complete/
│   ├── php/
│   │   ├── feature-1-basics/
│   │   ├── feature-2-validation/
│   │   ├── feature-3-filtering/
│   │   ├── feature-4-pagination/
│   │   ├── feature-5-auth/
│   │   └── feature-complete/
│   └── typescript/
│       ├── feature-1-basics/
│       ├── feature-2-validation/
│       ├── feature-3-filtering/
│       ├── feature-4-pagination/
│       ├── feature-5-auth/
│       └── feature-complete/
```

## 2. Pro Feature: Immer diese Dateien

### TypeScript
```
feature-X/
├── docker-compose.yml      # Port 300X
├── Dockerfile
├── src/index.ts            # Code (unvollständig) - ALLES in einer Datei!
├── tests.http              # ~11 Tests, 1/3 als AUFGABE
├── hint.http               # VOLLSTÄNDIGE Lösungen
├── README.md               # Doku mit Aufgaben
├── openapi.yaml
├── package.json
├── tsconfig.json
└── task-api.db             # Auto-generiert
```

### PHP
```
feature-X/
├── docker-compose.yml      # Port 300X
├── Dockerfile
├── index.php               # Code (unvollständig) - ALLES in einer Datei!
├── tests.http              # ~11 Tests, 1/3 als AUFGABE
├── hint.http               # VOLLSTÄNDIGE Lösungen
├── README.md               # Doku mit Aufgaben
├── openapi.yaml
├── composer.json
└── task-api.db             # Auto-generiert
```

### C#
```
feature-X/
├── docker-compose.yml      # Port 300X
├── Dockerfile
├── Controllers/            # API Controller
├── Models/                 # Datenmodelle
├── Data/                   # Datenbank-Config
├── tests.http              # ~11 Tests, 1/3 als AUFGABE
├── hint.http               # VOLLSTÄNDIGE Lösungen
├── README.md               # Doku mit Aufgaben
├── openapi.yaml
├── feature-X.csproj
└── Program.cs              # App-Setup
```

> **WICHTIG:** In Features 1-5 ist der gesamte Code in einer Datei:
> - **TypeScript:** `src/index.ts`
> - **PHP:** `index.php`
> - **C#:** Mehrere Dateien (MVC-Struktur)
>
> Keine Unterordner (controllers/, middleware/, models/, routes/) bei TypeScript und PHP!
> Erst in `feature-complete/` gibt es eine modulare Struktur:
> ```
> feature-complete/
> ├── src/
> │   ├── controllers/    # Task- und Auth-Controller
> │   ├── middleware/     # auth.ts, validation.ts, errorHandler.ts
> │   ├── models/         # Datenbank und Models
> │   ├── routes/         # Route-Definitionen
> │   └── index.ts        # App-Setup
> ```

## 3. WICHTIGSTES PRINZIP

**Der Source Code in `feature-X/` ist UNVOLLSTÄNDIG.** Die Teilnehmer lösen die Aufgaben selber. `feature-complete/` enthält die Gesamtlösung.

| Feature | TypeScript (src/index.ts) | PHP (index.php) | C# (MVC) |
|---------|---------------------------|-----------------|----------|
| feature-1-basics | GET implementiert, POST/PUT/DELETE als TODO | GET implementiert, POST/PUT/DELETE als TODO | GET implementiert, POST/PUT/DELETE als TODO |
| feature-2-validation | CRUD + Validierung als TODO | Noch nicht verfügbar | CRUD + Validierung als TODO |
| feature-3-filtering | CRUD + Validation + Filtering als TODO | Noch nicht verfügbar | CRUD + Validation + Filtering als TODO |
| feature-4-pagination | CRUD + Validation + Filtering + Pagination als TODO | Noch nicht verfügbar | CRUD + Validation + Filtering + Pagination als TODO |
| feature-5-auth | Alles + Auth als TODO | Noch nicht verfügbar | Alles + Auth als TODO |
| feature-complete | ALLES vollständig | Noch nicht verfügbar | ALLES vollständig |

## 4. tests.http Struktur (NEU: Tests selber schreiben!)

- ~11 Tests pro Feature
- **~1/3 als "AUFGABE" mit TODO-Markierungen** - Teilnehmer müssen Request-Body/URL ergänzen
- **~1/3 als API-Aufgaben** - Tests sind vollständig, aber API fehlt noch
- **~1/3 als Referenz-Tests** - Bereits implementiert, dienen als Beispiel

### Struktur:

```http
###
# 1-2. Referenz-Tests (bereits implementiert)
GET {{baseUrl}}/api/tasks

###
# 3. AUFGABE: Test + API implementieren
# LÖSE SELBER:
#   1. Implementiere POST /api/tasks in src/index.ts
#   2. Schreibe diesen Test (Request-Body ergänzen):
POST {{baseUrl}}/api/tasks
Content-Type: application/json

# TODO: Schreibe den Request Body hier:
# {
#   "title": "...",
#   "description": "..."
# }

###
# 4. AUFGABE: Nur Test schreiben
# LÖSE SELBER: Schreibe einen POST Test ohne title:
POST {{baseUrl}}/api/tasks
Content-Type: application/json

# TODO: JSON Body ohne title Feld
```

### Warum Tests selber schreiben?

- **Verständnis vertiefen**: Durch das Schreiben der Requests verstehen Teilnehmer die API besser
- **Zweifache Validierung**: Zuerst den Test schreiben, dann die API implementieren
- **Realistischer**: In der Praxis schreibt man oft zuerst Tests (TDD)

## 5. hint.http Struktur

- IMMER VOLLSTÄNDIGE Lösungen zu allen Tests
- Keine TODOs - nur funktionierende Requests

```http
###
# 3. Create a new task
# LÖSUNG:
POST {{baseUrl}}/api/tasks
Content-Type: application/json

{
  "title": "Neuer Task",
  "description": "This is a new task",
  "priority": "high"
}
# Erwartet: 201 Created mit uuid, status="pending"
```

## 6. README.md Pflicht-Abschnitte

| Sektion | Inhalt |
|---------|--------|
| Aufgaben | Klar definiert: "Das musst du implementieren" |
| Deine Aufgaben im Detail | POST/PUT/DELETE mit Tipps und Code-Beispielen |
| Gegeben | Was bereits implementiert ist (GET) |
| Erwartetes Ergebnis | Request/Response Beispiele |
| Akzeptanzkriterien | Checkliste |
| Docker | Starten/Stoppen/Logs-Befehle |
| Vorbedingungen | Was vorher erfüllt sein muss |
| Test-Szenarien | Tests mit curl + Tabelle |
| Vorgehensweise | 6-Schritte-Anleitung |
| Nächste Schritte | Link zum nächsten Feature |

## 7. Ports

| Feature | Port |
|---------|------|
| feature-1-basics | 3001 |
| feature-2-validation | 3002 |
| feature-3-filtering | 3003 |
| feature-4-pagination | 3004 |
| feature-5-auth | 3005 |
| feature-complete | 3006 |

## 8. Docker Start

### TypeScript
```bash
cd projekt-1-task-api/typescript/feature-X
docker compose up -d
# API auf http://localhost:300X
```

### PHP
```bash
cd projekt-1-task-api/php/feature-X
docker compose up -d
# API auf http://localhost:300X
```

### C#
```bash
cd projekt-1-task-api/csharp/feature-X
docker compose up -d
# API auf http://localhost:300X
```

## 9. Features und ihre Schwerpunkte

### Konzepte (sprachunabhängig)

| Feature | Schwerpunkt | Was dazukommt |
|---------|-------------|---------------|
| feature-1-basics | CRUD-Grundlagen | POST, PUT, DELETE |
| feature-2-validation | Validierung | Request-Validierung |
| feature-3-filtering | Query-Filter | ?status, ?priority, ?search |
| feature-4-pagination | Pagination | ?page, ?limit |
| feature-5-auth | Authentifizierung | Login, Token, Benutzer-Isolation |
| feature-complete | Gesamtlösung | Alles zusammen |

### Sprach-spezifische Libraries

| Feature | TypeScript | PHP | C# |
|---------|------------|-----|-----|
| feature-1-basics | express | PDO SQLite | ASP.NET Core + SQLite |
| feature-2-validation | express-validator | Noch nicht verfügbar | FluentValidation |
| feature-3-filtering | SQL WHERE | Noch nicht verfügbar | SQL WHERE |
| feature-4-pagination | LIMIT/OFFSET | Noch nicht verfügbar | LIMIT/OFFSET |
| feature-5-auth | JWT + bcrypt | Noch nicht verfügbar | JWT + ASP.NET Identity |

## 10. Code-Vorlagen

### TypeScript (src/index.ts)

```typescript
import express, { Application, Request, Response } from 'express';
import sqlite3 from 'sqlite3';
import path from 'path';
import { v4 as uuidv4 } from 'uuid';

const app: Application = express();
const PORT = process.env.PORT || 300X;
const dbPath = path.join(__dirname, '../task-api.db');

app.use(express.json());

const db = new sqlite3.Database(dbPath, (err) => {
  if (err) console.error('DB Error:', err.message);
  else {
    console.log('Connected to SQLite');
    initDb();
  }
});

function initDb() {
  db.serialize(() => {
    db.run(`
      CREATE TABLE IF NOT EXISTS tasks (
        id TEXT PRIMARY KEY,
        title TEXT NOT NULL,
        description TEXT NOT NULL,
        status TEXT DEFAULT 'pending',
        priority TEXT DEFAULT 'medium',
        createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
        updatedAt TEXT DEFAULT CURRENT_TIMESTAMP
      )
    `);

    db.get('SELECT COUNT(*) as count FROM tasks', (err, row: { count: number }) => {
      if (row.count === 0) {
        const tasks = [
          ['task-1', 'Learn TypeScript', 'Complete TypeScript basics', 'completed', 'high'],
          ['task-2', 'Build REST API', 'Create Task API', 'in_progress', 'high'],
          ['task-3', 'Write docs', 'Document all endpoints', 'pending', 'medium']
        ];
        tasks.forEach(([id, title, desc, status, priority]) => {
          db.run(`INSERT INTO tasks VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`,
            [id, title, desc, status, priority]);
        });
      }
    });
  });
}

app.get('/api/tasks', (req: Request, res: Response) => {
  db.all('SELECT * FROM tasks', (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});

app.get('/api/tasks/:id', (req: Request, res: Response) => {
  db.get('SELECT * FROM tasks WHERE id = ?', [req.params.id], (err, task) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch task' });
    if (!task) return res.status(404).json({ error: 'Task not found' });
    res.json(task);
  });
});

// ============================================================
// HIER AUFHÖREN ZU LESEN - AB HIER SELBER LÖSEN!
// ============================================================
//
// TODO: POST /api/tasks implementieren
//       - Neue Task-ID mit uuidv4() generieren
//       - title, description, priority aus req.body nehmen
//       - Status default: 'pending'
//       - INSERT in Datenbank
//       - 201 Created mit neuem Task zurueckgeben
//
app.post('/api/tasks', (req: Request, res: Response) => {
  // HIER IMPLEMENTIEREN
  res.status(501).json({ error: 'Not implemented yet' });
});

// ============================================================
// TODO: PUT /api/tasks/:id implementieren
//       - id aus req.params nehmen
//       - Felder aus req.body: title, description, status, priority
//       - Nur uebergebene Felder updaten
//       - updatedAt auf CURRENT_TIMESTAMP setzen
//       - 404 wenn Task nicht existiert
//
app.put('/api/tasks/:id', (req: Request, res: Response) => {
  // HIER IMPLEMENTIEREN
  res.status(501).json({ error: 'Not implemented yet' });
});

// ============================================================
// TODO: DELETE /api/tasks/:id implementieren
//       - id aus req.params nehmen
//       - DELETE FROM tasks WHERE id = ?
//       - 204 No Content bei Erfolg
//       - 404 wenn Task nicht existiert
//
app.delete('/api/tasks/:id', (req: Request, res: Response) => {
  // HIER IMPLEMENTIEREN
  res.status(501).json({ error: 'Not implemented yet' });
});

// ============================================================

app.use((req: Request, res: Response) => res.status(404).json({ error: 'Not found' }));
app.use((err: Error, req: Request, res: Response, next: Function) => res.status(500).json({ error: 'Server error' }));

app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
```

### PHP (index.php)

```php
<?php
/**
 * Task Management API - Feature 1: Basics (PHP)
 * GET ist implementiert, POST/PUT/DELETE sind TODOs
 */

// CORS Headers
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

// Datenbankverbindung
$dbPath = __DIR__ . '/task-api.db';
try {
    $db = new PDO('sqlite:' . $dbPath);
    $db->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    $db->setAttribute(PDO::ATTR_DEFAULT_FETCH_MODE, PDO::FETCH_ASSOC);
} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode(['error' => 'Datenbankverbindung fehlgeschlagen']);
    exit;
}

// Request-Daten
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

// Datenbank initialisieren
function initializeDatabase(PDO $db): void {
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
    // ... Seed-Daten einfügen
}

// GET Alle Tasks
function getAllTasks(PDO $db): void {
    $stmt = $db->query("SELECT * FROM tasks");
    echo json_encode($stmt->fetchAll());
}

// GET Einzelner Task
function getTask(PDO $db, string $id): void {
    $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
    $stmt->execute([$id]);
    $task = $stmt->fetch();
    
    if ($task) {
        echo json_encode($task);
    } else {
        http_response_code(404);
        echo json_encode(['error' => 'Task not found']);
    }
}

// TODO: POST, PUT, DELETE implementieren
function createTask(PDO $db, array $input): void {
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet']);
}

function updateTask(PDO $db, string $id, array $input): void {
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet']);
}

function deleteTask(PDO $db, string $id): void {
    http_response_code(501);
    echo json_encode(['error' => 'Not implemented yet']);
}

// UUID Hilfsfunktion
function generateUuid(): string {
    $data = random_bytes(16);
    $data[6] = chr(ord($data[6]) & 0x0f | 0x40);
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80);
    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
}
```

> **Hinweis:** C# verwendet eine MVC-Struktur mit separaten Dateien in `Controllers/`, `Models/`, `Data/`. Siehe `projekt-1-task-api/csharp/feature-1-basics/`.

## 11. docker-compose.yml Vorlage

```yaml
version: '3.8'

services:
  task-api-basics:
    build: .
    container_name: task-api-basics
    ports:
      - "3001:3000"
    volumes:
      - .:/app
      - /app/node_modules
    working_dir: /app
    command: npm run dev
    restart: unless-stopped

networks:
  default:
    name: restws-network
```

## 12. Dockerfile Vorlage

**WICHTIG:** Debian-basiertes Image verwenden (nicht Alpine!) für bessere Kompatibilität.

```dockerfile
FROM node:23-slim

WORKDIR /workspaces/feature-X

# curl für Health Checks installieren
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev"]
```

> **Hinweis:** Kein `USER node` verwenden! Das verursacht Permission-Probleme mit Bind Mounts in DevContainern.

## 13. package.json Vorlage (feature-1-basics)

```json
{
  "name": "task-api-feature-1-basics",
  "version": "1.0.0",
  "description": "Task API - Feature 1: Basics",
  "main": "dist/index.js",
  "scripts": {
    "build": "tsc",
    "start": "node dist/index.js",
    "dev": "ts-node src/index.ts"
  },
  "dependencies": {
    "express": "^4.18.2",
    "sqlite3": "^5.1.7",
    "uuid": "^9.0.1"
  },
  "devDependencies": {
    "@types/express": "^4.17.21",
    "@types/node": "^20.10.0",
    "@types/uuid": "^9.0.7",
    "@types/sqlite3": "^3.1.11",
    "typescript": "^5.3.2",
    "ts-node": "^10.9.2"
  }
}
```

## 14. Wichtige Pfade

| Pfad | Beschreibung |
|------|--------------|
| `/home/unf/IdeaProjects/restws-programming-lab/` | Hauptprojektordner |
| `/home/unf/dokumente/brain/brain/Atlas/Docs/restws-programming-lab/` | Obsidian Vault Docs |

## 15. DevContainer Setup (empfohlen für Schulungen)

Jedes Feature hat seinen eigenen DevContainer für optimale Entwicklungserfahrung mit Linter, IntelliSense und automatischer Dependency-Installation.

### 15.1 Struktur pro Feature

```
feature-X/
├── .devcontainer/
│   ├── devcontainer.json   # Container-Konfiguration
│   ├── post-create.sh      # npm install nach Container-Start
│   └── Dockerfile          # Node.js 23 (Debian slim)
├── .vscode/
│   └── settings.json       # ESLint, REST Client, TypeScript
├── docker-compose.yml      # Mit node_modules Volume
├── Dockerfile
├── src/
├── tests.http
├── hint.http
├── README.md
├── openapi.yaml
├── package.json
├── tsconfig.json
└── task-api.db
```

### 15.2 .devcontainer/devcontainer.json

```json
{
  "name": "Task API - Feature X",
  "build": {
    "dockerfile": "Dockerfile",
    "context": ".."
  },
  "postCreateCommand": "bash .devcontainer/post-create.sh",
  "workspaceFolder": "/workspaces/feature-X",
  "workspaceMount": "source=${localWorkspaceFolder},target=/workspaces/feature-X,type=bind",
  "customizations": {
    "vscode": {
      "extensions": [
        "dbaeumer.vscode-eslint",
        "humao.rest-client",
        "esbenp.prettier-vscode",
        "streetsidesoftware.code-spell-checker",
        "ms-azuretools.vscode-docker"
      ],
      "settings": {
        "typescript.tsdk": "/workspaces/feature-X/node_modules/typescript/lib",
        "search.exclude": {
          "**/node_modules": true,
          "**/dist": true,
          "**/.devcontainer": true
        },
        "files.associations": {
          "*.http": "http"
        },
        "rest-client.environmentVariables": {
          "baseUrl": "http://localhost:300X"
        },
        "editor.formatOnSave": true,
        "editor.codeActionsOnSave": {
          "source.fixAll.eslint": "explicit"
        }
      }
    }
  },
  "portsAttributes": [
    {
      "port": 300X,
      "label": "Task API - Feature X",
      "onAutoForward": "notify"
    }
  ],
  "features": {
    "ghcr.io/devcontainers/features/docker-in-docker:2": {}
  }
}
```

> **Änderungen:**
> - `context` auf `".."` (Parent-Ordner) gesetzt
> - `postCreateCommand` verwendet `bash` statt `sh`
> - Docker-in-Docker Feature hinzugefügt (für docker compose im Container)
> - `remoteUser` entfernt (wir laufen als root für bessere Permissions)

### 15.3 .devcontainer/post-create.sh

```bash
#!/bin/bash
set -e

echo "=========================================="
echo "Post-Create Setup"
echo "=========================================="

echo "Updating npm to latest version..."
npm install -g npm@latest

echo "Node version: $(node -v)"
echo "npm version: $(npm -v)"

echo ""
echo "=========================================="
echo "Installing dependencies..."
echo "=========================================="

# Create node_modules directory with correct permissions
mkdir -p node_modules

# Install dependencies
npm install

echo ""
echo "=========================================="
echo "Setup complete!"
echo "=========================================="
echo "You can now:"
echo "  - Run: docker compose up -d"
echo "  - Open: http://localhost:300X"
echo "  - Test API with: curl http://localhost:300X/api/tasks"
echo "=========================================="
```

> **Hinweis:** Wir verwenden `bash` da node:23-slim (Debian) bash bereits enthält.

### 15.4 .devcontainer/Dockerfile

```dockerfile
FROM node:23-slim

WORKDIR /workspaces/feature-X

# Install curl for health checks and any other tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install dependencies
COPY package*.json ./
RUN npm install

# Copy source code
COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev"]
```

> **Wichtige Änderungen:**
> - `node:23-slim` (Debian-basiert) statt `node:22-alpine`
> - Kein `USER node` - wir laufen als root für bessere DevContainer-Permissions
> - Keine `chown` nötig da keine USER-Umschaltung
> - `apt-get` statt `apk` (Debian vs Alpine)

### 15.5 .vscode/settings.json

```json
{
  "typescript.tsdk": "node_modules/typescript/lib",
  "typescript.enablePromptUseWorkspaceTsdk": true,
  "search.exclude": {
    "**/node_modules": true,
    "**/dist": true,
    "**/.devcontainer": true
  },
  "files.associations": {
    "*.http": "http"
  },
  "rest-client.environmentVariables": {
    "baseUrl": "http://localhost:300X"
  },
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": "explicit"
  },
  "eslint.validate": [
    "javascript",
    "typescript"
  ]
}

> **⚠️ WICHTIG - Datei-Pfad:** Diese Datei MUSS unter `.vscode/settings.json` liegen (nicht in `.devcontainer/`!)
> 
> **Warum `files.associations` essenziell ist:**
> Ohne die Zuordnung `"*.http": "http"` erkennt VSCode die .http Dateien nicht als HTTP Requests.
> Dann werden die "Send Request" Links nicht angezeigt und die REST Client Extension funktioniert nicht!
> 
> **Prüfung:** Unten rechts in VSCode muss stehen: "HTTP" (nicht "Plain Text")

### 15.6 docker-compose.yml (mit node_modules Volume)

```yaml
version: '3.8'

services:
  task-api-feature:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: task-api-feature-X
    ports:
      - "300X:3000"
    volumes:
      - .:/workspaces/feature-X
      - node_modules:/workspaces/feature-X/node_modules
    working_dir: /workspaces/feature-X
    command: sh -c "npm install && npm run dev"
    restart: unless-stopped

volumes:
  node_modules:

networks:
  default:
    name: restws-network
```

> **Wichtige Änderungen:**
> - `dockerfile` auf `Dockerfile` (nicht `.devcontainer/Dockerfile`) gesetzt
> - Bind-Mount verwendet `.` (aktueller Ordner) statt `..`
> - `command` führt `npm install && npm run dev` aus (wichtig für korrekte node_modules!)
> - Named Volume `node_modules` isoliert Dependencies vom Host

### 15.7 Tests ausführen

#### Option A: REST Client Extension (empfohlen)

1. **Datei öffnen:** `tests.http` oder `hint.http`
2. **Auf "Send Request" klicken** (erscheint über jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

#### Option B: VSCode Command Palette (falls "Send Request" nicht erscheint)

Falls die "Send Request" Links nicht angezeigt werden:

1. **Datei öffnen:** `tests.http`
2. **Cursor auf die Anfrage setzen** (z.B. auf die Zeile mit `GET {{baseUrl}}/api/tasks`)
3. **Command Palette öffnen:** `Ctrl+Shift+P` (oder `Cmd+Shift+P` auf Mac)
4. **Tippen:** `Rest Client: Send Request`
5. **Auswählen** und die Anfrage wird ausgeführt
6. **Response wird angezeigt** (rechts im Panel)

#### Option C: Mit curl im Terminal

```bash
# Alle Tasks abrufen:
curl http://localhost:300X/api/tasks

# Einzelnen Task abrufen:
curl http://localhost:300X/api/tasks/task-1

# Task erstellen:
curl -X POST http://localhost:300X/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Neuer Task", "description": "Test", "priority": "high"}'

# Task aktualisieren:
curl -X PUT http://localhost:300X/api/tasks/task-1 \
  -H "Content-Type: application/json" \
  -d '{"status": "completed"}'

# Task löschen:
curl -X DELETE http://localhost:300X/api/tasks/task-3
```

### 15.8 Workflow mit DevContainer

#### Erstmaliges Setup

1. VSCode öffnen → "In Container neu öffnen?" → **Ja**
2. Automatisch: Docker Build + npm install
3. Fertig - voller IntelliSense und funktionierender Linter

#### Entwicklung

```bash
# Im VSCode Terminal (innerhalb des DevContainers):
docker compose up -d

# API auf http://localhost:300X
```

#### 15.8.1 DevContainer manuell öffnen

Falls VSCode nicht automatisch fragt:

**Schritt 1: Ordner öffnen**
```
VSCode → File → Open Folder → /path/to/feature-X
```

**Schritt 2: Reopen in Container**
```
Strg+Shift+P → "Dev Containers: Reopen in Container"
```

**Schritt 3: Warten**
- Docker Image wird gebaut
- npm install läuft
- Fertig!

#### 15.8.2 Troubleshooting: sqlite3 Binary Fehler

Wenn dieser Fehler erscheint:
```
Error: libc.musl-x86_64.so.1: cannot open shared object file
```

**Ursache:** Die node_modules wurden mit Alpine-Linux gebunden, aber der Container läuft mit Debian.

**Lösung:** 
1. Container stoppen: `docker compose down -v`
2. Host node_modules löschen: `rm -rf node_modules`
3. Container neu starten: `docker compose up -d`

Der `npm install` im Container-Command stellt sicher, dass die korrekten Binaries für die Container-Architektur installiert werden.

#### 15.8.3 Troubleshooting: Permission denied (EACCES)

Wenn npm install Fehler wegen Permissions wirft:
```
npm error code EACCES
npm error syscall open
```

**Ursache:** Der Container läuft als non-root User (node) aber Bind-Mount gehört zum Host-User.

**Lösung:** Kein `USER node` im Dockerfile verwenden. Der Container läuft als root, was für DevContainer-Entwicklung akzeptabel ist.

### 15.9 Windows + Remote VM Setup

```
┌─────────────────┐
│ Windows (VSCode)│
│ + Remote SSH    │
└────────┬────────┘
         │ SSH
         ▼
┌─────────────────┐
│ Linux VM        │
│ + Docker        │
└────────┬────────┘
         │ Mount
         ▼
┌─────────────────┐
│ Source Code     │
│ Ordner          │
└─────────────────┘
```

#### Setup-Schritte

1. **Auf Windows:** VSCode mit folgenden Extensions installieren:
   - Remote - SSH
   - Dev Containers

2. **Mit VM verbinden:**
   ```
   Strg+Shift+P → "Remote-SSH: Connect to Host"
   → VM-IP eingeben
   → Passwort eingeben
   ```

3. **Ordner öffnen:**
   ```
   /home/user/restws-programming-lab/typescript/projekt-1-task-api/feature-X
   ```

4. **Container öffnen:**
   - VSCode fragt: "In Container neu öffnen?" → **Ja**
   - Warten bis npm install fertig ist

### 15.10 node_modules persistent

Durch das named Volume `node_modules` werden Dependencies zwischen Container-Neustarts persistiert:

```bash
# Erster Start: npm install (dauert länger)
docker compose up -d

# Zweiter Start: Bereits vorhanden (schnell)
docker compose up -d
```

### 15.11 Voraussetzungen auf der Linux VM

```bash
# Docker muss laufen (ohne sudo)
docker ps

# Falls sudo nötig:
sudo usermod -aG docker $USER

# Docker Socket berechtigen (falls nötig):
sudo chmod 666 /var/run/docker.sock
```

---

## 16. Schnellstart für neue Features

Bei einem neuen Feature diese Dateien kopieren und anpassen:

| Datei/Ordner | Anpassungen |
|--------------|-------------|
| `.devcontainer/devcontainer.json` | Port 300X, Feature-Name |
| `.devcontainer/post-create.sh` | Keine Anpassung nötig |
| `.devcontainer/Dockerfile` | WORKDIR anpassen |
| `.vscode/settings.json` | **WICHTIG:** baseUrl auf Port 300X. Muss in `.vscode/` liegen! |
| `docker-compose.yml` | Port 300X, Containername |
| `package.json` | Feature-Name, Dependencies |
| `tests.http` | **NEU:** ~1/3 mit TODOs - Teilnehmer schreiben Request-Body/URL |
| `hint.http` | VOLLSTÄNDIGE Lösungen |
| `README.md` | Aufgabenbeschreibung |
| `openapi.yaml` | API-Spezifikation |

> **⚠️ Häufiger Fehler:** Die `settings.json` wird oft fälschlicherweise in `.devcontainer/` statt `.vscode/` abgelegt.
> Dann funktioniert die REST Client Extension nicht!
