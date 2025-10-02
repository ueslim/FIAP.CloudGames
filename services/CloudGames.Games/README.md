# Fiap.CloudGames.Games

Games catalog microservice with Azure Cognitive Search integration.

## Run locally

- `dotnet run --project Fiap.CloudGames.Games/Fiap.CloudGames.Games.csproj`
- Env vars: `ConnectionStrings__GamesDb`, `ConnectionStrings__Storage`, `Jwt__*`, `Search__Endpoint`, `Search__ApiKey`, `Search__IndexName`
- Swagger: `/swagger`

## Endpoints

- GET `/api/games` (list)
- GET `/api/games/{id}` (details)
- GET `/api/games/search?q=...` (search via ACS)
- POST `/api/games` (admin)
- PUT `/api/games/{id}` (admin)
- DELETE `/api/games/{id}` (admin)

## OpenAPI export

- GET `http://localhost:port/swagger/v1/swagger.json` and save to `openapi.games.json`

## Azure setup

- Azure Cognitive Search (Free): create index `games`
- Azure Storage Queues: `games-events`
- Deploy to App Service Free or Container Apps. Configure env vars and App Insights.
