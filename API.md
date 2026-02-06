# API Reference

This file summarizes the public HTTP endpoints and the gRPC contract used by the project. For implementation details see the controller and DTO files linked below.

**REST endpoints (PlatformService)**

- GET /api/platforms
  - Description: List all platforms.
  - Response: array of `PlatformReadDto`.
  - Implementation: [PlatformService/Controllers/PlatformsController.cs](PlatformService/Controllers/PlatformsController.cs)

- GET /api/platforms/{id}
  - Description: Get platform by id.
  - Response: `PlatformReadDto` or 404.
  - Implementation: [PlatformService/Controllers/PlatformsController.cs](PlatformService/Controllers/PlatformsController.cs)

- POST /api/platforms
  - Description: Create a new platform. Persists to DB, then synchronously calls the Commands service and publishes an async event to RabbitMQ.
  - Request body: `PlatformCreateDto` (fields: `Name`, `Publisher`, `Cost`). See [PlatformService/Dtos/PlatformCreateDto.cs](PlatformService/Dtos/PlatformCreateDto.cs)
  - Response: `PlatformReadDto` (fields: `Id`, `Name`, `Publisher`, `Cost`) — created resource with `Location` header. See [PlatformService/Dtos/PlatformReadDto.cs](PlatformService/Dtos/PlatformReadDto.cs)

**REST endpoints (CommandsService)**

- GET /api/c/platforms
  - Description: List platforms known to the Commands service (mirrored/received from events).
  - Response: array of `PlatformReadDto` (CommandsService variant: `Id`, `Name`).
  - Implementation: [CommandsService/Controllers/PlatformsController.cs](CommandsService/Controllers/PlatformsController.cs)

- GET /api/c/platforms/{platformId}/commands
  - Description: List commands for a platform.
  - Response: array of `CommandReadDto`.
  - Implementation: [CommandsService/Controllers/CommandsController.cs](CommandsService/Controllers/CommandsController.cs)

- GET /api/c/platforms/{platformId}/commands/{commandId}
  - Description: Get a single command for a platform.
  - Response: `CommandReadDto` or 404.
  - Implementation: [CommandsService/Controllers/CommandsController.cs](CommandsService/Controllers/CommandsController.cs)

- POST /api/c/platforms/{platformId}/commands
  - Description: Create a command for a platform (stored in Commands service DB).
  - Request body: `CommandCreateDto` (fields: `HowTo`, `CommandLine`). See [CommandsService/DTOs/CommandCreateDto.cs](CommandsService/DTOs/CommandCreateDto.cs)
  - Response: `CommandReadDto` (fields: `Id`, `HowTo`, `CommandLine`, `PlatformId`). See [CommandsService/DTOs/CommandReadDto.cs](CommandsService/DTOs/CommandReadDto.cs)

**Asynchronous events (RabbitMQ)**

- Event: `Platform_Published` (published by `PlatformService` after creation)
  - Payload type: `PlatformPublishedDto` (`Id`, `Name`, `Event`).
  - Publisher: `PlatformService/AsyncDataServices/MessageBusClient.cs`
  - Subscriber: `CommandsService/AsyncDataServices/MessageBusSubscriber.cs` (handler logic in `CommandsService/EventProcessing/` or `EventProcessor`).
  - DTOs: [PlatformService/Dtos/PlatformPublishedDto.cs](PlatformService/Dtos/PlatformPublishedDto.cs) and [CommandsService/DTOs/PlatformPublishedDto.cs](CommandsService/DTOs/PlatformPublishedDto.cs)

**gRPC contract**

The project exposes a simple gRPC service defined in `Protos/platforms.proto` used to fetch platform data.

- Service: `GrpcPlatform`
  - RPC: `GetAllPlatforms(GetAllRequest) returns (PlatformResponse)` — returns all platforms.
  - Message: `GrpcPlatformModel` fields: `platformId` (int32), `name` (string), `publisher` (string).
  - Proto file: [PlatformService/Protos/platforms.proto](PlatformService/Protos/platforms.proto)

Proto snippet (summary):

```proto
service GrpcPlatform {
  rpc GetAllPlatforms(GetAllRequest) returns (PlatformResponse);
}

message GrpcPlatformModel {
  int32 platformId = 1;
  string name = 2;
  string publisher = 3;
}

message PlatformResponse {
  repeated GrpcPlatformModel platform = 1;
}
```

**DTO quick reference**

- `PlatformCreateDto` (PlatformService)
  - `Name` (string, required)
  - `Publisher` (string, required)
  - `Cost` (string, required)
  - [PlatformService/Dtos/PlatformCreateDto.cs](PlatformService/Dtos/PlatformCreateDto.cs)

- `PlatformReadDto` (PlatformService)
  - `Id` (int)
  - `Name` (string)
  - `Publisher` (string)
  - `Cost` (string)
  - [PlatformService/Dtos/PlatformReadDto.cs](PlatformService/Dtos/PlatformReadDto.cs)

- `PlatformPublishedDto`
  - `Id` (int)
  - `Name` (string)
  - `Event` (string)
  - [PlatformService/Dtos/PlatformPublishedDto.cs](PlatformService/Dtos/PlatformPublishedDto.cs)

- `CommandCreateDto`
  - `HowTo` (string, required)
  - `CommandLine` (string, required)
  - [CommandsService/DTOs/CommandCreateDto.cs](CommandsService/DTOs/CommandCreateDto.cs)

- `CommandReadDto`
  - `Id` (int)
  - `HowTo` (string)
  - `CommandLine` (string)
  - `PlatformId` (int)
  - [CommandsService/DTOs/CommandReadDto.cs](CommandsService/DTOs/CommandReadDto.cs)

**Examples**

- Create platform (example):

```bash
curl -X POST http://localhost:5000/api/platforms \
  -H "Content-Type: application/json" \
  -d '{"name":"MyPlatform","publisher":"ACME","cost":"Free"}'
```

- Get commands for a platform:

```bash
curl http://localhost:5001/api/c/platforms/1/commands
```

**Where to inspect code**

- Platform controller: [PlatformService/Controllers/PlatformsController.cs](PlatformService/Controllers/PlatformsController.cs)
- Commands controller: [CommandsService/Controllers/CommandsController.cs](CommandsService/Controllers/CommandsController.cs)
- gRPC proto: [PlatformService/Protos/platforms.proto](PlatformService/Protos/platforms.proto)
- Messaging client/subscriber: `*/AsyncDataServices/MessageBusClient.cs` and `*/AsyncDataServices/MessageBusSubscriber.cs`

