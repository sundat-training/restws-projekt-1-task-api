# Task Management API - Feature 1: Basics

Herzlich Willkommen zu Feature 1 der Task Management API! In diesem Feature lernst du die Grundlagen einer REST API mit PHP. Du implementierst die POST-, PUT- und DELETE-Endpunkte, während die GET-Endpunkte bereits vorgegeben sind.

## Aufgaben

| HTTP-Methode | Endpoint | Status |
|--------------|----------|--------|
| `POST` | `/api/tasks` | Selber lösen |
| `PUT` | `/api/tasks/:id` | Selber lösen |
| `DELETE` | `/api/tasks/:id` | Selber lösen |

## Deine Aufgaben im Detail

### Aufgabe 1: POST /api/tasks

Erstelle einen neuen Task in der Datenbank.

**Was du tun musst:**

1. Extrahiere die Daten aus dem Request-Body
2. Generiere eine UUID für den neuen Task
3. Führe das INSERT-Statement aus
4. Gib den erstellten Task mit Status 201 Created zurück

**PHP-spezifische Hinweise:**

```php
// Daten aus dem Request-Body extrahieren
$data = json_decode(file_get_contents('php://input'), true);
$title = $data['title'] ?? null;
$description = $data['description'] ?? null;
$status = $data['status'] ?? 'pending';
$priority = $data['priority'] ?? 'medium';

// UUID generieren mit random_bytes
$uuid = vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex(random_bytes(16)), 4));

// Prepared Statement für sichere Datenbankabfrage
$stmt = $db->prepare("INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) VALUES (?, ?, ?, ?, ?, datetime('now'), datetime('now'))");
$stmt->execute([$uuid, $title, $description, $status, $priority]);

// Den frisch erstellten Task zurückgeben
$stmt = $db->prepare("SELECT * FROM tasks WHERE id = ?");
$stmt->execute([$uuid]);
$newTask = $stmt->fetch();

// HIER IMPLEMENTIEREN: Response mit 201 Created und dem neuen Task
```

**Wichtige Punkte:**
- Verwende `json_decode(file_get_contents('php://input'), true)` um JSON-Daten zu lesen
- Nutze `random_bytes(16)` für kryptographisch sichere UUIDs
- Prepared Statements schützen vor SQL-Injection
- Gib den Status 201 Created zurück, nicht 200

### Aufgabe 2: PUT /api/tasks/:id

Aktualisiere einen bestehenden Task.

**Was du tun musst:**

1. Extrahiere die ID aus der URL
2. Extrahiere die zu aktualisierenden Felder aus dem Body
3. Baue das UPDATE-Statement dynamisch auf
4. Aktualisiere das updatedAt-Feld
5. Gib den aktualisierten Task zurück

**PHP-spezifische Hinweise:**

```php
// ID aus der URL extrahieren
$id = $routeParams['id'] ?? null;

// Daten aus dem Request-Body
$data = json_decode(file_get_contents('php://input'), true);
$title = $data['title'] ?? null;
$description = $data['description'] ?? null;
$status = $data['status'] ?? null;
$priority = $data['priority'] ?? null;

// Dynamisches UPDATE-Statement aufbauen
$updates = [];
$params = [];

if ($title !== null) {
    $updates[] = 'title = ?';
    $params[] = $title;
}
if ($description !== null) {
    $updates[] = 'description = ?';
    $params[] = $description;
}
if ($status !== null) {
    $updates[] = 'status = ?';
    $params[] = $status;
}
if ($priority !== null) {
    $updates[] = 'priority = ?';
    $params[] = $priority;
}

$updates[] = 'updatedAt = CURRENT_TIMESTAMP';
$params[] = $id;

// Prepared Statement ausführen
$sql = "UPDATE tasks SET " . implode(', ', $updates) . " WHERE id = ?";
$stmt = $db->prepare($sql);
$stmt->execute($params);

// HIER IMPLEMENTIEREN: Prüfen ob Task existiert und aktualisierten Task zurückgeben
```

**Wichtige Punkte:**
- Baue das UPDATE-Statement dynamisch auf, je nachdem welche Felder gesendet wurden
- Verwende `CURRENT_TIMESTAMP` für updatedAt
- Prüfe ob der Task existiert, bevor du aktualisierst

### Aufgabe 3: DELETE /api/tasks/:id

Lösche einen Task aus der Datenbank.

**Was du tun musst:**

1. Extrahiere die ID aus der URL
2. Führe das DELETE-Statement aus
3. Prüfe ob der Task gelöscht wurde
4. Gib 204 No Content zurück

**PHP-spezifische Hinweise:**

```php
// ID aus der URL extrahieren
$id = $routeParams['id'] ?? null;

// DELETE Statement ausführen
$stmt = $db->prepare("DELETE FROM tasks WHERE id = ?");
$stmt->execute([$id]);

// Prüfen ob eine Zeile gelöscht wurde
if ($stmt->rowCount() === 0) {
    // HIER IMPLEMENTIEREN: 404 Not Found wenn Task nicht existiert
}

// HIER IMPLEMENTIEREN: 204 No Content zurückgeben
```

