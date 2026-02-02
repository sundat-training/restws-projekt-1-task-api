import { Router } from 'express';
import { TaskController } from '../controllers/taskController';
import { authenticateToken } from '../middleware/auth';
import {
  createTaskValidation,
  updateTaskValidation,
  handleValidationErrors
} from '../middleware/validation';

const router = Router();

router.use(authenticateToken);

router.get('/', TaskController.getAllTasks);
router.get('/:id', TaskController.getTaskById);
router.post('/', createTaskValidation, handleValidationErrors, TaskController.createTask);
router.put('/:id', updateTaskValidation, handleValidationErrors, TaskController.updateTask);
router.delete('/:id', TaskController.deleteTask);

export default router;
