# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CloudGames.Games/CloudGames.Games.csproj CloudGames.Games/
RUN dotnet restore CloudGames.Games/CloudGames.Games.csproj
COPY . .
RUN dotnet publish CloudGames.Games/CloudGames.Games.csproj -c Release -o /app/publish --no-restore

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s CMD wget -qO- http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "CloudGames.Games.dll"]
