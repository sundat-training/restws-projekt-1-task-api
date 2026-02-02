# HINTS.md - Authentication Implementation Hints

## Reading the Authorization Header

In PHP, you can access the Authorization header like this:

```php
$authHeader = $_SERVER['HTTP_AUTHORIZATION'] ?? '';
```

## Parsing Bearer Token

Extract the user ID from a Bearer token:

```php
if (strpos($authHeader, 'Bearer ') === 0) {
    $userId = substr($authHeader, 7);
} else {
    http_response_code(401);
    echo json_encode(['error' => 'Authentication required']);
    exit;
}
```

## Login Implementation

Here's a complete login implementation:

```php
$input = json_decode(file_get_contents('php://input'), true);

$stmt = $db->prepare("SELECT * FROM users WHERE username = ?");
$stmt->execute([$input['username']]);
$user = $stmt->fetch();

if ($user && $user['password'] === $input['password']) {
    http_response_code(200);
    echo json_encode(['userId' => $user['id'], 'username' => $user['username']]);
} else {
    http_response_code(401);
    echo json_encode(['error' => 'Invalid credentials']);
}
```

## Auth Middleware Pattern

Create a reusable function to check authentication:

```php
function requireAuth($db) {
    $authHeader = $_SERVER['HTTP_AUTHORIZATION'] ?? '';
    
    if (empty($authHeader)) {
        http_response_code(401);
        echo json_encode(['error' => 'Authentication required']);
        return null;
    }
    
    if (strpos($authHeader, 'Bearer ') !== 0) {
        http_response_code(401);
        echo json_encode(['error' => 'Invalid authorization format']);
        return null;
    }
    
    $userId = substr($authHeader, 7);
    
    $stmt = $db->prepare("SELECT * FROM users WHERE id = ?");
    $stmt->execute([$userId]);
    $user = $stmt->fetch();
    
    if (!$user) {
        http_response_code(401);
        echo json_encode(['error' => 'Invalid token']);
        return null;
    }
    
    return $userId;
}
```

Use it in your routes:

```php
// Protected route example
$userId = requireAuth($db);
if ($userId === null) {
    exit; // Response already sent
}

// Now use $userId for further operations
```

## User Isolation in SQL

Filter tasks to only show the authenticated user's tasks:

```php
$userId = requireAuth($db);
if ($userId === null) {
    exit;
}

$stmt = $db->prepare("SELECT * FROM tasks WHERE userId = ?");
$stmt->execute([$userId]);
$tasks = $stmt->fetchAll();
```

## Ownership Check

Verify that a task belongs to the current user before modifying or deleting:

```php
$taskId = $_GET['id'] ?? $_POST['id'] ?? null;
$userId = requireAuth($db);
if ($userId === null) {
    exit;
}

$stmt = $db->prepare("SELECT * FROM tasks WHERE id = ? AND userId = ?");
$stmt->execute([$taskId, $userId]);
$task = $stmt->fetch();

if (!$task) {
    http_response_code(403);
    echo json_encode(['error' => 'Forbidden - Not your task']);
    exit;
}

// Proceed with update/delete
```

## Complete Example: Protected PUT Endpoint

```php
if ($_SERVER['REQUEST_METHOD'] === 'PUT') {
    $input = json_decode(file_get_contents('php://input'), true);
    $taskId = $_GET['id'] ?? null;
    
    $userId = requireAuth($db);
    if ($userId === null) {
        exit;
    }
    
    // Verify ownership first
    $stmt = $db->prepare("SELECT * FROM tasks WHERE id = ? AND userId = ?");
    $stmt->execute([$taskId, $userId]);
    $task = $stmt->fetch();
    
    if (!$task) {
        http_response_code(403);
        echo json_encode(['error' => 'Not authorized to modify this task']);
        exit;
    }
    
    // Perform update
    $stmt = $db->prepare("UPDATE tasks SET title = ?, status = ? WHERE id = ?");
    $stmt->execute([$input['title'], $input['status'], $taskId]);
    
    http_response_code(200);
    echo json_encode(['message' => 'Task updated']);
}
```

## Common Mistakes

### Mistake 1: Forgetting to set userId on POST

When creating a new task, you must set the `userId` from the authenticated user, not from the request body:

```php
// WRONG - trusting user input
$userId = $input['userId'];

// CORRECT - using authenticated user's ID
$userId = $authenticatedUserId;
```

### Mistake 2: Not checking ownership before DELETE

Always verify the task belongs to the user before deletion:

```php
// WRONG - deletes any task
$stmt = $db->prepare("DELETE FROM tasks WHERE id = ?");
$stmt->execute([$taskId]);

// CORRECT - only deletes own task
$stmt = $db->prepare("DELETE FROM tasks WHERE id = ? AND userId = ?");
$stmt->execute([$taskId, $userId]);
```

### Mistake 3: Not handling missing Authorization header

The header might not be present at all:

```php
// WRONG - assuming header exists
$token = substr($_SERVER['HTTP_AUTHORIZATION'], 7);

// CORRECT - checking first
if (!isset($_SERVER['HTTP_AUTHORIZATION'])) {
    http_response_code(401);
    echo json_encode(['error' => 'Authorization header missing']);
    exit;
}
```

### Mistake 4: Using the wrong comparison for Bearer prefix

Use `strpos()` with strict comparison:

```php
// WRONG
if (strpos($authHeader, 'Bearer ') == 0) { // Works but not explicit

// CORRECT
if (strpos($authHeader, 'Bearer ') === 0) { // Strict comparison
```

### Mistake 5: Not returning early after sending error response

Once you send an error response, exit to prevent further processing:

```php
http_response_code(401);
echo json_encode(['error' => 'Unauthorized']);
exit; // Important!
```

## File Structure

Your `src/index.php` should have this structure:

1. Database connection setup
2. Helper function `requireAuth()`
3. Routes:
   - POST /api/auth/login
   - GET /api/tasks (with auth, filtered by userId)
   - POST /api/tasks (with auth, sets userId)
   - GET /api/tasks/:id (with auth, checks ownership)
   - PUT /api/tasks/:id (with auth, checks ownership)
   - DELETE /api/tasks/:id (with auth, checks ownership)

## Testing Your Implementation

Test with alice (user-1):

```bash
# Should return 401
curl http://localhost:3005/api/tasks

# Should work and return alice's tasks
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-1"

# Should return 403 (task-4 belongs to bob)
curl http://localhost:3005/api/tasks/task-4 \
  -H "Authorization: Bearer user-1"
```

Test with bob (user-2):

```bash
# Should work and return bob's tasks
curl http://localhost:3005/api/tasks \
  -H "Authorization: Bearer user-2"

# Should work (task-4 belongs to bob)
curl http://localhost:3005/api/tasks/task-4 \
  -H "Authorization: Bearer user-2"

# Should return 403 (task-1 belongs to alice)
curl http://localhost:3005/api/tasks/task-1 \
  -H "Authorization: Bearer user-2"
```
