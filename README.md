# graphql-mcp

This repository contains a sample Model Context Protocol (MCP) server with a collection of tools for interacting with GraphQL schemas.

## Building and Running

The project targets .NET 9. Use the standard `dotnet` CLI to run the server:

```bash
dotnet run --project graphql-mcp.csproj
```

You can also build a Docker image using the provided Dockerfile:

```bash
docker build -t graphql-mcp .
docker run --rm -it graphql-mcp
```

## Usage

The server exposes several tools, such as the query analyzer and schema utilities. Environment variables allow customizing the endpoint and schema path:

- `ENDPOINT` – URL of the GraphQL endpoint
- `HEADERS` – JSON object of HTTP headers
- `ALLOW_MUTATIONS` – `true` to enable mutations
- `SCHEMA` – path to a local schema file

## Development

Tools live in the `Tools` directory and are automatically registered at startup. Contributions are welcome!
