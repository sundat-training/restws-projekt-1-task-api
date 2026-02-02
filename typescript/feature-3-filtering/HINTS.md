# Lösungshinweise - Feature 3: Filtering

Diese Datei enthält Hinweise und Code-Beispiele für die Implementierung der Query-Filter in `src/index.ts`.

---

## Aufgabe 1: Filter nach Status implementieren

### Lösungsansatz

1. Extrahiere Query-Parameter mit `req.query`
2. Baue SQL-Query dynamisch auf
3. Füge WHERE-Bedingungen hinzu wenn Parameter vorhanden
4. Verwende Prepared Statements (?) gegen SQL Injection

### Query-Parameter extrahieren

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status } = req.query;  // req.query.status
  
  // status ist string | undefined | string[]
  // Bei SQLite brauchen wir string
  const statusFilter = status as string | undefined;
});
```

### Lösung: Dynamische SQL-Query

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status } = req.query;
  
  let query = 'SELECT * FROM tasks';
  const params: any[] = [];
  
  if (status) {
    query += ' WHERE status = ?';
    params.push(status);
  }
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});
```

### Lösung: Mehrere Status-Werte erlauben

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status } = req.query;
  
  let query = 'SELECT * FROM tasks';
  const params: any[] = [];
  const conditions: string[] = [];
  
  if (status) {
    conditions.push('status = ?');
    params.push(status);
  }
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});
```

---

## Aufgabe 2: Filter nach Priority implementieren

### Lösungsansatz

1. Füge `priority` Parameter hinzu
2. Kombiniere mit `status` Filter (AND)
3. Beide Filter sollen gleichzeitig funktionieren

### Lösung: Status UND Priority

```typescript
app.get('/api/tasks', (req: Request, res: Response) => {
  const { status, priority } = req.query;
  
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
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});
```

### Beispiele für kombinierte Filter

```bash
# Nur Status
GET /api/tasks?status=pending

# Nur Priority
GET /api/tasks?priority=high

# Beides
GET /api/tasks?status=pending&priority=high

# Kein Filter (alle Tasks)
GET /api/tasks
```

---

## Aufgabe 3: Suche implementieren

### Lösungsansatz

1. Füge `search` Parameter hinzu
2. Verwende SQL `LIKE` mit Wildcards (%)
3. Suche in `title` UND `description`
4. SQLite ist case-insensitive per default

### Lösung: Suche mit LIKE

```typescript
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
    // Suche in title ODER description
    conditions.push('(title LIKE ? OR description LIKE ?)');
    const searchPattern = `%${search}%`;
    params.push(searchPattern, searchPattern);
  }
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  db.all(query, params, (err, tasks) => {
    if (err) return res.status(500).json({ error: 'Failed to fetch tasks' });
    res.json(tasks);
  });
});
```

### Wichtige Hinweise zur Suche

- `%` ist Wildcard für beliebige Zeichen
- `%search%` findet "search" überall im Text
- `search%` findet Wörter die mit "search" beginnen
- `%search` findet Wörter die mit "search" enden
- SQLite LIKE ist case-insensitive für ASCII

### Case-Sensitive Suche (wenn nötig)

```typescript
// Mit COLLATE für exakte Case-Sensitive-Suche
conditions.push('(title LIKE ? COLLATE BINARY OR description LIKE ? COLLATE BINARY)');
```

---

## Hilfreiche SQL Patterns

### Dynamische WHERE-Bedingungen

```typescript
// Pattern für beliebig viele Filter
const buildQuery = (filters: Record<string, any>) => {
  let query = 'SELECT * FROM tasks';
  const params: any[] = [];
  const conditions: string[] = [];
  
  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      conditions.push(`${key} = ?`);
      params.push(value);
    }
  });
  
  if (conditions.length > 0) {
    query += ' WHERE ' + conditions.join(' AND ');
  }
  
  return { query, params };
};

// Verwendung
const { query, params } = buildQuery({ status, priority });
db.all(query, params, callback);
```

### Suche in mehreren Spalten

```typescript
// Suche in 3 Spalten
if (search) {
  conditions.push('(title LIKE ? OR description LIKE ? OR id LIKE ?)');
  const pattern = `%${search}%`;
  params.push(pattern, pattern, pattern);
}
```

### Kombination Filter + Suche

```typescript
// Beispiel: ?status=pending&search=API&priority=high
// WHERE status = 'pending' AND priority = 'high' AND (title LIKE '%API%' OR description LIKE '%API%')
```

---

## TypeScript Typisierung

### Korrekte Typen für Query-Parameter

```typescript
import { Request, Response } from 'express';

app.get('/api/tasks', (req: Request, res: Response) => {
  // req.query ist ParsedQs | string | string[] | undefined
  const status = req.query.status as string | undefined;
  const priority = req.query.priority as string | undefined;
  const search = req.query.search as string | undefined;
  
  // Oder mit destructuring
  const { 
    status, 
    priority, 
    search 
  } = req.query as { 
    status?: string; 
    priority?: string; 
    search?: string;
  };
});
```

### Validierung der Query-Parameter

```typescript
// Optional: Prüfe ob Parameter gültig sind
const validStatuses = ['pending', 'in_progress', 'completed'];
const validPriorities = ['low', 'medium', 'high'];

if (status && !validStatuses.includes(status as string)) {
  return res.status(400).json({ error: 'Invalid status' });
}
```

---

## Häufige Fehler vermeiden

1. **SQL Injection:** Immer `?` Platzhalter verwenden, niemals Strings konkatenieren!
   ```typescript
   // FALSCH:
   query += `WHERE status = '${status}'`;  // ❌ SQL Injection!
   
   // RICHTIG:
   query += 'WHERE status = ?';  // ✅ Parameterized Query
   params.push(status);
   ```

2. **Undefined Parameter:** Prüfe ob Parameter vorhanden ist
   ```typescript
   if (status) {  // Nur wenn status !== undefined
     conditions.push('status = ?');
   }
   ```

3. **Leere WHERE:** Nur WHERE hinzufügen wenn Bedingungen vorhanden
   ```typescript
   if (conditions.length > 0) {
     query += ' WHERE ' + conditions.join(' AND ');
   }
   ```

4. **Case-Sensitivity:** SQLite LIKE ist meist case-insensitive, aber nicht garantiert

5. **URL Encoding:** Bei Leerzeichen in Suchbegriffen URL-encode verwenden
   ```
   search=TypeScript%20Basics  // TypeScript Basics
   ```
