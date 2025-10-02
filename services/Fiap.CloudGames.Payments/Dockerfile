# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Fiap.CloudGames.Payments/Fiap.CloudGames.Payments.csproj Fiap.CloudGames.Payments/
RUN dotnet restore Fiap.CloudGames.Payments/Fiap.CloudGames.Payments.csproj
COPY . .
RUN dotnet publish Fiap.CloudGames.Payments/Fiap.CloudGames.Payments.csproj -c Release -o /app/publish --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s CMD wget -qO- http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "Fiap.CloudGames.Payments.dll"]
