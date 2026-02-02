import express, { Application, Request, Response } from 'express';
import sqlite3 from 'sqlite3';
import path from 'path';
import { v4 as uuidv4 } from 'uuid';

const app: Application = express();
const PORT = process.env.PORT || 3000;
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
// TIPP: Siehe hint.http fuer Loesung
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
