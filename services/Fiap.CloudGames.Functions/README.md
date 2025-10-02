# Fiap.CloudGames.Functions

Azure Functions (isolated) for async processing.

- `GamesIndexUpdater`: Queue trigger `games-events` → updates Azure Cognitive Search index
- `PaymentsConfirmationProcessor`: Queue trigger `payments-events` → marks payment succeeded and appends event log

## Run locally

- `func start` (after `dotnet build`)
- Set `AzureWebJobsStorage`, `Search__Endpoint`, `Search__ApiKey`, `Search__IndexName`, `PaymentsDb`

## Deploy

- Azure Functions Consumption (Free). Use Azure Functions extension or GitHub Actions.
