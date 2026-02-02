FROM node:23-slim

WORKDIR /workspaces/feature-4-pagination

# Install curl for health checks and any other tools
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Install dependencies
COPY package*.json ./
RUN npm install

# Copy source code
COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev"]
