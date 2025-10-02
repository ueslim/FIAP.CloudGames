# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Fiap.CloudGames.Games/Fiap.CloudGames.Games.csproj Fiap.CloudGames.Games/
RUN dotnet restore Fiap.CloudGames.Games/Fiap.CloudGames.Games.csproj
COPY . .
RUN dotnet publish Fiap.CloudGames.Games/Fiap.CloudGames.Games.csproj -c Release -o /app/publish --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s CMD wget -qO- http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "Fiap.CloudGames.Games.dll"]
