FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /workspaces/feature-complete

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy project file first (for layer caching)
COPY feature-complete.csproj ./
RUN dotnet restore

# Copy source code
COPY . .

EXPOSE 3000

CMD ["dotnet", "run", "--urls", "http://0.0.0.0:3000"]