**Wichtige Punkte:**
- `rowCount()` zeigt wie viele Zeilen betroffen waren
- 204 No Content bedeutet, dass die Anfrage erfolgreich war aber kein Body zurückgegeben wird
- Prüfe mit rowCount() ob der Task existierte

## Gegeben

Die folgenden Endpunkte sind bereits implementiert und funktionsfähig:

| HTTP-Methode | Endpoint | Beschreibung |
|--------------|----------|--------------|
| `GET` | `/api/tasks` | Gibt alle Tasks zurück |
| `GET` | `/api/tasks/:id` | Gibt einen einzelnen Task zurück |

**Architektur:**

Die gesamte Anwendung läuft in einer einzigen Datei: `index.php`. Dies ist bewusst einfach gehalten, damit du dich auf die Grundlagen konzentrieren kannst. Die Daten werden in einer SQLite-Datenbank (`database.sqlite`) gespeichert, die automatisch beim ersten Start erstellt wird.

**Datenbankstruktur:**

Die Tabelle `tasks` hat folgende Spalten:
- `id` (TEXT, PRIMARY KEY) - UUID des Tasks
- `title` (TEXT) - Titel des Tasks
- `description` (TEXT) - Beschreibung des Tasks
- `status` (TEXT) - Status: pending, in_progress, done
- `priority` (TEXT) - Priorität: low, medium, high
- `createdAt` (DATETIME) - Erstellungszeitpunkt
- `updatedAt` (DATETIME) - Letzte Aktualisierung

## Erwartetes Ergebnis

### POST /api/tasks

**Request:**

```bash
curl -X POST http://localhost:8080/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Neuer Task", "description": "Eine Beschreibung", "status": "pending", "priority": "high"}'
```

**Response (201 Created):**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Neuer Task",
  "description": "Eine Beschreibung",
  "status": "pending",
  "priority": "high",
  "createdAt": "2024-01-15 10:30:00",
  "updatedAt": "2024-01-15 10:30:00"
}
```

### PUT /api/tasks/:id

**Request:**

```bash
curl -X PUT http://localhost:8080/api/tasks/550e8400-e29b-41d4-a716-446655440000 \
  -H "Content-Type: application/json" \
  -d '{"title": "Geänderter Titel", "status": "in_progress"}'
```

**Response (200 OK):**

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Geänderter Titel",
  "description": "Eine Beschreibung",
  "status": "in_progress",
  "priority": "high",
  "createdAt": "2024-01-15 10:30:00",
  "updatedAt": "2024-01-15 10:35:00"
}
```

### DELETE /api/tasks/:id

**Request:**

```bash
curl -X DELETE http://localhost:8080/api/tasks/550e8400-e29b-41d4-a716-446655440000
```

**Response (204 No Content):**

```
(kein Body)
```

### Fehlerfälle

| Statuscode | Bedingung |
|------------|-----------|
| 400 Bad Request | Ungültige oder fehlende JSON-Daten |
| 404 Not Found | Task mit der ID existiert nicht |
| 500 Internal Server Error | Datenbankfehler oder Serverproblem |

## Akzeptanzkriterien

Bevor du Feature 1 als abgeschlossen markierst, stelle sicher, dass alle folgenden Kriterien erfüllt sind:

- [ ] POST /api/tasks erstellt einen neuen Task und gibt 201 Created zurück
- [ ] PUT /api/tasks/:id aktualisiert einen existierenden Task
- [ ] DELETE /api/tasks/:id löscht einen Task und gibt 204 No Content zurück
- [ ] Alle Endpunkte geben sinnvolle Fehlermeldungen bei ungültigen Daten zurück
- [ ] Alle Endpunkte geben 404 zurück, wenn der Task nicht existiert
- [ ] Prepared Statements werden verwendet (SQL-Injection-Schutz)
- [ ] UUIDs werden mit random_bytes generiert (kryptographisch sicher)
- [ ] Das updatedAt-Feld wird bei Änderungen aktualisiert

## Projekt starten

### Mit DevContainer (empfohlen)

1. Öffne das Projekt in VS Code
2. Installiere die "Dev Containers" Erweiterung
3. Klicke unten rechts auf "Reopen in Container"
4. Warte bis der Container gestartet ist

### Mit Docker Compose

Stelle sicher, dass Docker und Docker Compose installiert sind:

```bash
# Container starten
docker compose up -d

# Container stoppen
docker compose down

# Logs anzeigen
docker compose logs -f
```

Der PHP-Server ist dann unter `http://localhost:8080` erreichbar.

### Erster Test

Prüfe ob die API läuft:

```bash
curl http://localhost:8080/api/tasks
```

Du solltest ein leeres Array erhalten:

```json
[]
```

## Tests ausführen

Die Tests sind in drei Kategorien unterteilt:

- **TODOs (~1/3)**: Diese zeigen dir wo du Code schreiben musst. Suche nach "// HIER IMPLEMENTIEREN" in der index.php
- **API Tasks (~1/3)**: Die eigentlichen Testfälle für deine Implementierung
- **Reference (~1/3)**: Referenzimplementierung zum Vergleichen

