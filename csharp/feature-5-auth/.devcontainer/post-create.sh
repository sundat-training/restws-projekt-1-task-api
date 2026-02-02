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
echo "  - Open: http://localhost:3005"
echo "  - Test Login: curl -X POST http://localhost:3005/api/auth/login \\"
echo "      -H 'Content-Type: application/json' \\"
echo "      -d '{\"username\":\"alice\",\"password\":\"password123\"}'"
echo "=========================================="
