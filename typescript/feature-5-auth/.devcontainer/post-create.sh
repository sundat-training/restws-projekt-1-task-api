#!/bin/bash
set -e

echo "=========================================="
echo "Post-Create Setup"
echo "=========================================="

echo "Updating npm to latest version..."
npm install -g npm@latest

echo "Node version: $(node -v)"
echo "npm version: $(npm -v)"

echo ""
echo "=========================================="
echo "Installing dependencies..."
echo "=========================================="

# Create node_modules directory with correct permissions
mkdir -p node_modules

# Install dependencies
npm install

echo ""
echo "=========================================="
echo "Setup complete!"
echo "=========================================="
echo "You can now:"
echo "  - Run: docker compose up -d"
echo "  - Open: http://localhost:3005"
echo "  - Test API with: curl http://localhost:3005/api/tasks"
echo "=========================================="
