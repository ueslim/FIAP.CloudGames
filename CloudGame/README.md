# CloudGame Aggregator

This solution aggregates the CloudGames microservices for local development.

## Docker Compose (Azurite and services)

- Start Azurite only:
```bash
cd CloudGame
docker compose up -d azurite
```

- Start Azurite + all services (uncomment service blocks in docker-compose.yml first if desired):
```bash
docker compose up -d
```

- Rebuild images:
```bash
docker compose up --build -d
```

Notes:
- Services are configured for Development and expect `UseDevelopmentStorage=true` for Storage Queues.
- Visual Studio can still run each API individually. You can run Azurite via Compose alongside VS.
