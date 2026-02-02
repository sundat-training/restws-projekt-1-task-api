# Task Management API - Feature 5: Authentication

## Aufgaben

In diesem Feature erweiterst du die API um **Authentifizierung**. Alle vorherigen Features (CRUD, Validierung, Filtering, Pagination) sind bereits implementiert. Jetzt sollen die Tasks Benutzer-spezifisch werden.

| Deine Aufgabe | Status |
|---------------|--------|
| POST `/api/auth/login` | **Selber lösen** |
| Auth-Middleware | **Selber lösen** |
| User-Isolation | **Selber lösen** |
| Eigentümerschaft prüfen bei PUT/DELETE | **Selber lösen** |

---

## Deine Aufgaben im Detail

### Aufgabe 1: Login Endpunkt implementieren

**Was du tun musst:**
1. Erstelle POST `/api/auth/login`
2. Prüfe username und password in der users-Tabelle
3. Bei Erfolg: return `{ userId, username }`
4. Bei Fehler: return `401 Unauthorized`

**Wo du es findest:**
- Datei: `src/index.php`
- Suche nach: `// TODO: Implementiere Login`

---

### Aufgabe 2: Auth-Middleware implementieren

**Was du tun musst:**
1. Erstelle eine Funktion die den Authorization Header prüft
2. Extrahiere den User aus dem Bearer Token
3. Bei fehlendem/ungültigem Auth: return `401 Unauthorized`
4. Speichere die userId für spätere Endpunkte

**Wo du es findest:**
- Datei: `src/index.php`
- Suche nach: `// TODO: Erstelle Auth-Middleware`

---

### Aufgabe 3: User-Isolation implementieren

**Was du tun musst:**
1. Bei GET `/api/tasks`: Zeige nur Tasks wo `userId = ?`
2. Bei POST `/api/tasks`: Setze `userId` aus dem Authorization Header
3. Bei GET `/api/tasks/:id`: Prüfe ob der Task dem User gehört

**Wo du es findest:**
- Datei: `src/index.php`
- GET und POST Endpunkte

---

### Aufgabe 4: Eigentümerschaft prüfen bei PUT/DELETE

**Was du tun musst:**
1. Bei PUT `/api/tasks/:id`: Prüfe ob der Task dem User gehört
2. Bei DELETE `/api/tasks/:id`: Prüfe ob der Task dem User gehört
3. Wenn nicht: return `403 Forbidden`

**Wo du es findest:**
- Datei: `src/index.php`
- PUT und DELETE Endpunkte

---

## Test-Users

| Username | Password | User ID |
|----------|----------|---------|
| alice | password123 | user-1 |
| bob | password456 | user-2 |

## Sample Tasks

| Task ID | Title | User |
|---------|-------|------|
| task-1 | Learn TypeScript | alice (user-1) |
| task-2 | Build REST API | alice (user-1) |
| task-3 | Write docs | alice (user-1) |
| task-4 | Setup database | bob (user-2) |
| task-5 | Write tests | bob (user-2) |

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

## Projekt starten

### Docker starten (Port 3005)

```bash
cd /home/unf/IdeaProjects/restws-programming-lab/projekt-1-task-api/php/feature-5-auth
docker compose up -d
```

### Prüfen ob es läuft

```bash
docker ps
curl http://localhost:3005/api/tasks
```

### Docker stoppen

```bash
docker compose down
```

---

## Tests ausführen

### REST Client Extension (empfohlen)

1. Öffne `tests.http`
2. Klick auf "Send Request" über jedem Test

### Mit curl

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

## Vorgehensweise

1. **Aufgabe lesen** - Diese README
2. **src/index.php öffnen** - Finde die TODOs
3. **Login implementieren** - POST /api/auth/login
4. **Auth-Middleware erstellen** - Prüfe Authorization Header
5. **GET anpassen** - Nur eigene Tasks anzeigen
6. **POST anpassen** - userId setzen
7. **PUT/DELETE anpassen** - Autorisierung prüfen
8. **Testen** - Führe Tests mit REST Client oder curl aus
9. **Vergleichen** - Mit der Lösung in `hint.http` vergleichen

---

## Siehe auch

- [[../feature-4-pagination/README|Feature 4: Pagination]] - Voraussetzung
- [[tests.http|Test-Szenarien ausführen]]
- [[hint.http|Lösungen anzeigen]]
- [[HINTS.md|Hilfe und Hinweise]]
