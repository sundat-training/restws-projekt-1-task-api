# Lösungshinweise - Feature 5: Authentication

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Authentifizierung in `src/index.ts`.

---

## Aufgabe 1: Login Endpunkt implementieren

### Lösungsansatz

1. Erstelle POST `/api/auth/login`
2. Extrahiere `username` und `password` aus `req.body`
3. Suche User in `users`-Tabelle
4. Vergleiche Passwörter (plaintext oder hashed)
5. Bei Erfolg: return `{ userId, username }`
6. Bei Fehler: return `401 Unauthorized`

### Datenbank-Schema

```sql
-- Users-Tabelle existiert bereits:
CREATE TABLE users (
  id TEXT PRIMARY KEY,
  username TEXT UNIQUE NOT NULL,
  password TEXT NOT NULL,
  createdAt TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Test-Benutzer:
-- alice / password123 (user-1)
-- bob / password123 (user-2)
```

### Lösung: Einfacher Login (Plaintext)

```typescript
app.post('/api/auth/login', (req: Request, res: Response) => {
  const { username, password } = req.body;
  
  // Validierung
  if (!username || !password) {
    return res.status(400).json({ error: 'Username and password required' });
  }
  
  // User suchen
  db.get(
    'SELECT * FROM users WHERE username = ?',
    [username],
    (err, user: any) => {
      if (err) {
        return res.status(500).json({ error: 'Database error' });
      }
      
      // User nicht gefunden oder falsches Passwort
      if (!user || user.password !== password) {
        return res.status(401).json({ error: 'Invalid credentials' });
      }
      
      // Erfolg
      res.json({
        userId: user.id,
        username: user.username
      });
    }
  );
});
```

### Lösung: Mit bcrypt (Password Hashing)

```typescript
import bcrypt from 'bcrypt';

app.post('/api/auth/login', async (req: Request, res: Response) => {
  const { username, password } = req.body;
  
  if (!username || !password) {
    return res.status(400).json({ error: 'Username and password required' });
  }
  
  db.get(
    'SELECT * FROM users WHERE username = ?',
    [username],
    async (err, user: any) => {
      if (err) return res.status(500).json({ error: 'Database error' });
      
      if (!user) {
        return res.status(401).json({ error: 'Invalid credentials' });
      }
      
      // Passwort mit bcrypt vergleichen
      const match = await bcrypt.compare(password, user.password);
      
      if (!match) {
        return res.status(401).json({ error: 'Invalid credentials' });
      }
      
      res.json({
        userId: user.id,
        username: user.username
      });
    }
  );
});
```

---

## Aufgabe 2: Auth-Middleware implementieren

### Lösungsansatz

1. Erstelle Middleware-Funktion
2. Prüfe `Authorization` Header
3. Extrahiere Token/User-ID
4. Bei Fehler: return `401 Unauthorized`
5. Bei Erfolg: speichere User in `req.user`
6. Rufe `next()` auf um zur nächsten Middleware/Route zu gehen

### Lösung: Einfache Token-Variante

```typescript
// Type-Erweiterung für Request
declare global {
  namespace Express {
    interface Request {
      user?: { id: string; username: string };
    }
  }
}

// Middleware
const authMiddleware = (req: Request, res: Response, next: NextFunction) => {
  const authHeader = req.headers.authorization;
  
  if (!authHeader) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  
  // Einfache Variante: "Bearer user-1"
  const token = authHeader.replace('Bearer ', '');
  
  if (!token) {
    return res.status(401).json({ error: 'Invalid token' });
  }
  
  // User aus Datenbank holen (optional - für mehr Sicherheit)
  db.get(
    'SELECT id, username FROM users WHERE id = ?',
    [token],
    (err, user: any) => {
      if (err || !user) {
        return res.status(401).json({ error: 'Invalid token' });
      }
      
      // User an Request anhängen
      req.user = user;
      next();
    }
  );
};
```

### Lösung: Ohne Datenbank-Prüfung (schneller)

```typescript
const authMiddleware = (req: Request, res: Response, next: NextFunction) => {
  const authHeader = req.headers.authorization;
  
  if (!authHeader) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  
  const token = authHeader.replace('Bearer ', '');
  
  // Einfachst-Variante: Token = userId
  if (!token.startsWith('user-')) {
    return res.status(401).json({ error: 'Invalid token' });
  }
  
  req.user = { id: token, username: '' };  // Username könnte man aus DB holen
  next();
};
```

