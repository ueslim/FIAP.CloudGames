# Multi-stage build para otimizar o tamanho da imagem final
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Definir variáveis de ambiente para otimização
# Removido DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 daqui
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true \
    NUGET_XMLDOC_MODE=skip

# Definir diretório de trabalho
WORKDIR /src

# Copiar arquivos de projeto primeiro para aproveitar o cache do Docker
COPY ["FIAP.CloudGames.API/FIAP.CloudGames.API.csproj", "FIAP.CloudGames.API/"]
COPY ["FIAP.CloudGames.Application/FIAP.CloudGames.Application.csproj", "FIAP.CloudGames.Application/"]
COPY ["FIAP.CloudGames.Domain/FIAP.CloudGames.Domain.csproj", "FIAP.CloudGames.Domain/"]
COPY ["FIAP.CloudGames.Infra/FIAP.CloudGames.Infra.csproj", "FIAP.CloudGames.Infra/"]
COPY ["FIAP.CloudGames.sln", "./"]

# Restaurar dependências
RUN dotnet restore "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj"

# Copiar todo o código fonte
COPY . .

# Publicar a aplicação em modo Release
RUN dotnet publish "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --verbosity normal

# Stage de produção - imagem runtime otimizada
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Instalar dependências necessárias para Alpine Linux
RUN apk add --no-cache \
    icu-libs \
    && addgroup -g 1001 -S appgroup \
    && adduser -u 1001 -S appuser -G appgroup

# Definir variáveis de ambiente para produção
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:80 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_EnableDiagnostics=0

# Definir diretório de trabalho
WORKDIR /app

# Copiar apenas os arquivos publicados do stage de build
COPY --from=build /app/publish .

# Alterar propriedade dos arquivos para o usuário da aplicação
RUN chown -R appuser:appgroup /app

# Mudar para o usuário não-root
USER appuser

# Expor porta 80
EXPOSE 80

# Health check para verificar se a aplicação está funcionando
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

# Comando para executar a aplicação
ENTRYPOINT ["dotnet", "FIAP.CloudGames.API.dll"]