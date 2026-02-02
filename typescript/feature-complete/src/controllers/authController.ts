import { Response } from 'express';
import { db } from '../models/database';
import { AuthRequest } from '../middleware/auth';
import { CreateUserDTO, LoginDTO, AuthResponse } from '../models/user.model';
import { v4 as uuidv4 } from 'uuid';
import bcrypt from 'bcryptjs';
import { generateToken } from '../middleware/auth';

export class AuthController {
  static register(req: AuthRequest, res: Response): void {
    const { username, email, password } = req.body as CreateUserDTO;
    const id = uuidv4();
    const hashedPassword = bcrypt.hashSync(password, 10);

    db.run(
      `INSERT INTO users (id, username, email, password) VALUES (?, ?, ?, ?)`,
      [id, username, email, hashedPassword],
      function(err) {
        if (err) {
          if (err.message.includes('UNIQUE constraint')) {
            res.status(400).json({ error: 'Username or email already exists' });
            return;
          }
          res.status(500).json({ error: 'Failed to register user' });
          return;
        }

        const user = { id, username, email, createdAt: new Date().toISOString() };
        const token = generateToken({ id, email, username });

        res.status(201).json({ user, token } as AuthResponse);
      }
    );
  }

  static login(req: AuthRequest, res: Response): void {
    const { email, password } = req.body as LoginDTO;

    db.get('SELECT * FROM users WHERE email = ?', [email], (err, user: any) => {
      if (err) {
        res.status(500).json({ error: 'Failed to login' });
        return;
      }

      if (!user) {
        res.status(401).json({ error: 'Invalid credentials' });
        return;
      }

      const validPassword = bcrypt.compareSync(password, user.password);
      if (!validPassword) {
        res.status(401).json({ error: 'Invalid credentials' });
        return;
      }

      const safeUser = { id: user.id, username: user.username, email: user.email };
      const token = generateToken(safeUser);

      res.json({ user: safeUser, token } as AuthResponse);
    });
  }

  static getProfile(req: AuthRequest, res: Response): void {
    const user = req.user;
    if (!user) {
      res.status(404).json({ error: 'User not found' });
      return;
    }

    db.get('SELECT id, username, email, createdAt FROM users WHERE id = ?', [user.id], (err, userData: any) => {
      res.json(userData);
    });
  }
}
