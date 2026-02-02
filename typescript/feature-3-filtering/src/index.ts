import express, { Application, Request, Response } from 'express';
import sqlite3 from 'sqlite3';
import path from 'path';
import { v4 as uuidv4 } from 'uuid';
import { body, validationResult } from 'express-validator';

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
          ['task-3', 'Write docs', 'Document all endpoints', 'pending', 'medium'],
          ['task-4', 'Test API', 'Write integration tests', 'pending', 'low'],
          ['task-5', 'Deploy', 'Deploy to production', 'in_progress', 'medium']
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
// BEREITS IMPLEMENTIERT - aus Feature 1 & 2
// ============================================================

// GET all tasks - BEREITS IMPLEMENTIERT
app.get('/api/tasks', (req: Request, res: Response) => {
  // ============================================================
  // HIER BEGINNT DEINE AUFGABE - Query Parameter auswerten
  // ============================================================
  // TODO: Implementiere Filterung mit Query-Parametern:
  //       - ?status=pending - Filter nach Status
  //       - ?priority=high - Filter nach Priority
  //       - ?search=keyword - Suche in title und description
  //       - Kombinationen sollten möglich sein: ?status=pending&priority=high
  //
  // TIPP: Verwende req.query um auf die Parameter zuzugreifen
  //       Baue die SQL-Query dynamisch auf basierend auf den Parametern
  
  db.all('SELECT * FROM tasks', (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});

// GET single task - BEREITS IMPLEMENTIERT
app.get('/api/tasks/:id', (req: Request, res: Response) => {
  db.get('SELECT * FROM tasks WHERE id = ?', [req.params.id], (err, task) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch task' });
    if (!task) return res.status(404).json({ error: 'Task not found' });
    res.json(task);
  });
});

// POST with validation - BEREITS IMPLEMENTIERT
const validateCreateTask = [
  body('title').notEmpty().withMessage('Title is required')
               .isLength({ max: 200 }).withMessage('Title max 200 chars'),
  body('description').notEmpty().withMessage('Description is required'),
  body('priority').optional().isIn(['low', 'medium', 'high'])
                  .withMessage('Invalid priority')
];

app.post('/api/tasks', validateCreateTask, (req: Request, res: Response) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }

  const { title, description, priority = 'medium' } = req.body;
  const id = uuidv4();
  const status = 'pending';

  db.run(
    `INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt) 
     VALUES (?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`,
    [id, title, description, status, priority],
    function(err) {
      if (err) return res.status(500).json({ error: 'Failed to create task' });
      db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
        res.status(201).json(task);
      });
    }
  );
});

// PUT with validation - BEREITS IMPLEMENTIERT
const validateUpdateTask = [
  body('title').optional().isLength({ max: 200 }).withMessage('Title max 200 chars'),
  body('status').optional().isIn(['pending', 'in_progress', 'completed'])
                .withMessage('Invalid status'),
  body('priority').optional().isIn(['low', 'medium', 'high'])
                  .withMessage('Invalid priority')
];

app.put('/api/tasks/:id', validateUpdateTask, (req: Request, res: Response) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }

  const { id } = req.params;
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
    if (this.changes === 0) return res.status(404).json({ error: 'Task not found' });
    db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
      res.json(task);
    });
  });
});

// DELETE - BEREITS IMPLEMENTIERT
app.delete('/api/tasks/:id', (req: Request, res: Response) => {
  const { id } = req.params;
  db.run('DELETE FROM tasks WHERE id = ?', [id], function(err) {
    if (err) return res.status(500).json({ error: 'Failed to delete task' });
    if (this.changes === 0) return res.status(404).json({ error: 'Task not found' });
    res.status(204).send();
  });
});

// Error handlers
app.use((req: Request, res: Response) => res.status(404).json({ error: 'Not found' }));
app.use((err: Error, req: Request, res: Response, next: Function) => res.status(500).json({ error: 'Server error' }));

app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
