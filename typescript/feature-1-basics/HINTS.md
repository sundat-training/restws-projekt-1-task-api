# Lösungshinweise - Feature 1: Basics

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Aufgaben in `src/index.ts`.

---

## Aufgabe 1: POST /api/tasks implementieren

### Lösungsansatz

1. Extrahiere `title`, `description`, `priority` aus `req.body`
2. Generiere neue ID mit `uuidv4()`
3. Setze default `status = "pending"`
4. Füge Task in SQLite-Datenbank ein
5. Gebe den neuen Task mit Status `201 Created` zurück

### Wichtige Hinweise

- Verwende `uuidv4()` für die ID-Generierung
- Priority hat default-Wert "medium" wenn nicht angegeben
- Behandle Datenbankfehler mit 500 Status

### Code-Beispiel

```typescript
app.post('/api/tasks', (req: Request, res: Response) => {
  const { title, description, priority = 'medium' } = req.body;
  const id = uuidv4();
  const status = 'pending';

  db.run(
    `INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
     VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`,
    [id, title, description, status, priority],
    function(err) {
      if (err) {
        return res.status(500).json({ error: 'Failed to create task' });
      }
      // Hole den neuen Task zurück
      db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
        res.status(201).json(task);
      });
    }
  );
});
```

### Variante mit Callback

```typescript
app.post('/api/tasks', (req: Request, res: Response) => {
  const { title, description, priority = 'medium' } = req.body;
  const id = uuidv4();
  
  const sql = `INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
               VALUES (?, ?, ?, 'pending', ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`;
  
  db.run(sql, [id, title, description, priority], function(err) {
    if (err) return res.status(500).json({ error: err.message });
    
    // this.lastID würde bei INTEGER PRIMARY KEY funktionieren,
    // aber bei TEXT PRIMARY KEY müssen wir SELECT machen
    db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, row) => {
      if (err) return res.status(500).json({ error: err.message });
      res.status(201).json(row);
    });
  });
});
```

---

## Aufgabe 2: PUT /api/tasks/:id implementieren

### Lösungsansatz

1. Hole `id` aus `req.params.id`
2. Hole zu aktualisierende Felder aus `req.body`
3. Baue dynamisch das UPDATE-Statement (nur übergebene Felder)
4. Setze `updatedAt = CURRENT_TIMESTAMP`
5. Prüfe ob Task existiert (404 wenn nicht)
6. Gebe aktualisierten Task zurück

### Wichtige Hinweise

- Nur übergebene Felder aktualisieren (nicht alles)
- Prüfe ob mindestens ein Feld zum Aktualisieren vorhanden ist
- Verwende `this.changes` um zu prüfen ob etwas aktualisiert wurde
- Aktualisiere immer `updatedAt`

### Code-Beispiel

```typescript
app.put('/api/tasks/:id', (req: Request, res: Response) => {
  const { id } = req.params;
  const updates: string[] = [];
  const params: any[] = [];

  // Nur übergebene Felder sammeln
  if (req.body.title) {
    updates.push('title = ?');
    params.push(req.body.title);
  }
  if (req.body.description) {
    updates.push('description = ?');
    params.push(req.body.description);
  }
  if (req.body.status) {
    updates.push('status = ?');
    params.push(req.body.status);
  }
  if (req.body.priority) {
    updates.push('priority = ?');
    params.push(req.body.priority);
  }

  // Prüfe ob überhaupt Felder zum Aktualisieren vorhanden
  if (updates.length === 0) {
    return res.status(400).json({ error: 'No fields to update' });
  }

  // updatedAt immer aktualisieren
  updates.push('updatedAt = CURRENT_TIMESTAMP');
  params.push(id); // WHERE id = ?

  const query = `UPDATE tasks SET ${updates.join(', ')} WHERE id = ?`;

  db.run(query, params, function(err) {
    if (err) {
      return res.status(500).json({ error: 'Failed to update task' });
    }
    
    // Prüfe ob Task gefunden und aktualisiert wurde
    if (this.changes === 0) {
      return res.status(404).json({ error: 'Task not found' });
    }

    // Hole aktualisierten Task
    db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
      if (err) return res.status(500).json({ error: 'Failed to fetch task' });
      res.json(task);
    });
  });
});
```

### Alternative: Einfache Variante (nur Status aktualisieren)

```typescript
app.put('/api/tasks/:id', (req: Request, res: Response) => {
  const { id } = req.params;
  const { status } = req.body;

  db.run(
    'UPDATE tasks SET status = ?, updatedAt = CURRENT_TIMESTAMP WHERE id = ?',
    [status, id],
    function(err) {
      if (err) return res.status(500).json({ error: err.message });
      if (this.changes === 0) return res.status(404).json({ error: 'Task not found' });
      
      db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, row) => {
        res.json(row);
      });
    }
  );
});
```

---

## Aufgabe 3: DELETE /api/tasks/:id implementieren

### Lösungsansatz

1. Hole `id` aus `req.params.id`
2. Lösche Task aus der Datenbank
3. Prüfe mit `this.changes` ob ein Task gelöscht wurde
4. Gebe `204 No Content` bei Erfolg zurück
5. Gebe `404` wenn Task nicht gefunden

### Wichtige Hinweise

- DELETE gibt typischerweise 204 No Content zurück (kein Body)
- Verwende `this.changes` um zu prüfen ob Löschung erfolgreich war
- Kein SELECT nötig vor dem DELETE

### Code-Beispiel

```typescript
app.delete('/api/tasks/:id', (req: Request, res: Response) => {
  const { id } = req.params;

  db.run('DELETE FROM tasks WHERE id = ?', [id], function(err) {
    if (err) {
      return res.status(500).json({ error: 'Failed to delete task' });
    }
    
    // Prüfe ob Task gelöscht wurde
    if (this.changes === 0) {
      return res.status(404).json({ error: 'Task not found' });
    }

    // 204 No Content = Erfolg, kein Body
    res.status(204).send();
  });
});
```

---

## Hilfreiche SQLite Patterns

### Prüfen ob Eintrag existiert

```typescript
db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
  if (!task) {
    return res.status(404).json({ error: 'Task not found' });
  }
  // ... weiter verarbeiten
});
```

### Error Handling Pattern

```typescript
db.run(query, params, function(err) {
  if (err) {
    console.error('Database error:', err.message);
    return res.status(500).json({ error: 'Database operation failed' });
  }
  // ... Erfolg
});
```

### Aktuelle Zeit in SQLite

```sql
-- Automatisch setzen:
CREATE TABLE tasks (
  createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
  updatedAt TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Manuelles Update:
UPDATE tasks SET updatedAt = CURRENT_TIMESTAMP WHERE id = ?;
```

---

## Häufige Fehler vermeiden

1. **Nicht vergessen:** `updatedAt` bei PUT aktualisieren
2. **ID Generierung:** Immer `uuidv4()` verwenden, nicht selbst erstellen
3. **Status Codes:** 
   - 201 für POST (Created)
   - 200 für PUT/GET (OK)
   - 204 für DELETE (No Content)
   - 404 wenn nicht gefunden
   - 500 bei Datenbankfehlern
4. **this.changes:** Funktioniert nur in `function(err)`, nicht in Arrow-Funktionen!
