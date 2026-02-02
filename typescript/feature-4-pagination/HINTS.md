# Lösungshinweise - Feature 4: Pagination

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Pagination in `src/index.ts`.

---

## Aufgabe 1: Pagination mit LIMIT und OFFSET implementieren

### Lösungsansatz

1. Extrahiere `page` und `limit` aus Query-Parametern
2. Berechne `offset = (page - 1) * limit`
3. Füge `LIMIT ? OFFSET ?` zur SQL-Query hinzu
4. Führe Query aus mit den berechneten Werten

### Standardwerte setzen

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  // Standardwerte: page=1, limit=10
  const page = parseInt(req.query.page as string) || 1;
  const limit = parseInt(req.query.limit as string) || 10;
  
  // Berechne offset
  const offset = (page - 1) * limit;
});
```

### Lösung: Pagination in SQL

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status, priority, search } = req.query;
  
  // Pagination Parameter
  const page = parseInt(req.query.page as string) || 1;
  const limit = parseInt(req.query.limit as string) || 10;
  const offset = (page - 1) * limit;
  
  // Build base query
  let query = 'SELECT * FROM tasks';
  const params: any[] = [];
  const conditions: string[] = [];
  
  // Filter (wie in Feature 3)
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
    const pattern = `%${search}%`;
    params.push(pattern, pattern);
  }
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  // Pagination hinzufügen
  query += ' LIMIT ? OFFSET ?';
  params.push(limit, offset);
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});
```

### Maximales Limit setzen

```typescript
// Sicherstellen dass limit nicht zu groß ist
const maxLimit = 100;
const limit = Math.min(parseInt(req.query.limit as string) || 10, maxLimit);
```

---

## Aufgabe 2: Meta-Daten für Pagination

### Lösungsansatz

1. Führe zusätzliche Query für Gesamtzahl aus
2. Berechne `totalPages = Math.ceil(total / limit)`
3. Füge Meta-Daten zur Response hinzu
4. Gebe Tasks in `data` Feld zurück

### Lösung: Meta-Daten berechnen

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status, priority, search } = req.query;
  const page = parseInt(req.query.page as string) || 1;
  const limit = parseInt(req.query.limit as string) || 10;
  const offset = (page - 1) * limit;
  
  // Build WHERE clause
  const conditions: string[] = [];
  const filterParams: any[] = [];
  
  if (status) {
    conditions.push('status = ?');
    filterParams.push(status);
  }
  if (priority) {
    conditions.push('priority = ?');
    filterParams.push(priority);
  }
  if (search) {
    conditions.push('(title LIKE ? OR description LIKE ?)');
    const pattern = `%${search}%`;
    filterParams.push(pattern, pattern);
  }
  
  const whereClause = conditions.length > 0 
    ? 'WHERE ' + conditions.join(' AND ')
    : '';
  
  // 1. Zuerst Gesamtzahl ermitteln
  const countQuery = `SELECT COUNT(*) as count FROM tasks ${whereClause}`;
  
  db.get(countQuery, filterParams, (err, row: { count: number }) => {
    if (err) return res.status(500).json({ error: 'Failed to count tasks' });
    
    const total = row.count;
    const totalPages = Math.ceil(total / limit);
    
    // 2. Dann Tasks für aktuelle Page holen
    const query = `SELECT * FROM tasks ${whereClause} LIMIT ? OFFSET ?`;
    const params = [...filterParams, limit, offset];
    
    db.all(query, params, (err, tasks) => {
      if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
      
      res.json({
        data: tasks,
        meta: {
          page,
          limit,
          total,
          totalPages,
          hasNextPage: page < totalPages,
          hasPrevPage: page > 1
        }
      });
    });
  });
});
```

### Erwartetes Response-Format

```json
{
  "data": [
    { "id": "task-1", "title": "...", "status": "..." },
    { "id": "task-2", "title": "...", "status": "..." }
  ],
  "meta": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPrevPage": false
  }
}
```

---

## Hilfreiche Patterns

### Pagination Helper Funktion

```typescript
interface PaginationParams {
  page: number;
  limit: number;
  offset: number;
}

