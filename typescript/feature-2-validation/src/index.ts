import express, { Application, Request, Response } from 'express';
import sqlite3 from 'sqlite3';
import path from 'path';
import { v4 as uuidv4 } from 'uuid';
// TODO: Importiere express-validator
// import { body, validationResult } from 'express-validator';

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

// ============================================================
// TODO AUFGABE 1: POST Validierung implementieren
// ============================================================
// Erstelle Validierungs-Regeln für POST /api/tasks:
// - title: Pflichtfeld, max 200 Zeichen
// - description: Pflichtfeld
// - priority: Optional, muss 'low', 'medium' oder 'high' sein
//
// Tipp: Siehe HINTS.md für Code-Beispiele

// const createTaskValidation = [
//   body('title').notEmpty().withMessage('Title is required').isLength({ max: 200 }),
//   body('description').notEmpty().withMessage('Description is required'),
//   body('priority').optional().isIn(['low', 'medium', 'high'])
// ];

// ============================================================
// TODO AUFGABE 2: PUT Validierung implementieren
// ============================================================
// Erstelle Validierungs-Regeln für PUT /api/tasks/:id:
// - title: Optional, max 200 Zeichen
// - status: Optional, muss 'pending', 'in_progress' oder 'completed' sein
// - priority: Optional, muss 'low', 'medium' oder 'high' sein
//
// Tipp: Siehe HINTS.md für Code-Beispiele

// const updateTaskValidation = [
//   body('title').optional().isLength({ max: 200 }),
//   body('status').optional().isIn(['pending', 'in_progress', 'completed']),
//   body('priority').optional().isIn(['low', 'medium', 'high'])
// ];

// ============================================================
// TODO AUFGABE 3: Fehlerbehandlung implementieren
// ============================================================
// Erstelle Middleware für Validierungsfehler:
// - Prüfe validationResult(req)
// - Bei Fehlern: return 400 Bad Request mit errors-Array
//
// Tipp: Siehe HINTS.md für Code-Beispiele

// const handleValidationErrors = (req: Request, res: Response, next: Function): void => {
//   const errors = validationResult(req);
//   if (!errors.isEmpty()) {
//     res.status(400).json({ errors: errors.array() });
//     return;
//   }
//   next();
// };

// ============================================================
// BEREITS IMPLEMENTIERT - Referenz aus Feature 1
// ============================================================

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
// TODO: POST mit Validierung
// ============================================================
// Füge Validierung hinzu: app.post('/api/tasks', createTaskValidation, handleValidationErrors, ...)
// Die Handler-Logik bleibt gleich, aber vorher validieren!

app.post('/api/tasks', (req: Request, res: Response) => {
  // TODO: Prüfe Validierungsfehler hier

  const { title, description, priority = 'medium' } = req.body;
  const id = uuidv4();
  db.run(`INSERT INTO tasks VALUES (?, ?, ?, 'pending', ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`,
    [id, title, description, priority], function(err) {
      if (err) return res.status(500).json({ error: 'Failed to create task' });
      db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => res.status(201).json(task));
    });
});

// ============================================================
// TODO: PUT mit Validierung
// ============================================================
// Füge Validierung hinzu: app.put('/api/tasks/:id', updateTaskValidation, handleValidationErrors, ...)
// Die Handler-Logik bleibt gleich, aber vorher validieren!

app.put('/api/tasks/:id', (req: Request, res: Response) => {
  // TODO: Prüfe Validierungsfehler hier

  const { title, description, status, priority } = req.body;
  const updates: string[] = [];
  const params: string[] = [];
  if (title !== undefined) { updates.push('title = ?'); params.push(title); }
  if (description !== undefined) { updates.push('description = ?'); params.push(description); }
  if (status !== undefined) { updates.push('status = ?'); params.push(status); }
  if (priority !== undefined) { updates.push('priority = ?'); params.push(priority); }
  updates.push('updatedAt = CURRENT_TIMESTAMP');
  params.push(req.params.id);
  if (updates.length === 1) return res.status(400).json({ error: 'No fields to update' });
  db.run(`UPDATE tasks SET ${updates.join(', ')} WHERE id = ?`, params, function(err) {
    if (err) return res.status(500).json({ error: 'Failed to update task' });
    if (this.changes === 0) return res.status(404).json({ error: 'Task not found' });
    db.get('SELECT * FROM tasks WHERE id = ?', [req.params.id], (err, task) => res.json(task));
  });
});

// ============================================================
// BEREITS IMPLEMENTIERT - DELETE aus Feature 1
// ============================================================

app.delete('/api/tasks/:id', (req: Request, res: Response) => {
  db.run('DELETE FROM tasks WHERE id = ?', [req.params.id], function(err) {
    if (err) return res.status(500).json({ error: 'Failed to delete task' });
    if (this.changes === 0) return res.status(404).json({ error: 'Task not found' });
    res.status(204).send();
  });
});

app.use((req: Request, res: Response) => res.status(404).json({ error: 'Not found' }));
app.use((err: Error, req: Request, res: Response, next: Function) => res.status(500).json({ error: 'Server error' }));

app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
