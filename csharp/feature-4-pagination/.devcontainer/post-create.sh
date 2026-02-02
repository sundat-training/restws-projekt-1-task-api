#!/bin/bash
set -e

echo "=========================================="
echo "Post-Create Setup - C# .NET 8"
echo "=========================================="

echo "Restoring NuGet packages..."
dotnet restore

echo ""
echo ".NET SDK Version:"
dotnet --version

echo ""
echo "=========================================="
echo "Setup complete!"
echo "=========================================="
echo "You can now:"
echo "  - Run: docker compose up -d"
echo "  - Open: http://localhost:3004"
echo "  - Test API with: curl http://localhost:3004/api/tasks"
echo "  - Test pagination with: curl 'http://localhost:3004/api/tasks?page=1&limit=5'"
echo "=========================================="
