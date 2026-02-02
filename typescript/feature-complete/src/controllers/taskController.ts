import { Response } from 'express';
import { db } from '../models/database';
import { AuthRequest } from '../middleware/auth';
import { Task, CreateTaskDTO, UpdateTaskDTO, TaskFilters, PaginationParams } from '../models/task.model';
import { v4 as uuidv4 } from 'uuid';

export class TaskController {
  static getAllTasks(req: AuthRequest, res: Response): void {
    const { status, priority, search } = req.query as TaskFilters;
    const { page = '1', limit = '10' } = req.query as PaginationParams;
    
    const pageNum = parseInt(page);
    const limitNum = parseInt(limit);
    const offset = (pageNum - 1) * limitNum;

    let query = 'SELECT * FROM tasks WHERE 1=1';
    const params: string[] = [];

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
      params.push(`%${search}%`, `%${search}%`);
    }

    db.all(query, params, (err, tasks: Task[]) => {
      if (err) {
        res.status(500).json({ error: 'Failed to fetch tasks' });
        return;
      }

      const total = tasks.length;
      const paginatedTasks = tasks.slice(offset, offset + limitNum);

      res.json({
        data: paginatedTasks,
        pagination: {
          page: pageNum,
          limit: limitNum,
          total,
          totalPages: Math.ceil(total / limitNum)
        }
      });
    });
  }

  static getTaskById(req: AuthRequest, res: Response): void {
    const { id } = req.params;

    db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task: Task) => {
      if (err) {
        res.status(500).json({ error: 'Failed to fetch task' });
        return;
      }

      if (!task) {
        res.status(404).json({ error: 'Task not found' });
        return;
      }

      res.json(task);
    });
  }

  static createTask(req: AuthRequest, res: Response): void {
    const { title, description, priority = 'medium' } = req.body as CreateTaskDTO;
    const id = uuidv4();
    const userId = req.user?.id;

    db.run(
      `INSERT INTO tasks (id, title, description, status, priority, userId) VALUES (?, ?, ?, 'pending', ?, ?)`,
      [id, title, description, priority, userId],
      function(err) {
        if (err) {
          res.status(500).json({ error: 'Failed to create task' });
          return;
        }

        db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task: Task) => {
          res.status(201).json(task);
        });
      }
    );
  }

  static updateTask(req: AuthRequest, res: Response): void {
    const { id } = req.params;
    const { title, description, status, priority } = req.body as UpdateTaskDTO;

    const updates: string[] = [];
    const params: string[] = [];

    if (title !== undefined) {
      updates.push('title = ?');
      params.push(title);
    }
    if (description !== undefined) {
      updates.push('description = ?');
      params.push(description);
    }
    if (status !== undefined) {
      updates.push('status = ?');
      params.push(status);
    }
    if (priority !== undefined) {
      updates.push('priority = ?');
      params.push(priority);
    }

    if (updates.length === 0) {
      res.status(400).json({ error: 'No fields to update' });
      return;
    }

    updates.push('updatedAt = CURRENT_TIMESTAMP');
    params.push(id);

    db.run(
      `UPDATE tasks SET ${updates.join(', ')} WHERE id = ?`,
      params,
      function(err) {
        if (err) {
          res.status(500).json({ error: 'Failed to update task' });
          return;
        }

        if (this.changes === 0) {
          res.status(404).json({ error: 'Task not found' });
          return;
        }

        db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task: Task) => {
          res.json(task);
        });
      }
    );
  }

  static deleteTask(req: AuthRequest, res: Response): void {
    const { id } = req.params;

    db.run('DELETE FROM tasks WHERE id = ?', [id], function(err) {
      if (err) {
        res.status(500).json({ error: 'Failed to delete task' });
        return;
      }

      if (this.changes === 0) {
        res.status(404).json({ error: 'Task not found' });
        return;
      }

      res.status(204).send();
    });
  }
}
