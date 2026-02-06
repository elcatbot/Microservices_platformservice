# Deployment Guide

This document describes how to build, run and deploy the Microservices Platform using Docker (local) and Kubernetes (cluster). It assumes you have the repo checked out and have read `SETUP.md` for prerequisites.

Contents:
- Docker (local): build images, docker-compose local stack, env overrides
- Docker registries: tag and push images
- Kubernetes (K8S/): apply manifests, secrets, ordering, tips
- Configuration: env vars and connection strings
- Health checks, scaling and rolling updates
- Troubleshooting

## Docker (local)

1. Build the solution binaries:

```bash
dotnet build Microservices_Platform.sln
```

2. Build container images (example tags):

```bash
docker build -t myrepo/platformservice:dev -f PlatformService/Dockerfile PlatformService
docker build -t myrepo/commandsservice:dev -f CommandsService/Dockerfile CommandsService
```

3. Use the provided `docker-compose.yml` to start the full stack (RabbitMQ, SQL, services):

```bash
docker-compose up --build
```

4. For local development overrides use `docker-compose.override.yml` and a `.env` file (example `.env.example` provided). This mounts source and exposes developer ports.

Notes:
- `docker-compose` service names must match values referenced in connection strings and RabbitMQ settings (see `docker-compose.yml`).
- If you change exposed ports or URLs, update `appsettings.Development.json` or pass environment variables in `docker-compose.override.yml`.

## Docker registries (push images)

1. Tag and push built images to your registry (Docker Hub, ACR, ECR):

```bash
docker tag myrepo/platformservice:dev <registry>/platformservice:latest
docker push <registry>/platformservice:latest
```

2. Update Kubernetes manifests or Helm values to reference the pushed image tags.

## Kubernetes (K8S)

Files: the repo contains manifests under `K8S/`. Review each manifest before applying.

1. Prepare secrets (example): create or edit `K8S/mssql-secret.yaml` with production-safe credentials (do not commit real secrets).

2. Apply manifests in recommended order (sequential, because some resources depend on infra):

```bash
kubectl apply -f K8S/mssql-secret.yaml
kubectl apply -f K8S/mssql-plat-depl.yaml
kubectl apply -f K8S/rabbitmq-depl.yaml
kubectl apply -f K8S/platforms-depl.yaml
kubectl apply -f K8S/commands-depl.yaml
kubectl apply -f K8S/ingress-srv.yaml    # if ingress manifests exist
```

3. Verify resources:

```bash
kubectl get pods,svc,deploy,statefulset -n <namespace>
kubectl logs deploy/platforms-deployment
kubectl describe pod <pod-name>
```

4. Expose services via `Service` objects and optionally an `Ingress` resource. Configure DNS / hosts accordingly (see `K8S/hosts`).

Notes about manifests:
- Adjust image names and tags in manifests to point at your registry.
- Ensure `env` entries and `ConnectionStrings` environment variables match your cluster secrets.
- The SQL manifest may use a PVC; ensure your cluster has suitable storage class.

## Configuration & secrets

- Local: use `.env` and `docker-compose.override.yml` for overrides.
- Kubernetes: store secrets in `K8S/mssql-secret.yaml` or use an external secret store (recommended for production).
- Connection string example used in `docker-compose.yml`:

```
Server=platform-sqlserver;Initial Catalog=platformsdb;User ID=sa;Password=YourPassword123;TrustServerCertificate=True;
```

Replace with secure values in prod.

## Health checks and probes

- Add `livenessProbe` and `readinessProbe` to pod specs (HTTP or gRPC endpoints) for production deployments.
- Example (HTTP readiness):

```yaml
readinessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 10
```

Ensure your services expose a health endpoint (create `/health` if missing).

## Scaling and rolling updates

- Make sure DB migrations are compatible with rolling updates.
- Use `kubectl rollout` to update deployments safely:

```bash
kubectl set image deployment/platforms-deployment platform=myregistry/platformservice:v2
kubectl rollout status deployment/platforms-deployment
```

For large changes, consider a blue/green or canary deployment strategy.

## Troubleshooting

- Pod not starting: `kubectl describe pod <pod>` and `kubectl logs <pod>`.
- DB connection failures: validate secret, connection string, and network policy; try `kubectl exec` into a debug pod and run `telnet platform-sqlserver 1433`.
- RabbitMQ issues: check management UI (port 15672) and confirm exchange/queue bindings.

## CI/CD recommendations

- CI pipeline should:
  - Build and test (`dotnet build`, `dotnet test`)
  - Build container images and push to registry
  - Update manifests (image tags) and apply to cluster (or create PR for CD to apply)

- Protect secrets in CI using the platform's secret storage (GitHub Actions secrets, GitLab CI variables, etc.).

## Useful commands

```bash
# Build and run locally
docker-compose up --build

# Build images
docker build -t myrepo/platformservice:dev -f PlatformService/Dockerfile PlatformService

# Push image
docker push <registry>/platformservice:dev

# Apply K8S manifests
kubectl apply -f K8S/mssql-secret.yaml
kubectl apply -f K8S/platforms-depl.yaml

# Check rollout
kubectl rollout status deployment/platforms-deployment

