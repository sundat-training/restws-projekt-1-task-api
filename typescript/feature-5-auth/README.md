# Task Management API - Feature 5: Authentication

## Aufgaben

In diesem Feature erweiterst du die API um **Authentifizierung**. Alle vorherigen Features (CRUD, Validierung, Filtering, Pagination) sind bereits implementiert. Jetzt sollen die Tasks Benutzer-spezifisch werden.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/auth/login` | **Selber lösen** |
| Auth-Middleware | **Selber lösen** |
| Tasks mit User verknüpfen | **Selber lösen** |
| User-Isolation | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Login Endpunkt implementieren

**Was du tun musst:**
1. Erstelle POST `/api/auth/login`
2. Prüfe username und password in der users-Tabelle
3. Bei Erfolg: return `{ userId, username }`
4. Bei Fehler: return `401 Unauthorized`

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: Implementiere Login`

**Tipp:**
```typescript
app.post('/api/auth/login', (req, res) => {
  const { username, password } = req.body;
  
  db.get('SELECT * FROM users WHERE username = ?', [username], (err, user) => {
    if (!user || user.password !== password) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }
    res.json({ userId: user.id, username: user.username });
  });
});
```

---

### Aufgabe 2: Auth-Middleware implementieren

**Was du tun musst:**
1. Erstelle eine Middleware-Funktion `authMiddleware`
2. Prüfe den Authorization Header
3. Extrahiere den User aus dem Token/Basic Auth
4. Bei fehlendem/ungültigem Auth: return `401 Unauthorized`
5. Speichere den User in `req.user` für spätere Endpunkte

**Wo du es findest:**
- Datei: `src/index.ts`
- Suche nach: `// TODO: Erstelle authMiddleware`

**Tipp (einfache Variante ohne JWT):**
```typescript
const authMiddleware = (req: Request, res: Response, next: NextFunction) => {
  const authHeader = req.headers.authorization;
  
  if (!authHeader) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  
  // Einfache Variante: "Bearer user-1" oder Basic Auth
  const userId = authHeader.replace('Bearer ', '');
  req.user = { id: userId };
  next();
};

// Anwenden auf geschützte Routen:
app.get('/api/tasks', authMiddleware, (req, res) => {
  // req.user.id enthält die userId
});
```

---

### Aufgabe 3: Tasks mit User verknüpfen

**Was du tun musst:**
1. Bei POST `/api/tasks`: Setze `userId` aus dem eingeloggten User
2. Bei GET `/api/tasks`: Zeige nur Tasks wo `userId = req.user.id`
3. Datenbank-Tabelle tasks hat bereits eine userId Spalte

**Wo du es findest:**
- Datei: `src/index.ts`
- GET und POST Endpunkte

**Tipp:**
```typescript
// POST - Task mit userId erstellen
app.post('/api/tasks', authMiddleware, validateCreateTask, (req, res) => {
  const userId = req.user.id; // Aus Middleware
  // ... INSERT mit userId
  db.run(
    'INSERT INTO tasks (id, title, ..., userId) VALUES (..., ?)',
    [..., userId],
    // ...
  );
});

// GET - Nur eigene Tasks
app.get('/api/tasks', authMiddleware, (req, res) => {
  const userId = req.user.id;
  db.all('SELECT * FROM tasks WHERE userId = ?', [userId], (err, tasks) => {
    // ...
  });
});
```

---

### Aufgabe 4: User-Isolation bei PUT/DELETE

**Was du tun musst:**
1. Bei PUT `/api/tasks/:id`: Prüfe ob der Task dem User gehört
2. Bei DELETE `/api/tasks/:id`: Prüfe ob der Task dem User gehört
3. Wenn nicht: return `403 Forbidden`

**Wo du es findest:**
- Datei: `src/index.ts`
- PUT und DELETE Endpunkte

**Tipp:**
```typescript
app.put('/api/tasks/:id', authMiddleware, validateUpdateTask, (req, res) => {
  const { id } = req.params;
  const userId = req.user.id;
  
  // Zuerst prüfen ob Task existiert und User gehört
  db.get('SELECT * FROM tasks WHERE id = ? AND userId = ?', [id, userId], (err, task) => {
    if (!task) {
      return res.status(403).json({ error: 'Not authorized to modify this task' });
    }
    // ... dann UPDATE
  });
});
```

---

