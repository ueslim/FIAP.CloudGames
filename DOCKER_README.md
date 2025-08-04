# Docker Setup - FIAP Cloud Games API

Este documento contém instruções para executar a aplicação FIAP Cloud Games API usando Docker.

## 🏗️ Estrutura do Projeto

```
FIAP.CloudGames/
├── FIAP.CloudGames.API/           # 🚀 Camada de Apresentação
├── FIAP.CloudGames.Application/   # ⚙️ Camada de Aplicação  
├── FIAP.CloudGames.Domain/        # 🏛️ Camada de Domínio
├── FIAP.CloudGames.Infra/         # 🗄️ Camada de Infraestrutura
├── FIAP.CloudGames.Tests/         # 🧪 Testes Unitários
├── Dockerfile                     # 🐳 Dockerfile multi-stage
├── .dockerignore                  # 📝 Arquivos ignorados no build
└── docker-compose.yml             # 🐙 Orquestração de containers
```

## 🚀 Executando a Aplicação

### **Opção 1: Build e execução completa**
```bash
# Build e start de todos os serviços
docker-compose up --build -d

# Verificar status dos containers
docker-compose ps

# Ver logs da aplicação
docker-compose logs -f fiap-cloudgames-api
```

### **Opção 2: Apenas a aplicação**
```bash
# Build da imagem
docker build -t fiap-cloudgames-api .

# Executar container
docker run -d -p 8080:80 --name fiap-cloudgames-api fiap-cloudgames-api
```

## 🌐 Acessando a Aplicação

- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger

## 🔧 Configurações

### **Portas Utilizadas**
- **8080** - Aplicação API (mapeada para 80 interno)
- **1433** - SQL Server
- **6379** - Redis

### **Variáveis de Ambiente**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
DOTNET_RUNNING_IN_CONTAINER=true
```

## 🛠️ Comandos Úteis

### **Gerenciamento de Containers**
```bash
# Parar todos os serviços
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Rebuild de um serviço específico
docker-compose build fiap-cloudgames-api

# Executar comandos dentro do container
docker-compose exec fiap-cloudgames-api dotnet --version
```

### **Logs e Debugging**
```bash
# Ver logs de todos os serviços
docker-compose logs

# Ver logs de um serviço específico
docker-compose logs fiap-cloudgames-api

# Ver logs em tempo real
docker-compose logs -f fiap-cloudgames-api

# Acessar shell do container
docker-compose exec fiap-cloudgames-api sh
```

### **Banco de Dados**
```bash
# Conectar ao SQL Server
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P FiapCloudGames2024!

# Executar migrações (dentro do container da API)
docker-compose exec fiap-cloudgames-api dotnet ef database update
```

## 🔒 Otimizações de Segurança

### **Dockerfile Multi-Stage**
- **Stage 1 (build)**: Compilação e publicação da aplicação
- **Stage 2 (runtime)**: Imagem final otimizada com Alpine Linux

## 🔍 Troubleshooting

### **Problemas Comuns**

1. **Porta já em uso**
   ```bash
   # Verificar portas em uso
   netstat -an | grep 8080
   
   # Alterar porta no docker-compose.yml
   ports:
     - "8081:80"  # Mudar de 8080 para 8081
   ```

2. **Erro de conexão com banco**
   ```bash
   # Verificar se o SQL Server está rodando
   docker-compose ps sqlserver
   
   # Verificar logs do SQL Server
   docker-compose logs sqlserver
   ```

3. **Problemas de permissão**
   ```bash
   # Rebuild da imagem
   docker-compose build --no-cache fiap-cloudgames-api
   ```

## 🧹 Limpeza

```bash
# Remover containers, redes e volumes
docker-compose down -v

# Remover imagens não utilizadas
docker image prune -f

# Limpeza completa do Docker
docker system prune -a
```

## 📊 Tamanho da Imagem

A imagem final otimizada tem aproximadamente:
- **~150-200MB** (com Alpine Linux)
- **~50-70% menor** que uma imagem padrão
- **Inicialização rápida** com ReadyToRun