const getPagination = (req: Request, defaultLimit = 10, maxLimit = 100): PaginationParams => {
  const page = Math.max(1, parseInt(req.query.page as string) || 1);
  const limit = Math.min(
    maxLimit,
    Math.max(1, parseInt(req.query.limit as string) || defaultLimit)
  );
  const offset = (page - 1) * limit;
  
  return { page, limit, offset };
};

// Verwendung
const { page, limit, offset } = getPagination(req);
```

### Offset-Validierung

```typescript
// Sicherstellen dass offset nicht negativ wird
const page = Math.max(1, parseInt(req.query.page as string) || 1);
const limit = Math.max(1, parseInt(req.query.limit as string) || 10);
const offset = (page - 1) * limit;

// Oder alternativ:
const offset = Math.max(0, (page - 1) * limit);
```

### Kompakte Meta-Daten

```typescript
// Nur wichtigste Felder
meta: {
  page,
  limit,
  total,
  totalPages: Math.ceil(total / limit)
}

// Oder erweitert mit Navigation:
meta: {
  page,
  limit,
  total,
  totalPages,
  nextPage: page < totalPages ? page + 1 : null,
  prevPage: page > 1 ? page - 1 : null,
  firstPage: 1,
  lastPage: totalPages
}
```

---

## Fehlerbehandlung

### Ungültige Parameter

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  let page = parseInt(req.query.page as string) || 1;
  let limit = parseInt(req.query.limit as string) || 10;
  
  // Validierung
  if (isNaN(page) || page < 1) {
    return res.status(400).json({ error: 'Page must be a positive number' });
  }
  
  if (isNaN(limit) || limit < 1 || limit > 100) {
    return res.status(400).json({ error: 'Limit must be between 1 and 100' });
  }
  
  const offset = (page - 1) * limit;
  // ... restlicher Code
});
```

### Page zu groß

```typescript
// Prüfen ob page > totalPages
if (page > totalPages && totalPages > 0) {
  return res.status(400).json({ 
    error: `Page ${page} does not exist. Total pages: ${totalPages}` 
  });
}
```

---

## Kombination mit Filtering

### Vollständiges Beispiel

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  // 1. Pagination
  const page = Math.max(1, parseInt(req.query.page as string) || 1);
  const limit = Math.min(100, Math.max(1, parseInt(req.query.limit as string) || 10));
  const offset = (page - 1) * limit;
  
  // 2. Filter
  const { status, priority, search } = req.query;
  const conditions: string[] = [];
  const params: any[] = [];
  
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
    const pattern = `%${search}%`;
    params.push(pattern, pattern);
  }
  
  const whereClause = conditions.length > 0 
    ? 'WHERE ' + conditions.join(' AND ')
    : '';
  
  // 3. Count (für Meta)
  const countSql = `SELECT COUNT(*) as count FROM tasks ${whereClause}`;
  
  db.get(countSql, params, (err, row: { count: number }) => {
    if (err) return res.status(500).json({ error: err.message });
    
    const total = row.count;
    const totalPages = Math.ceil(total / limit);
    
    // 4. Data Query
    const dataSql = `SELECT * FROM tasks ${whereClause} LIMIT ? OFFSET ?`;
    const dataParams = [...params, limit, offset];
    
    db.all(dataSql, dataParams, (err, tasks) => {
      if (err) return res.status(500).json({ error: err.message });
      
      res.json({
        data: tasks,
        meta: {
          page,
          limit,
          total,
          totalPages,
          hasNextPage: page < totalPages,
          hasPrevPage: page > 1
        }
      });
    });
  });
});
```

---

## Häufige Fehler vermeiden

1. **Page startet bei 1, nicht 0**
   ```typescript
   const offset = (page - 1) * limit;  // Bei page=1: offset=0
   ```

2. **Nie mit 0 multiplizieren**
   ```typescript
   const limit = Math.max(1, parseInt(req.query.limit as string) || 10);
   ```

3. **SQL Injection vermeiden**
   ```typescript
   // FALSCH: LIMIT ${limit}
   // RICHTIG: LIMIT ?
   params.push(limit);
   ```

4. **Offset zu groß**
   ```typescript
   // Prüfen ob offset > total
   if (offset >= total && total > 0) {
     return res.status(400).json({ error: 'Page out of range' });
   }
   ```

5. **Count und Data mit gleichen Filtern**
   - Beide Queries müssen identische WHERE-Bedingungen haben
   - Nur Pagination-Parameter unterscheiden sich