## Gegeben (bereits implementiert)

- Alle CRUD-Endpunkte (GET, POST, PUT, DELETE)
- Validierung mit express-validator
- Filtering mit Query-Parametern
- Pagination
- SQLite-Datenbank mit users- und tasks-Tabelle
- 2 Beispiel-User: alice (user-1) und bob (user-2)
- 5 Tasks mit userId-Verknüpfung
- Alles in **einer Datei**: `src/index.ts` (keine Unterordner!)

### Zu implementieren

- Login Endpunkt
- Auth-Middleware
- User-Filter bei GET
- User-Zuordnung bei POST
- Autorisierungs-Prüfung bei PUT/DELETE

---

## Erwartetes Ergebnis

### Login

**Request:**
```bash
POST http://localhost:3005/api/auth/login
Content-Type: application/json

{
  "username": "alice",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "userId": "user-1",
  "username": "alice"
}
```

### Ohne Auth

**Request:**
```bash
GET http://localhost:3005/api/tasks
```

**Response (401 Unauthorized):**
```json
{
  "error": "Authentication required"
}
```

### Mit Auth - Eigene Tasks

**Request:**
```bash
GET http://localhost:3005/api/tasks
Authorization: Bearer user-1
```

**Response:**
```json
{
  "data": [
    { "id": "task-1", "title": "Learn TypeScript", "userId": "user-1" },
    { "id": "task-2", "title": "Build REST API", "userId": "user-1" },
    { "id": "task-3", "title": "Write docs", "userId": "user-1" }
  ],
  "meta": { "page": 1, "limit": 10, "total": 3, "totalPages": 1 }
}
```

### Fremden Task ansehen (verboten)

**Request:**
```bash
GET http://localhost:3005/api/tasks/task-4
Authorization: Bearer user-1
```

**Response (403 Forbidden):**
```json
{
  "error": "Not authorized to access this task"
}
```

### Fremden Task ändern (verboten)

**Request:**
```bash
PUT http://localhost:3005/api/tasks/task-1
Authorization: Bearer user-2
Content-Type: application/json

{
  "status": "completed"
}
```

**Response (403 Forbidden):**
```json
{
  "error": "Not authorized to modify this task"
}
```

---

## Akzeptanzkriterien

- [ ] POST `/api/auth/login` prüft username/password
- [ ] Login gibt bei Erfolg `{ userId, username }` zurück
- [ ] Login gibt bei Fehler 401 zurück
- [ ] Auth-Middleware prüft Authorization Header
- [ ] Ohne Auth wird 401 zurückgegeben
- [ ] GET `/api/tasks` zeigt nur Tasks des eingeloggten Users
- [ ] POST `/api/tasks` setzt automatisch userId des eingeloggten Users
- [ ] PUT `/api/tasks/:id` prüft ob Task dem User gehört
- [ ] DELETE `/api/tasks/:id` prüft ob Task dem User gehört
- [ ] Bei fremden Tasks wird 403 zurückgegeben
- [ ] Alice sieht nur ihre Tasks (task-1, 2, 3)
- [ ] Bob sieht nur seine Tasks (task-4, 5)

---

## Projekt starten (im DevContainer)

**Wichtig:** Alle Befehle werden im VSCode Terminal ausgeführt (innerhalb des DevContainers).

### Schritt 1: DevContainer öffnen

1. VSCode starten
2. `File → Open Folder → feature-5-auth`
3. "In Container neu öffnen?" → **Ja**
4. Warten bis Container bereit ist (npm install läuft automatisch)

### Schritt 2: API starten

```bash
# Im VSCode Terminal (DevContainer):
docker compose up -d
```

**Prüfen ob es läuft:**
```bash
docker ps
# Oder:
curl http://localhost:3005/api/tasks
```

### Schritt 3: API stoppen

```bash
docker compose down
```

---

## Tests ausführen

### Test-Struktur

| Test-Kategorie | Beschreibung |
|----------------|--------------|
| **~1/3 mit TODOs** | Teilnehmer müssen Request-Body/Auth-Header schreiben |
| **~1/3 API-Aufgaben** | Tests vollständig, API muss implementiert werden |
| **~1/3 Referenz** | Bereits funktionsfähig, dienen als Beispiel |

**Hinweis:** Suche nach `TODO:` in `tests.http`

### Option A: REST Client Extension (empfohlen)

