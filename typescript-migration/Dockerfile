# Use the official Node.js runtime as the base image
FROM node:18-alpine

# Set the working directory inside the container
WORKDIR /app

# Copy package.json and package-lock.json (if available)
COPY package*.json ./

# Install dependencies
RUN npm ci --only=production

# Copy the source code
COPY . .

# Build the TypeScript project
RUN npm run build

# Expose the port (if needed for HTTP mode)
EXPOSE 3000

# Set environment variables
ENV NODE_ENV=production
ENV ENDPOINT=http://localhost:4000/graphql
ENV HEADERS={}
ENV ALLOW_MUTATIONS=false
ENV NAME=mcp-graphql

# Create a non-root user
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nodejs -u 1001

# Change ownership of the app directory to the nodejs user
RUN chown -R nodejs:nodejs /app
USER nodejs

# Command to run the application
CMD ["node", "dist/index.js"]
