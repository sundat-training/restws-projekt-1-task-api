FROM php:8.2-cli

WORKDIR /workspaces/feature-2-validation

# Install system dependencies for SQLite and curl
RUN apt-get update && apt-get install -y \
    libsqlite3-dev \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install PDO SQLite extension
RUN docker-php-ext-install pdo pdo_sqlite

# Copy project files
COPY composer.json ./
COPY index.php ./

# Expose port
EXPOSE 3000

# Start PHP built-in server
CMD ["php", "-S", "0.0.0.0:3000", "index.php"]