### Anwendung der Middleware

```typescript
// Geschützte Route
app.get('/api/tasks', authMiddleware, (req: Request, res: Response) => {
  // req.user.id ist jetzt verfügbar
  const userId = req.user!.id;
  // ... restlicher Code
});

// Mehrere Middlewares
app.post('/api/tasks', authMiddleware, validateCreateTask, (req, res) => {
  // Erst Auth prüfen, dann Validierung, dann Handler
});
```

---

## Aufgabe 3: Tasks mit User verknüpfen

### GET - Nur eigene Tasks anzeigen

```typescript
app.get('/api/tasks', authMiddleware, (req: Request, res: Response) => {
  const userId = req.user!.id;
  
  // Filter + Pagination wie bisher
  const { status, priority, search, page = '1', limit = '10' } = req.query;
  const pageNum = parseInt(page as string) || 1;
  const limitNum = parseInt(limit as string) || 10;
  const offset = (pageNum - 1) * limitNum;
  
  // Build Query mit User-Filter
  let query = 'SELECT * FROM tasks WHERE userId = ?';
  const params: any[] = [userId];
  
  // Zusätzliche Filter
  if (status) {
    query += ' AND status = ?';
    params.push(status);
  }
  if (priority) {
    query += ' AND priority = ?';
    params.push(priority);
  }
  if (search) {
    query += ' AND (title LIKE ? OR description LIKE ?)';
    const pattern = `%${search}%`;
    params.push(pattern, pattern);
  }
  
  // Count
  const countQuery = query.replace('SELECT *', 'SELECT COUNT(*) as count');
  
  db.get(countQuery, params, (err, row: { count: number }) => {
    if (err) return res.status(500).json({ error: err.message });
    
    const total = row.count;
    
    // Pagination
    query += ' LIMIT ? OFFSET ?';
    params.push(limitNum, offset);
    
    db.all(query, params, (err, tasks) => {
      if (err) return res.status(500).json({ error: err.message });
      
      res.json({
        data: tasks,
        meta: {
          page: pageNum,
          limit: limitNum,
          total,
          totalPages: Math.ceil(total / limitNum)
        }
      });
    });
  });
});
```

### POST - Task mit User erstellen

