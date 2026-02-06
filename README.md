
# Microservices Platform

Lightweight example microservices platform demonstrating two .NET services that share data synchronously (gRPC/HTTP) and asynchronously (RabbitMQ events).

**Tech stack:** .NET (C#), EF Core, gRPC, RabbitMQ, Docker, Kubernetes

**Status:** First-draft documentation — see `ARCHITECTURE.md`, `API.md`, and `DEPLOYMENT.md` TODOs.

**Quickstart (local)**

1. Prerequisites: install .NET SDK (matching project), Docker (optional), and kubectl (optional).

2. Build and run locally (two services):

```bash
dotnet build Microservices_Platform.sln
dotnet run --project PlatformService/PlatformService.csproj
dotnet run --project CommandsService/CommandsService.csproj
```

3. Run with Docker Compose (recommended for local integration with RabbitMQ and SQL):

```bash
docker-compose up --build
```

**Repository layout**

- `PlatformService/` - service that owns platform data, EF Core migrations, gRPC proto, controllers, and message publisher.
- `CommandsService/` - service receiving platform events, exposing commands API, and subscribing to RabbitMQ events.
- `K8S/` - Kubernetes manifests (DB, RabbitMQ, deployments, services, secrets).
- `docker-compose.yml` - local multi-container setup.
- `Microservices_Platform.sln` - solution file for both services.

**What each service contains**

- `Program.cs` / `appsettings*.json` - startup and configuration.
- `Controllers/` - REST controllers for each service.
- `Data/` - EF Core `AppDbContext`, repos, and `PrepDb` seeding helper.
- `Protos/platforms.proto` - gRPC contract shared between services.
- `AsyncDataServices/` - RabbitMQ publisher/subscriber implementations.

**Architecture (summary)**

- Clients call `PlatformService` to create/read platforms.
- `PlatformService` persists data (EF Core) and publishes `PlatformPublished` events to RabbitMQ.
- `CommandsService` subscribes to platform events and stores/acts on platform metadata as needed.
- gRPC is used for synchronous platform data exchange where implemented; Protobuf contract lives in `Protos/`.

**Important commands**

Run EF Core migrations (service project context):

```bash
dotnet ef migrations add <Name> --project PlatformService/PlatformService.csproj
dotnet ef database update --project PlatformService/PlatformService.csproj
```

Docker build & compose (root):

```bash
docker-compose build
docker-compose up
```

Kubernetes apply (order matters: secrets → infra → services):

```bash
kubectl apply -f K8S/mssql-secret.yaml
kubectl apply -f K8S/mssql-plat-depl.yaml
kubectl apply -f K8S/rabbitmq-depl.yaml
kubectl apply -f K8S/platforms-depl.yaml
kubectl apply -f K8S/commands-depl.yaml
```