### Mit REST Client Extension

Installiere die "REST Client" Extension in VS Code und öffne dann die Datei `tests.http`:

1. Klicke auf "Send Request" über dem gewünschten Test
2. Die Antwort wird im Editor angezeigt

### Mit curl

Du kannst die Tests auch manuell mit curl ausführen:

```bash
# Alle Tasks abrufen (sollte funktionieren)
curl http://localhost:8080/api/tasks

# Einen neuen Task erstellen
curl -X POST http://localhost:8080/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title": "Test Task", "description": "Eine Beschreibung", "priority": "high"}'

# Einen Task aktualisieren (ersetze :id mit der UUID aus dem vorherigen Request)
curl -X PUT http://localhost:8080/api/tasks/:id \
  -H "Content-Type: application/json" \
  -d '{"status": "done"}'

# Einen Task löschen
curl -X DELETE http://localhost:8080/api/tasks/:id

# Nicht existierenden Task abrufen (sollte 404返回)
curl http://localhost:8080/api/tasks/nicht-existierende-id
```

### Über VS Code Command Palette

1. Drücke `Strg+Shift+P` (oder `Cmd+Shift+P` auf Mac)
2. Tippe "Rest Client" und wähle einen Request aus

## Vorbedingungen

Bevor du beginnst, stelle sicher, dass du folgende Voraussetzungen erfüllst:

- **PHP 8.2+** - Die Anwendung nutzt moderne PHP-Features
- **PDO SQLite** - Für die Datenbankverbindung
- **Docker** - Für die Container-Umgebung
- **Grundlagen PHP** - Variablen, Arrays, Funktionen, objektorientierte Basics

Falls du noch unsicher mit PHP bist, empfehlen wir die offizielle PHP-Dokumentation oder einen schnellen Einstiegskurs vor diesem Training.

## Test-Szenarien

| Test | Was wird getestet | Erwartetes Ergebnis |
|------|-------------------|---------------------|
| POST mit gültigen Daten | Neue Task-Erstellung | 201 Created, Task mit UUID |
| POST ohne Pflichtfelder | Validierung | 400 Bad Request |
| PUT für existierenden Task | Update-Funktion | 200 OK, updatedAt aktualisiert |
| PUT für nicht-existierenden Task | Fehlerbehandlung | 404 Not Found |
| PUT mit Partial Data | Dynamisches Update | Nur gesendete Felder werden geändert |
| DELETE für existierenden Task | Löschfunktion | 204 No Content |
| DELETE für nicht-existierenden Task | Fehlerbehandlung | 404 Not Found |
| GET für existierenden Task | Auslesen | 200 OK, Task-Daten |

## Vorgehensweise

Folge dieser Schritt-für-Schritt-Anleitung, um Feature 1 erfolgreich abzuschließen:

1. **Lese die Aufgabe durch** - Verstehe was gefordert ist, bevor du Code schreibst
2. **Öffne index.php** - Lies den bestehenden Code und verstehe die Struktur
3. **Implementiere die Lösung** - Suche nach "// HIER IMPLEMENTIEREN" und schreibe deinen Code
4. **Teste deine Lösung** - Führe curl-Befehle aus oder nutze die REST Client Extension
5. **Vergleiche mit Reference** - Schaue dir die Musterlösung an, wenn du nicht weiterkommst
6. **Gehe zum nächsten Feature** - Erst wenn alle Tests bestehen

## Vergleich mit feature-complete

In diesem Feature arbeitest du mit einer **einzelnen Datei (index.php)**, die alle Endpunkte enthält. Dies ist bewusst einfach gehalten, damit du die Grundlagen lernst ohne dich mit Projektstrukturen herumschlagen zu müssen.

In der vollständigen Implementierung (feature-complete) wird eine **MVC-Architektur** verwendet:
- **Models** für die Datenbanklogik
- **Views** für die Ausgabe (in diesem Fall JSON)
- **Controller** für die Request-Verarbeitung

Diese Struktur ermöglicht bessere Wartbarkeit, Testbarkeit und Erweiterbarkeit. Die Grundlagen, die du hier lernst, sind jedoch dieselben und übertragbar.

## Nächste Schritte

Nachdem du Feature 1 abgeschlossen hast, geht es weiter mit:

→ **[Feature 2: Validation](../feature-2-validation/README.md)**

In Feature 2 lernst du, wie du Eingabedaten validierst und aussagekräftige Fehlermeldungen zurückgibst.

## Siehe auch

- [openapi.yaml](../openapi.yaml) - Die OpenAPI-Spezifikation der API
- [tests.http](./tests.http) - Alle Test-Anfragen auf einen Blick
- [hint.http](./hint.http) - Hinweise und Tipps zur Implementierung
- [Reference-Implementierung](../feature-1-basics-reference/README.md) - Musterlösung zum Vergleichen

Viel Erfolg beim Implementieren! Bei Fragen hilft dir die Reference-Implementierung oder dein Trainer.