```typescript
app.post('/api/tasks', authMiddleware, validateCreateTask, (req: Request, res: Response) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }
  
  const { title, description, priority = 'medium' } = req.body;
  const userId = req.user!.id;  // Aus Middleware
  const id = uuidv4();
  const status = 'pending';

  db.run(
    `INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt) 
     VALUES (?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`,
    [id, title, description, status, priority, userId],
    function(err) {
      if (err) return res.status(500).json({ error: 'Failed to create task' });
      
      db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
        res.status(201).json(task);
      });
    }
  );
});
```

---

## Aufgabe 4: User-Isolation bei PUT/DELETE

### PUT - Nur eigene Tasks aktualisieren

```typescript
app.put('/api/tasks/:id', authMiddleware, validateUpdateTask, (req: Request, res: Response) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }
  
  const { id } = req.params;
  const userId = req.user!.id;
  
  // Prüfen ob Task existiert und User gehört
  db.get(
    'SELECT * FROM tasks WHERE id = ? AND userId = ?',
    [id, userId],
    (err, task: any) => {
      if (err) return res.status(500).json({ error: 'Database error' });
      
      if (!task) {
        return res.status(403).json({ 
          error: 'Not authorized to update this task' 
        });
      }
      
      // Task gehört User - jetzt aktualisieren
      const updates: string[] = [];
      const params: any[] = [];
      
      if (req.body.title) { updates.push('title = ?'); params.push(req.body.title); }
      if (req.body.description) { updates.push('description = ?'); params.push(req.body.description); }
      if (req.body.status) { updates.push('status = ?'); params.push(req.body.status); }
      if (req.body.priority) { updates.push('priority = ?'); params.push(req.body.priority); }
      
      if (updates.length === 0) {
        return res.status(400).json({ error: 'No fields to update' });
      }
      
      updates.push('updatedAt = CURRENT_TIMESTAMP');
      params.push(id);
      
      const query = `UPDATE tasks SET ${updates.join(', ')} WHERE id = ?`;
      
      db.run(query, params, function(err) {
        if (err) return res.status(500).json({ error: 'Failed to update task' });
        
        db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, updatedTask) => {
          res.json(updatedTask);
        });
      });
    }
  );
});
```

### DELETE - Nur eigene Tasks löschen

```typescript
app.delete('/api/tasks/:id', authMiddleware, (req: Request, res: Response) => {
  const { id } = req.params;
  const userId = req.user!.id;
  
  // Prüfen ob Task existiert und User gehört
  db.get(
    'SELECT * FROM tasks WHERE id = ? AND userId = ?',
    [id, userId],
    (err, task: any) => {
      if (err) return res.status(500).json({ error: 'Database error' });
      
      if (!task) {
        return res.status(403).json({ 
          error: 'Not authorized to delete this task' 
        });
      }
      
      // Löschen
      db.run('DELETE FROM tasks WHERE id = ?', [id], function(err) {
        if (err) return res.status(500).json({ error: 'Failed to delete task' });
        res.status(204).send();
      });
    }
  );
});
```

### Alternative: Direktes DELETE mit Prüfung

```typescript
app.delete('/api/tasks/:id', authMiddleware, (req: Request, res: Response) => {
  const { id } = req.params;
  const userId = req.user!.id;
  
  // Direktes DELETE mit userId-Bedingung
  db.run(
    'DELETE FROM tasks WHERE id = ? AND userId = ?',
    [id, userId],
    function(err) {
      if (err) return res.status(500).json({ error: 'Failed to delete task' });
      
      // Prüfen ob etwas gelöscht wurde
      if (this.changes === 0) {
        return res.status(403).json({ error: 'Not authorized or task not found' });
      }
      
      res.status(204).send();
    }
  );
});
```

---

## Bonus: Register Endpunkt

### Lösung: User Registrierung

```typescript
app.post('/api/auth/register', async (req: Request, res: Response) => {
  const { username, password } = req.body;
  
  // Validierung
  if (!username || !password) {
    return res.status(400).json({ error: 'Username and password required' });
  }
  
  if (password.length < 6) {
    return res.status(400).json({ error: 'Password must be at least 6 characters' });
  }
  
  // Prüfen ob Username existiert
  db.get(
    'SELECT * FROM users WHERE username = ?',
    [username],
    async (err, existingUser: any) => {
      if (err) return res.status(500).json({ error: 'Database error' });
      
      if (existingUser) {
        return res.status(409).json({ error: 'Username already exists' });
      }
      
      // Passwort hashen (optional)
      const hashedPassword = await bcrypt.hash(password, 10);
      
      // User erstellen
      const id = uuidv4();
      
      db.run(
        'INSERT INTO users (id, username, password) VALUES (?, ?, ?)',
        [id, username, hashedPassword],
        function(err) {
          if (err) return res.status(500).json({ error: 'Failed to create user' });
          
          res.status(201).json({
            userId: id,
            username
          });
        }
      );
    }
  );
});
```

---

## Häufige Fehler vermeiden

1. **Immer User-ID aus req.user holen, nicht aus Body**
   ```typescript
   const userId = req.user!.id;  // ✅ Aus Middleware
   const userId = req.body.userId;  // ❌ User könnte fremde ID angeben
   ```

2. **403 vs 404 bei fremden Tasks**
   - 403 = Verboten (Task existiert, aber User darf nicht)
   - 404 = Nicht gefunden (Task existiert nicht)
   - Bei Security meist 403 verwenden (nicht verraten dass Task existiert)

3. **Nie req.user überschreiben**
   ```typescript
   req.user = { id: 'user-1' };  // ✅ Nur in Middleware
   ```

4. **Auth-Middleware Reihenfolge**
   ```typescript
   app.get('/api/tasks', authMiddleware, handler);  // ✅ Auth zuerst
   app.get('/api/tasks', handler, authMiddleware);  // ❌ Zu spät
   ```

5. **Token-Validierung**
   - Nie blind token akzeptieren
   - Optional: In Datenbank prüfen ob User existiert
   - Bei JWT: Token-Verifizierung mit Secret
