#!/bin/bash
set -e

echo "🚀 Setting up PHP Task API development environment..."

# Check PHP version
php --version

# Check PDO SQLite extension
php -m | grep -i pdo
php -m | grep -i sqlite

echo "✅ PHP development environment ready!"
echo ""
echo "To start the API:"
echo "  docker compose up -d"
echo ""
echo "To test the API:"
echo "  curl http://localhost:3003/api/tasks"
