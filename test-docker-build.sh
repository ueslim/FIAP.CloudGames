#!/bin/bash

echo "🐳 Testando build do Docker..."

# Limpar containers e imagens antigas
echo "🧹 Limpando containers e imagens antigas..."
docker-compose down --remove-orphans
docker image prune -f

# Build da imagem
echo "🔨 Fazendo build da imagem..."
docker build -t fiap-cloudgames-api .

# Verificar se o build foi bem-sucedido
if [ $? -eq 0 ]; then
    echo "✅ Build realizado com sucesso!"
    
    # Executar container de teste
    echo "🚀 Executando container de teste..."
    docker run -d --name test-api -p 8080:80 fiap-cloudgames-api
    
    # Aguardar inicialização
    echo "⏳ Aguardando inicialização da aplicação..."
    sleep 10
    
    # Testar health check
    echo "🏥 Testando health check..."
    curl -f http://localhost:8080/health
    
    if [ $? -eq 0 ]; then
        echo "✅ Health check funcionando!"
    else
        echo "❌ Health check falhou!"
    fi
    
    # Parar e remover container de teste
    echo "🛑 Parando container de teste..."
    docker stop test-api
    docker rm test-api
    
else
    echo "❌ Build falhou!"
    exit 1
fi

echo "🎉 Teste concluído!" 