1. **Datei öffnen:** `tests.http` (Aufgaben) oder `hint.http` (Lösungen als Referenz)
2. **Auf "Send Request" klicken** (erscheint über jedem HTTP-Request)
3. **Response wird angezeigt** (rechts im Panel)

**Voraussetzung:** Extension "REST Client" (humao.rest-client) ist installiert.

### Option B: VSCode Command Palette

Falls die "Send Request" Links nicht angezeigt werden:

1. **Datei öffnen:** `tests.http`
2. **Cursor auf die Anfrage setzen**
3. **Command Palette öffnen:** `Ctrl+Shift+P`
4. **Tippen:** `Rest Client: Send Request`

### Option C: Mit curl im Terminal

```bash
# Login:
curl -X POST http://localhost:3005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "password123"}'

# Mit Auth (einfache Variante):
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-1"

# Als bob:
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-2"
```

---

## Vorbedingungen

✅ **Bereits erfüllt im DevContainer:**
- Node.js 23.x mit npm 11.x
- Docker & Docker Compose
- VSCode Extensions (REST Client, ESLint, etc.)
- SQLite Datenbank
- express-validator (bereits installiert)
- 2 Beispiel-User: alice und bob

**Nur noch starten:** `docker compose up -d`

---

## Test-Szenarien (deine Aufgaben)

| Test | Was getestet wird | Deine Aufgabe |
|------|-------------------|---------------|
| Test 1-3 | Login | POST /api/auth/login |
| Test 4-6 | Auth-Middleware | Authorization Header |
| Test 7 | User-Isolation | Nur eigene Tasks sehen |
| Test 8-9 | Autorisierung | Kein Zugriff auf fremde Tasks |
| Test 10-11 | Register (Bonus) | POST /api/auth/register |
| Test 12 | Password Hashing (Bonus) | bcrypt verwenden |

### Tests ausführen

**Mit REST Client Extension:**
1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

**Mit curl:**
```bash
# Test 1: Login
curl -X POST http://localhost:3005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "alice", "password": "password123"}'

# Test 7: Eigene Tasks (nach Login)
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-1"
```

### Lösungen anzeigen

Wenn du nicht weiterkommst:
- Öffne `hint.http` für die fertigen Requests
- Sieh dir `src/index.ts` an - die TODO-Kommentare helfen dir

---

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **src/index.ts öffnen** - Finde die TODOs
3. **Login implementieren** - POST /api/auth/login
4. **Auth-Middleware erstellen** - Prüfe Authorization Header
5. **GET anpassen** - Nur eigene Tasks anzeigen
6. **POST anpassen** - userId setzen
7. **PUT/DELETE anpassen** - Autorisierung prüfen
8. **Testen** - Führe Tests mit REST Client oder curl aus
9. **Vergleichen** - Mit der Lösung in `hint.http` vergleichen
10. **Weiter** - Wechsle zu `feature-complete` für die Gesamtlösung

---

## Nächste Schritte

Nach diesem Feature:
- [[../feature-complete/README|Feature Complete]] - Gesamtlösung ansehen

---

## Vergleich mit feature-complete

In `feature-complete` wird die Auth anders strukturiert:
- Separate `authController.ts` und `authRoutes.ts`
- JWT (jsonwebtoken) statt einfachem User-ID-Token
- `bcrypt` für Password Hashing
- Zentrale `auth.ts` Middleware
- Refresh Token Mechanismus

Deine Lösung darf einfacher sein:
- Einfaches Token-System (z.B. "Bearer user-1")
- Klartext-Passwörter (oder bcrypt als Bonus)
- Auth direkt in `src/index.ts`

---

## Bonus: Password Hashing

Wenn du Password Hashing implementieren möchtest:

```bash
npm install bcrypt
npm install -D @types/bcrypt
```

```typescript
import bcrypt from 'bcrypt';

// Beim Registrieren:
const hashedPassword = await bcrypt.hash(password, 10);

// Beim Login:
const match = await bcrypt.compare(password, user.password);
```

---

## Siehe auch

- [[JWT Authentication]]
- [[bcrypt Password Hashing]]
- [[Express Middleware]]
- [[../feature-4-pagination/README|Feature 4: Pagination]] - Voraussetzung
- [[openapi.yaml|OpenAPI Spezifikation]]
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[REST API Standards]]
