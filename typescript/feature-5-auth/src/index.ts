import express, { Application, Request, Response, NextFunction } from 'express';
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
    // Tasks table
    db.run(`
      CREATE TABLE IF NOT EXISTS tasks (
        id TEXT PRIMARY KEY,
        title TEXT NOT NULL,
        description TEXT NOT NULL,
        status TEXT DEFAULT 'pending',
        priority TEXT DEFAULT 'medium',
        userId TEXT,
        createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
        updatedAt TEXT DEFAULT CURRENT_TIMESTAMP
      )
    `);

    // Users table for auth
    db.run(`
      CREATE TABLE IF NOT EXISTS users (
        id TEXT PRIMARY KEY,
        username TEXT UNIQUE NOT NULL,
        password TEXT NOT NULL,
        createdAt TEXT DEFAULT CURRENT_TIMESTAMP
      )
    `);

    // Insert sample users if empty
    db.get('SELECT COUNT(*) as count FROM users', (err, row: { count: number }) => {
      if (row.count === 0) {
        // Password: "password123" (in production, this would be hashed!)
        db.run(`INSERT INTO users VALUES (?, ?, ?, CURRENT_TIMESTAMP)`,
          ['user-1', 'alice', 'password123']);
        db.run(`INSERT INTO users VALUES (?, ?, ?, CURRENT_TIMESTAMP)`,
          ['user-2', 'bob', 'password123']);
      }
    });

    // Insert sample tasks if empty
    db.get('SELECT COUNT(*) as count FROM tasks', (err, row: { count: number }) => {
      if (row.count === 0) {
        const tasks = [
          ['task-1', 'Learn TypeScript', 'Complete TypeScript basics', 'completed', 'high', 'user-1'],
          ['task-2', 'Build REST API', 'Create Task API', 'in_progress', 'high', 'user-1'],
          ['task-3', 'Write docs', 'Document all endpoints', 'pending', 'medium', 'user-1'],
          ['task-4', 'Test API', 'Write integration tests', 'pending', 'low', 'user-2'],
          ['task-5', 'Deploy', 'Deploy to production', 'in_progress', 'medium', 'user-2']
        ];
        tasks.forEach(([id, title, desc, status, priority, userId]) => {
          db.run(`INSERT INTO tasks (id, title, description, status, priority, userId, createdAt, updatedAt) 
                  VALUES (?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)`,
            [id, title, desc, status, priority, userId]);
        });
      }
    });
  });
}

// ============================================================
// BEREITS IMPLEMENTIERT - aus Features 1-4
// ============================================================

// GET all tasks with filtering and pagination - BEREITS IMPLEMENTIERT
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status, priority, search, page = '1', limit = '10' } = req.query;
  
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
  
  // Pagination
  const pageNum = parseInt(page as string, 10) || 1;
  const limitNum = parseInt(limit as string, 10) || 10;
  const offset = (pageNum - 1) * limitNum;
  
  query += ' LIMIT ? OFFSET ?';
  params.push(limitNum, offset);
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    
    // Get total count for pagination meta
    let countQuery = 'SELECT COUNT(*) as count FROM tasks';
    if (conditions.length > 0) {
      countQuery += ' WHERE ' + conditions.slice(0, -2).join(' AND '); // Remove LIMIT params
    }
    
    db.get(countQuery, params.slice(0, -2), (err, row: { count: number }) => {
      const total = row ? row.count : 0;
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

// ============================================================
// AUFGABE: AUTHENTIFIZIERUNG IMPLEMENTIEREN
// ============================================================
// TODO: AUTH - Login Endpunkt implementieren
//       - POST /api/auth/login
//       - Prüfe username und password in users-Tabelle
//       - Bei Erfolg: return { userId, username }
//       - Bei Fehler: 401 Unauthorized

// TODO: AUTH - Middleware zum Schützen von Endpunkten
//       - Erstelle authMiddleware
//       - Prüfe Authorization Header
//       - Für dieses Feature: Einfache Basic Auth oder Token
//       - Tasks sollen nur für eingeloggte User sichtbar sein

// TODO: AUTH - Tasks mit User verknüpfen
//       - Bei POST /api/tasks: Setze userId aus dem eingeloggten User
//       - Bei GET /api/tasks: Zeige nur Tasks des eingeloggten Users
//       - Bei PUT/DELETE: Prüfe ob der Task dem eingeloggten User gehört

// Login placeholder - TODO implementieren
app.post('/api/auth/login', (req: Request, res: Response) => {
  // TODO: Implementiere Login
  // 1. Hole username und password aus req.body
  // 2. Prüfe in users-Tabelle
  // 3. Bei Erfolg: return { userId, username }
  // 4. Bei Fehler: 401 Unauthorized
  res.status(501).json({ error: 'Not implemented yet - implement login here' });
});

// Error handlers
app.use((req: Request, res: Response) => res.status(404).json({ error: 'Not found' }));
app.use((err: Error, req: Request, res: Response, next: NextFunction) => res.status(500).json({ error: 'Server error' }));

app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
