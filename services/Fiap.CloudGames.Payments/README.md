# Fiap.CloudGames.Payments

Payment initiation microservice. Publishes events to Storage Queues for async confirmation.

## Run locally

- `dotnet run --project Fiap.CloudGames.Payments/Fiap.CloudGames.Payments.csproj`
- Env vars: `ConnectionStrings__PaymentsDb`, `ConnectionStrings__Storage`, `Queues__Payments`, `Jwt__*`
- Swagger: `/swagger`

## Endpoints

- POST `/api/payments` (initiate)
- GET `/api/payments/{id}` (status)

## OpenAPI export

- GET `http://localhost:port/swagger/v1/swagger.json` → `openapi.payments.json`

## Azure setup

- Azure Storage Queues: `payments-events`
- Deploy to App Service Free or Container Apps. Configure env vars and App Insights.
