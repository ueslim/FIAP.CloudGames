# CloudGames Microservices (Phase 3)

This folder contains three independent .NET 8 microservices and two Azure Functions, designed with Hexagonal Architecture (Ports & Adapters):

- Fiap.CloudGames.Users (JWT issuer, user management)
- Fiap.CloudGames.Games (catalog + Azure Cognitive Search)
- Fiap.CloudGames.Payments (payment initiation + events)
- Functions:
  - GamesIndexUpdater (updates Cognitive Search index)
  - PaymentsConfirmationProcessor (updates payment status)

Each service is independently buildable, deployable, and has its own Dockerfile and appsettings. Use Azure for Students free tiers: App Service Free/Container Apps, Cognitive Search Free, Storage Queues, Functions Free, and Application Insights.

## Local quickstart

- Users: `dotnet run --project Fiap.CloudGames.Users/Fiap.CloudGames.Users.csproj`
- Games: `dotnet run --project Fiap.CloudGames.Games/Fiap.CloudGames.Games.csproj`
- Payments: `dotnet run --project Fiap.CloudGames.Payments/Fiap.CloudGames.Payments.csproj`

Configure per-service `appsettings.json` or environment variables. Default CORS allows http://localhost:4200.

## Azure deployment (Students)

- Create Azure resources: App Service (F1) or Container Apps, Storage Account (Queues), Cognitive Search (Free), Application Insights, optionally API Management (Developer SKU for tests).
- Configure environment variables for JWT, connection strings, Storage, and Search.
- Deploy with the provided GitHub Actions workflows in each service folder.
