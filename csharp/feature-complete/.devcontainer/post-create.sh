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
echo "Feature Complete API is ready!"
echo ""
echo "You can now:"
echo "  - Run: docker compose up -d"
echo "  - Open: http://localhost:3006"
echo ""
echo "Test endpoints:"
echo "  - Register: curl -X POST http://localhost:3006/api/auth/register \\"
echo "      -H 'Content-Type: application/json' \\"
echo "      -d '{\"username\":\"test\",\"password\":\"password123\"}'"
echo ""
echo "  - Login: curl -X POST http://localhost:3006/api/auth/login \\"
echo "      -H 'Content-Type: application/json' \\"
echo "      -d '{\"username\":\"alice\",\"password\":\"password123\"}'"
echo ""
echo "  - Get Tasks: curl http://localhost:3006/api/tasks \\"
echo "      -H 'Authorization: Bearer user-1'"
echo "=========================================="
