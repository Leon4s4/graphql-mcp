# GraphQL MCP Server - Python Implementation
FROM python:3.11-slim

LABEL maintainer="GraphQL MCP Team"
LABEL description="GraphQL MCP Server Python Implementation"

# Set working directory
WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    gcc \
    g++ \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements first for better layer caching
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy source code
COPY src/ ./src/
COPY pyproject.toml setup.py ./

# Install the package
RUN pip install -e .

# Create non-root user
RUN useradd --create-home --shell /bin/bash graphql-mcp
USER graphql-mcp

# Set environment variables
ENV PYTHONPATH=/app/src
ENV PYTHONUNBUFFERED=1

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD python -c "import graphql_mcp; print('OK')" || exit 1

# Default command
CMD ["python", "-m", "graphql_mcp.main"]