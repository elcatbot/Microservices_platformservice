# Services & Code Layout

This file explains the responsibilities and key code locations for each service, and the repository layout so contributors can quickly find code to work on.

## Overview

- Services:
  - `PlatformService` — owns platform data, exposes REST/gRPC, persists via EF Core, and publishes platform events to RabbitMQ.
  - `CommandsService` — subscribes to platform events, stores command records, and exposes REST endpoints for commands.

## Top-level layout

- `Microservices_Platform.sln` — solution containing both services.
- `docker-compose.yml` — local stack (RabbitMQ, SQL, both services).
- `K8S/` — Kubernetes manifests (secrets, DB, RabbitMQ, deployments).

Each service folder contains a similar structure:

- `Program.cs` / `GlobalUsings.cs` — application entry and DI setup.
- `appsettings.json`, `appsettings.Development.json` — config per environment.
- `Controllers/` — REST controllers (API surface).
- `Data/` — EF Core `AppDbContext`, repos, and `PrepDb` seeders.
- `DTOs` or `Dtos` — request/response/event DTOs.
- `AsyncDataServices/` — RabbitMQ publisher/subscriber implementations.
- `SyncDataServices/` or `Protos/` — gRPC clients/protos or HTTP sync clients.
- `Profiles/` — AutoMapper profiles.

## PlatformService (key responsibilities & files)

- Purpose: manage platform entities; publish events when platforms are created.
- Key files:
  - Controller: `PlatformService/Controllers/PlatformsController.cs`
  - Data: `PlatformService/Data/AppDbContext.cs`, `PlatformService/Data/IPlatformRepo.cs`, `PlatformService/Data/PlatformRepo.cs`
  - DTOs: `PlatformService/Dtos/PlatformCreateDto.cs`, `PlatformService/Dtos/PlatformReadDto.cs`, `PlatformService/Dtos/PlatformPublishedDto.cs`
  - Messaging: `PlatformService/AsyncDataServices/MessageBusClient.cs` (publishes `PlatformPublishedDto`)
  - gRPC proto: `PlatformService/Protos/platforms.proto`
  - Migrations: `PlatformService/Migrations/`

## CommandsService (key responsibilities & files)

- Purpose: receive platform events, maintain commands for platforms, and provide commands API.
- Key files:
  - Controllers: `CommandsService/Controllers/CommandsController.cs`, `CommandsService/Controllers/PlatformsController.cs`
  - Data: `CommandsService/Data/ICommandRepo.cs`, `CommandsService/Data/CommandRepo.cs`, `CommandsService/Data/AppDbContext.cs`
  - DTOs: `CommandsService/DTOs/CommandCreateDto.cs`, `CommandsService/DTOs/CommandReadDto.cs`, `CommandsService/DTOs/PlatformPublishedDto.cs`
  - Messaging: `CommandsService/AsyncDataServices/MessageBusSubscriber.cs` (listens for `PlatformPublished` events)

## Common patterns & conventions

- Dependency injection: services registered in `Program.cs`.
- DTOs + AutoMapper: mapping profiles in `Profiles/` folder.
- Repository pattern: interfaces `I*Repo` and implementations in `Data/`.
- Async messaging: publisher sends `PlatformPublishedDto` (PlatformService) and subscriber consumes it (CommandsService).
- Sync integration: `ICommandDataClient` on `PlatformService` calls `CommandsService` synchronously when creating platforms.

## How to add a new API endpoint

1. Add route in the appropriate controller under `Controllers/`.
2. Add DTOs under `DTOs/` and mapping in `Profiles/`.
3. If data persisted, add or update repository in `Data/` and update `AppDbContext` if needed.
4. Add unit tests (if present) and run `dotnet build`.

## How to extend messaging

1. Add new event DTO under the producing service's `DTOs/`.
2. Publish from code (e.g., after a save) using the existing `MessageBusClient` pattern.
3. Add a `MessageBusSubscriber` handler in the subscribing service to handle event deserialization and processing.

## Where to run tests and build

- Build solution: `dotnet build Microservices_Platform.sln`.
- Run a service: `dotnet run --project PlatformService/PlatformService.csproj` or `CommandsService/CommandsService.csproj`.

## Useful file pointers

- Solution: `Microservices_Platform.sln`
- Platform controller: `PlatformService/Controllers/PlatformsController.cs`
- Commands controller: `CommandsService/Controllers/CommandsController.cs`
- Protos: `*/Protos/platforms.proto`
- Messaging clients/subscribers: `*/AsyncDataServices/MessageBusClient.cs`, `*/AsyncDataServices/MessageBusSubscriber.cs`
- DB seed helpers: `*/Data/PrepDb.cs`


