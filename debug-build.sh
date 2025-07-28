#!/bin/bash

echo "🔍 Debug do Build - FIAP Cloud Games API"
echo "=========================================="

# Verificar se os arquivos existem
echo "📁 Verificando estrutura de arquivos..."
if [ -f "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj" ]; then
    echo "✅ FIAP.CloudGames.API.csproj encontrado"
else
    echo "❌ FIAP.CloudGames.API.csproj NÃO encontrado"
    exit 1
fi

if [ -f "FIAP.CloudGames.Application/FIAP.CloudGames.Application.csproj" ]; then
    echo "✅ FIAP.CloudGames.Application.csproj encontrado"
else
    echo "❌ FIAP.CloudGames.Application.csproj NÃO encontrado"
    exit 1
fi

if [ -f "FIAP.CloudGames.Domain/FIAP.CloudGames.Domain.csproj" ]; then
    echo "✅ FIAP.CloudGames.Domain.csproj encontrado"
else
    echo "❌ FIAP.CloudGames.Domain.csproj NÃO encontrado"
    exit 1
fi

if [ -f "FIAP.CloudGames.Infra/FIAP.CloudGames.Infra.csproj" ]; then
    echo "✅ FIAP.CloudGames.Infra.csproj encontrado"
else
    echo "❌ FIAP.CloudGames.Infra.csproj NÃO encontrado"
    exit 1
fi

if [ -f "FIAP.CloudGames.sln" ]; then
    echo "✅ FIAP.CloudGames.sln encontrado"
else
    echo "❌ FIAP.CloudGames.sln NÃO encontrado"
    exit 1
fi

echo ""
echo "🔧 Testando build local..."
echo "=========================="

# Testar restore
echo "📦 Testando dotnet restore..."
dotnet restore "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj"

if [ $? -eq 0 ]; then
    echo "✅ Restore bem-sucedido"
else
    echo "❌ Restore falhou"
    exit 1
fi

# Testar build
echo "🔨 Testando dotnet build..."
dotnet build "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj" -c Release

if [ $? -eq 0 ]; then
    echo "✅ Build bem-sucedido"
else
    echo "❌ Build falhou"
    exit 1
fi

# Testar publish
echo "📤 Testando dotnet publish..."
dotnet publish "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj" -c Release -o ./publish-test

if [ $? -eq 0 ]; then
    echo "✅ Publish bem-sucedido"
    echo "📁 Arquivos publicados em ./publish-test"
    ls -la ./publish-test
else
    echo "❌ Publish falhou"
    exit 1
fi

echo ""
echo "🐳 Testando build do Docker..."
echo "=============================="

# Limpar containers e imagens antigas
echo "🧹 Limpando containers e imagens antigas..."
docker-compose down --remove-orphans 2>/dev/null
docker image prune -f 2>/dev/null

# Build com logs detalhados
echo "🔨 Fazendo build do Docker com logs detalhados..."
docker build -t fiap-cloudgames-api-debug . --progress=plain --no-cache

if [ $? -eq 0 ]; then
    echo "✅ Build do Docker bem-sucedido!"
    
    # Testar execução
    echo "🚀 Testando execução do container..."
    docker run -d --name test-api-debug -p 8080:80 fiap-cloudgames-api-debug
    
    # Aguardar inicialização
    echo "⏳ Aguardando inicialização..."
    sleep 15
    
    # Testar health check
    echo "🏥 Testando health check..."
    if curl -f http://localhost:8080/health 2>/dev/null; then
        echo "✅ Health check funcionando!"
    else
        echo "❌ Health check falhou"
        echo "📋 Logs do container:"
        docker logs test-api-debug
    fi
    
    # Limpar
    docker stop test-api-debug 2>/dev/null
    docker rm test-api-debug 2>/dev/null
    
else
    echo "❌ Build do Docker falhou!"
    exit 1
fi

# Limpar arquivos de teste
echo "🧹 Limpando arquivos de teste..."
rm -rf ./publish-test

echo ""
echo "🎉 Debug concluído!" 