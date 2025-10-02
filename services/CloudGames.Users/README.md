# Fiap.CloudGames.Users

Authentication and user management microservice. Issues JWT tokens for other services to validate.

## Run locally

- `dotnet run --project Fiap.CloudGames.Users/Fiap.CloudGames.Users.csproj`
- Environment variables: `ConnectionStrings__UsersDb`, `ConnectionStrings__Storage`, `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`
- Swagger: `/swagger`

## Endpoints

- POST `/api/users/register`
- POST `/api/users/login`
- GET `/api/users/me` (requires Bearer)

## OpenAPI export

- GET `http://localhost:port/swagger/v1/swagger.json` and save to `openapi.users.json`

## Azure (Students) deployment

- App Service Free or Container Apps
- Configure env vars (see above) and Application Insights connection string
- Use the sample GitHub Actions workflow to build and publish
