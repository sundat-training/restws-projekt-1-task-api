import { Router } from 'express';
import { AuthController } from '../controllers/authController';
import {
  registerValidation,
  loginValidation,
  handleValidationErrors
} from '../middleware/validation';

const router = Router();

router.post('/register', registerValidation, handleValidationErrors, AuthController.register);
router.post('/login', loginValidation, handleValidationErrors, AuthController.login);
router.get('/profile', AuthController.getProfile);

export default router;
