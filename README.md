# Microservices Platform

Lightweight example microservices platform demonstrating two .NET services that share data synchronously (gRPC/HTTP) and asynchronously (RabbitMQ events).

**Tech stack:** .NET (C#), EF Core, gRPC, RabbitMQ, Docker, Kubernetes

**Status:** First-draft documentation — see `ARCHITECTURE.md`, `API.md`, and `DEPLOYMENT.md` TODOs.

**Quickstart (local)**

- Prerequisites: install .NET SDK (matching project), Docker (optional).
- [SETUP.md](SETUP.md)

**Deployment (Docker & Kubernetes)**
- Prerequisites: install .NET SDK (matching project), Docker (optional), and kubectl (optional).
- [DEPLOYMENT.md](DEPLOYMENT.md)

**ARCHITECTURE**

- [ARCHITECTURE.md](ARCHITECTURE.md)

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

