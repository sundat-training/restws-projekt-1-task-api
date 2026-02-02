# Lösungshinweise - Feature 2: Validation

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Validierung in `src/index.ts`.

---

## Aufgabe 1: POST Validierung implementieren

### Lösungsansatz

1. Installiere `express-validator`: `npm install express-validator`
2. Importiere `body` und `validationResult`
3. Erstelle Validierungs-Chain für POST
4. Wende Validierung im Route-Handler an
5. Prüfe auf Fehler und gib 400 zurück

### Installation

```bash
npm install express-validator
```

### Import

```typescript
import { body, validationResult } from 'express-validator';
```

### Lösung: Validierungs-Regeln

```typescript
// Validierungs-Regeln für POST
const validateCreateTask = [
  body('title')
    .notEmpty()
    .withMessage('Title is required')
    .isLength({ max: 200 })
    .withMessage('Title must be at most 200 characters'),
  
  body('description')
    .notEmpty()
    .withMessage('Description is required'),
  
  body('priority')
    .optional()
    .isIn(['low', 'medium', 'high'])
    .withMessage('Priority must be low, medium, or high')
];
```

### Lösung: Route mit Validierung

```typescript
app.post('/api/tasks', validateCreateTask, (req: Request, res: Response) => {
  // Prüfe auf Validierungsfehler
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }

  // Keine Fehler - Task erstellen
  const { title, description, priority = 'medium' } = req.body;
  const id = uuidv4();
  const status = 'pending';

  db.run(
    `INSERT INTO tasks (id, title, description, status, priority) VALUES (?, ?, ?, ?, ?)`,
    [id, title, description, status, priority],
    function(err) {
      if (err) return res.status(500).json({ error: 'Failed to create task' });
      db.get('SELECT * FROM tasks WHERE id = ?', [id], (err, task) => {
        res.status(201).json(task);
      });
    }
  );
});
```

### Erwartetes Fehler-Format

```json
{
  "errors": [
    {
      "type": "field",
      "msg": "Title is required",
      "path": "title",
      "location": "body"
    }
  ]
}
```

### Alternative: Eigene Fehler-Formatierung

```typescript
app.post('/api/tasks', validateCreateTask, (req: Request, res: Response) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    // Formatiere Fehler um
    const formattedErrors = errors.array().map(err => ({
      type: 'field',
      msg: err.msg,
      path: err.path,
      location: err.location
    }));
    return res.status(400).json({ errors: formattedErrors });
  }
  // ... restlicher Code
});
```

---

## Aufgabe 2: PUT Validierung implementieren

### Lösungsansatz

1. Erstelle Validierungs-Chain für PUT
2. Alle Felder sind optional (nur wenn vorhanden validieren)
3. Wende Validierung an
4. Behandle Fehler wie bei POST

### Lösung: Validierungs-Regeln

```typescript
// Validierungs-Regeln für PUT (alle optional)
const validateUpdateTask = [
  body('title')
    .optional()
    .isLength({ max: 200 })
    .withMessage('Title must be at most 200 characters'),
  
  body('description')
    .optional()
    .notEmpty()
    .withMessage('Description cannot be empty if provided'),
  
  body('status')
    .optional()
    .isIn(['pending', 'in_progress', 'completed'])
    .withMessage('Status must be pending, in_progress, or completed'),
  
  body('priority')
    .optional()
    .isIn(['low', 'medium', 'high'])
    .withMessage('Priority must be low, medium, or high')
];
```

### Lösung: Route mit Validierung

```typescript
app.put('/api/tasks/:id', validateUpdateTask, (req: Request, res: Response) => {
  // Prüfe auf Validierungsfehler
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({ errors: errors.array() });
  }

  // Rest der PUT-Logik wie in Feature 1
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
```

---

## Häufige express-validator Validatoren

### String-Validierung

```typescript
body('field')
  .isString()
  .withMessage('Must be a string')
  .notEmpty()
  .withMessage('Cannot be empty')
  .trim()  // Entfernt Leerzeichen am Anfang/Ende
  .escape()  // Escaped HTML
```

### Längen-Validierung

```typescript
body('field')
  .isLength({ min: 5, max: 200 })
  .withMessage('Must be between 5 and 200 characters')
```

### Enum-Validierung

```typescript
body('status')
  .isIn(['pending', 'in_progress', 'completed'])
  .withMessage('Invalid status')
```

### Optionale Felder

```typescript
body('field')
  .optional()  // Feld muss nicht vorhanden sein
  .notEmpty()
  .withMessage('If provided, cannot be empty')
```

### Standardwerte

```typescript
body('priority')
  .optional()
  .default('medium')  // Setzt default wenn nicht vorhanden oder leer
```

---

## Custom Validatoren

### Eigene Validierungs-Funktion

```typescript
body('title').custom((value, { req }) => {
  if (value && value.includes('forbidden')) {
    throw new Error('Title contains forbidden word');
  }
  return true;
})
```

### Async Validierung (Datenbank-Prüfung)

```typescript
body('username').custom(async (value) => {
  const user = await User.findOne({ username: value });
  if (user) {
    throw new Error('Username already exists');
  }
}),
```

---

## Hilfreiche Patterns

### Wiederverwendbare Validierungs-Regeln

```typescript
// In separate Datei auslagern: validators/taskValidators.ts
export const taskTitleValidation = body('title')
  .notEmpty()
  .withMessage('Title is required')
  .isLength({ max: 200 })
  .withMessage('Title too long');

export const taskPriorityValidation = body('priority')
  .optional()
  .isIn(['low', 'medium', 'high'])
  .withMessage('Invalid priority');
```

### Zentrale Fehlerbehandlung

```typescript
// In Middleware auslagern
const handleValidationErrors = (req: Request, res: Response, next: NextFunction) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    return res.status(400).json({
      errors: errors.array().map(err => ({
        type: 'field',
        msg: err.msg,
        path: err.path,
        location: err.location
      }))
    });
  }
  next();
};

// Anwendung
app.post('/api/tasks', validateCreateTask, handleValidationErrors, (req, res) => {
  // Hier sind die Daten validiert
});
```

---

## Häufige Fehler vermeiden

1. **Nicht vergessen:** `validationResult(req)` aufrufen und prüfen
2. **Reihenfolge:** Validierungs-Chain kommt VOR dem Route-Handler
3. **Optionale Felder:** Mit `.optional()` markieren, nicht nur weglassen
4. **Fehlermeldungen:** Aussagekräftige Messages mit `.withMessage()`
5. **Status Code:** Immer 400 bei Validierungsfehlern
