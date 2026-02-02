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
          ['task-1', 'Learn TypeScript basics', 'Complete TypeScript fundamentals course', 'completed', 'high'],
          ['task-2', 'Build REST API', 'Create Task API with Express', 'in_progress', 'high'],
          ['task-3', 'Write documentation', 'Document all API endpoints', 'pending', 'medium'],
          ['task-4', 'Write unit tests', 'Implement Jest tests for API', 'pending', 'low'],
          ['task-5', 'Deploy to production', 'Deploy API to cloud server', 'in_progress', 'medium'],
          ['task-6', 'Setup CI/CD pipeline', 'Configure GitHub Actions for deployment', 'pending', 'high'],
          ['task-7', 'Add authentication', 'Implement JWT-based auth', 'pending', 'high'],
          ['task-8', 'Create database schema', 'Design SQLite schema for tasks', 'completed', 'medium'],
          ['task-9', 'Setup development environment', 'Configure Docker and VSCode', 'completed', 'low'],
          ['task-10', 'Review code', 'Perform code review for PRs', 'in_progress', 'low'],
          ['task-11', 'Update dependencies', 'Keep npm packages up to date', 'completed', 'low'],
          ['task-12', 'Fix bugs', 'Address reported issues from QA', 'pending', 'medium'],
          ['task-13', 'Optimize performance', 'Improve API response times', 'pending', 'medium'],
          ['task-14', 'Add logging', 'Implement structured logging', 'pending', 'low'],
          ['task-15', 'Create API examples', 'Write usage examples for clients', 'completed', 'low']
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
// BEREITS IMPLEMENTIERT - aus Feature 1, 2 & 3
// ============================================================

// GET all tasks - BEREITS MIT FILTER (aus Feature 3)
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status, priority, search } = req.query;
  
  let query = 'SELECT * FROM tasks';
  const params: any[] = [];
  const conditions: string[] = [];
  
  if (status) {
    conditions.push('status = ?');
    params.push(status);
  }
  
  if (priority) {
    conditions.push('priority = ?');
    params.push(priority);
  }
  
  if (search) {
    conditions.push('(title LIKE ? OR description LIKE ?)');
    const searchPattern = `%${search}%`;
    params.push(searchPattern, searchPattern);
  }
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    
    // ============================================================
    // HIER BEGINNT DEINE AUFGABE - Pagination implementieren
    // ============================================================
    // TODO: Implementiere Pagination mit Query-Parametern:
    //       - ?page=1 - Aktuelle Seite (default: 1)
    //       - ?limit=5 - Elemente pro Seite (default: 10)
    //       
    // Das Response sollte folgende Struktur haben:
    // {
    //   "data": [...],       // Die Tasks auf dieser Seite
    //   "pagination": {
    //     "page": 1,
    //     "limit": 5,
    //     "totalItems": 15,
    //     "totalPages": 3,
    //     "hasNextPage": true,
    //     "hasPreviousPage": false
    //   }
    // }
    //
    // TIPP: 
    // 1. Extrahiere page und limit aus req.query
    // 2. Berechne offset: offset = (page - 1) * limit
    // 3. Füge LIMIT und OFFSET zur SQL-Query hinzu
    // 4. Hole die Gesamtanzahl der Tasks für totalItems
    // 5. Baue das Pagination-Objekt und sende es zurueck
    
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
