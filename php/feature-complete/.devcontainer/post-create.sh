#!/bin/bash
set -e

echo "🚀 Setting up PHP Task API (Feature Complete)..."

# Check PHP version
php --version

# Check PDO SQLite extension
php -m | grep -i pdo
php -m | grep -i sqlite

echo ""
echo "✅ PHP development environment ready!"
echo ""
echo "This is the COMPLETE implementation with all features:"
echo "  ✓ CRUD Operations"
echo "  ✓ Request Validation"
echo "  ✓ Query Filtering (status, priority, search)"
echo "  ✓ Pagination"
echo "  ✓ Authentication with Bearer Token"
echo ""
echo "To start the API:"
echo "  docker compose up -d"
echo ""
echo "To test the API:"
echo "  # Login"
echo "  curl -X POST http://localhost:3006/api/auth/login \\"
echo "    -H 'Content-Type: application/json' \\"
echo "    -d '{\"username\":\"alice\",\"password\":\"password123\"}'"
echo ""
echo "  # Get all tasks (with Bearer token)"
echo "  curl http://localhost:3006/api/tasks \\"
echo "    -H 'Authorization: Bearer user-1'"
echo ""
echo "Test Users:"
echo "  alice / password123"
echo "  bob / password456"
