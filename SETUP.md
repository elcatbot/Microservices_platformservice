# Setup & Local Run

This guide explains how to run the project locally for development. It covers prerequisites, running services with `dotnet`, using `docker-compose` for local infrastructure, and applying EF Core migrations.

## Prerequisites

- .NET SDK (recommended: 9.0) — install the SDK matching the TargetFramework in the projects. The repo contains artifacts for `net5.0` and `net9.0`.
- Docker and Docker Compose (for local SQL and RabbitMQ)
- `kubectl` (optional, for Kubernetes testing)
- (Optional) `dotnet-ef` tool for migrations:

```bash
dotnet tool install --global dotnet-ef
```

## Environment and configuration

- Configuration is in `appsettings.json` / `appsettings.Development.json` per service.
- Example environment var to set the environment:

```bash
export ASPNETCORE_ENVIRONMENT=Development
```

- Connection strings and messaging settings are typically in `PlatformService/appsettings*.json` and `CommandsService/appsettings*.json`. If you run the provided `docker-compose.yml`, it will bring up local SQL and RabbitMQ and the default connection strings should work.

## Build and run locally (dotnet)

1. From repository root build the solution:

```bash
dotnet build Microservices_Platform.sln
```

2. Run `PlatformService` (default ports configured in launch settings or appsettings):

```bash
dotnet run --project PlatformService/PlatformService.csproj
```

3. In a separate terminal run `CommandsService`:

```bash
dotnet run --project CommandsService/CommandsService.csproj
```

Notes: If you need to override URLs/ports, pass `--urls` or configure in `appsettings.Development.json`.

## Run with Docker Compose (recommended for full local stack)

The repo includes `docker-compose.yml` that will start both services plus dependent infra (RabbitMQ, SQL). From the repo root:

```bash
docker-compose up --build
```

To run in background:

```bash
docker-compose up -d --build
```

To stop and remove containers:

```bash
docker-compose down
```

## Database migrations / seeding

Platform migrations live in `PlatformService/Migrations/`. To apply migrations to the configured database (using the `PlatformService` project as the EF target):

```bash
dotnet ef database update --project PlatformService/PlatformService.csproj
```

If you need to add a migration locally:

```bash
dotnet ef migrations add <Name> --project PlatformService/PlatformService.csproj
```

The projects include a `PrepDb` helper that seeds example data during startup in Development. Check `PlatformService/Data/PrepDb.cs` and `CommandsService/Data/PrepDb.cs`.

Adjust image names and repository tags in manifests before deploying to a remote cluster.

## Ports & endpoints (typical local defaults)

- PlatformService: http://localhost:5000 (API: `/api/platforms`)
- CommandsService: http://localhost:6000 (API: `/api/c/platforms/...`)
- RabbitMQ management (if enabled by docker-compose): http://localhost:15672

If ports differ in your environment, check `Properties/launchSettings.json` in each service or the `docker-compose.yml` for mapped ports.

## Troubleshooting tips

- If EF migrations fail, confirm the connection string and that the SQL container is accessible.
- Check logs: for Docker Compose use `docker-compose logs -f`, for `dotnet run` the console output contains useful diagnostics.
- If RabbitMQ messages are not delivered, inspect queue/exchange bindings in the RabbitMQ UI and ensure the services use the same routing keys.

## Quick commands summary

```bash
# build
dotnet build Microservices_Platform.sln

# run services locally
dotnet run --project PlatformService/PlatformService.csproj
dotnet run --project CommandsService/CommandsService.csproj

# run stack with docker-compose
docker-compose up --build

# apply migrations
dotnet ef database update --project PlatformService/PlatformService.csproj
```

