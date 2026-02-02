import sqlite3 from 'sqlite3';
import path from 'path';

const dbPath = path.join(__dirname, '../../task-api.db');

export const db = new sqlite3.Database(dbPath, (err) => {
  if (err) {
    console.error('Error opening database:', err.message);
  } else {
    console.log('Connected to SQLite database');
    initializeDatabase();
  }
});

function initializeDatabase() {
  db.serialize(() => {
    db.run(`
      CREATE TABLE IF NOT EXISTS users (
        id TEXT PRIMARY KEY,
        username TEXT UNIQUE NOT NULL,
        email TEXT UNIQUE NOT NULL,
        password TEXT NOT NULL,
        createdAt TEXT DEFAULT CURRENT_TIMESTAMP
      )
    `);

    db.run(`
      CREATE TABLE IF NOT EXISTS tasks (
        id TEXT PRIMARY KEY,
        title TEXT NOT NULL,
        description TEXT NOT NULL,
        status TEXT DEFAULT 'pending',
        priority TEXT DEFAULT 'medium',
        createdAt TEXT DEFAULT CURRENT_TIMESTAMP,
        updatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
        userId TEXT,
        FOREIGN KEY (userId) REFERENCES users(id)
      )
    `);

    insertSampleData();
  });
}

function insertSampleData() {
  const checkData = `SELECT COUNT(*) as count FROM users`;
  
  db.get(checkData, (err, row: { count: number }) => {
    if (row.count === 0) {
      const bcrypt = require('bcryptjs');
      const hashedPassword = bcrypt.hashSync('password123', 10);
      
      db.run(`
        INSERT INTO users (id, username, email, password, createdAt)
        VALUES ('user-1', 'admin', 'admin@example.com', '${hashedPassword}', CURRENT_TIMESTAMP)
      `);

      const tasks = [
        ['task-1', 'Learn TypeScript', 'Complete TypeScript basics course', 'completed', 'high'],
        ['task-2', 'Build REST API', 'Create Task Management API', 'in_progress', 'high'],
        ['task-3', 'Write documentation', 'Document all API endpoints', 'pending', 'medium'],
        ['task-4', 'Setup CI/CD', 'Configure GitHub Actions', 'pending', 'low'],
        ['task-5', 'Code review', 'Review pull requests', 'completed', 'medium']
      ];

      tasks.forEach(([id, title, desc, status, priority]) => {
        db.run(`
          INSERT INTO tasks (id, title, description, status, priority, createdAt, updatedAt, userId)
          VALUES ('${id}', '${title}', '${desc}', '${status}', '${priority}', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, 'user-1')
        `);
      });
    }
  });
}
