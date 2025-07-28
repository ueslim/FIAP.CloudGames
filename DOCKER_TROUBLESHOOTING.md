# Docker Troubleshooting - FIAP Cloud Games API

Este documento contém soluções para problemas comuns encontrados durante o build e execução do Docker.

## 🚨 Problema: "not found" nos arquivos de projeto

### **Erro:**
```
failed to solve: failed to compute cache key: failed to calculate checksum of ref 21189e67-71d8-4aef-97f6-ad3688423916::iwvcau4r8tnyucxiemyujkso0: "/src/FIAP.CloudGames.Infra/FIAP.CloudGames.Infra.csproj": not found
```

### **Causa:**
O Dockerfile estava configurado para uma estrutura com pasta `src/`, mas os projetos estão diretamente na raiz.

### **Solução:**
✅ **CORRIGIDO** - O Dockerfile foi atualizado para refletir a estrutura real:
- Projetos estão em: `FIAP.CloudGames.API/`, `FIAP.CloudGames.Application/`, etc.
- **NÃO** em: `src/FIAP.CloudGames.API/`, `src/FIAP.CloudGames.Application/`, etc.

## 🚨 Problema: Falha no dotnet publish

### **Erro:**
```
failed to solve: process "/bin/sh -c dotnet publish \"FIAP.CloudGames.API/FIAP.CloudGames.API.csproj\"     -c Release     -o /app/publish     --no-restore     --verbosity quiet     -p:PublishTrimmed=true     -p:PublishSingleFile=false     -p:PublishReadyToRun=true     -p:EnableCompressionInSingleFile=true" did not complete successfully: exit code: 1
```

### **Causa:**
As otimizações de publicação (PublishTrimmed, PublishReadyToRun) podem não ser compatíveis com todas as dependências do projeto.

### **Solução:**
✅ **CORRIGIDO** - Dockerfile simplificado removendo otimizações problemáticas:
- Removido `PublishTrimmed=true`
- Removido `PublishReadyToRun=true`
- Removido `EnableCompressionInSingleFile=true`
- Adicionado build step separado para melhor debug

## 🔧 Comandos para Testar

### **1. Build Simples**
```bash
docker build -t fiap-cloudgames-api .
```

### **2. Build com Docker Compose**
```bash
docker-compose up --build -d
```

### **3. Verificar Estrutura**
```bash
# Verificar se os arquivos existem
ls -la FIAP.CloudGames.API/FIAP.CloudGames.API.csproj
ls -la FIAP.CloudGames.Application/FIAP.CloudGames.Application.csproj
ls -la FIAP.CloudGames.Domain/FIAP.CloudGames.Domain.csproj
ls -la FIAP.CloudGames.Infra/FIAP.CloudGames.Infra.csproj
```

### **4. Teste Completo**
```bash
# Executar script de teste (se disponível)
chmod +x test-docker-build.sh
./test-docker-build.sh
```

### **5. Debug Detalhado**
```bash
# Executar script de debug
chmod +x debug-build.sh
./debug-build.sh
```

### **6. Build com Logs Detalhados**
```bash
# Build com logs completos
docker build -t fiap-cloudgames-api . --progress=plain --no-cache
```

### **7. Teste Local Primeiro**
```bash
# Testar build local antes do Docker
dotnet restore "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj"
dotnet build "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj" -c Release
dotnet publish "FIAP.CloudGames.API/FIAP.CloudGames.API.csproj" -c Release -o ./publish-test
```

## 🐛 Outros Problemas Comuns

### **Problema: Porta já em uso**
```bash
# Verificar portas em uso
netstat -an | grep 8080

# Parar containers existentes
docker-compose down
docker stop $(docker ps -q)
```

### **Problema: Erro de permissão**
```bash
# Rebuild sem cache
docker-compose build --no-cache fiap-cloudgames-api

# Ou build individual
docker build --no-cache -t fiap-cloudgames-api .
```

### **Problema: Erro de conexão com banco**
```bash
# Verificar se o SQL Server está rodando
docker-compose ps sqlserver

# Ver logs do SQL Server
docker-compose logs sqlserver
```

### **Problema: Health check falha**
```bash
# Verificar se o endpoint está funcionando
curl http://localhost:8080/health

# Ver logs da aplicação
docker-compose logs fiap-cloudgames-api
```

## 📋 Checklist de Verificação

### **Antes do Build:**
- [ ] Todos os arquivos `.csproj` existem na estrutura correta
- [ ] O arquivo `FIAP.CloudGames.sln` está na raiz
- [ ] O `.dockerignore` não está excluindo arquivos necessários
- [ ] Docker Desktop está rodando

### **Durante o Build:**
- [ ] O build não apresenta erros de "not found"
- [ ] As dependências são restauradas corretamente
- [ ] A publicação é realizada com sucesso

### **Após o Build:**
- [ ] A imagem é criada com sucesso
- [ ] O container inicia sem erros
- [ ] O health check responde corretamente
- [ ] A aplicação está acessível na porta configurada

## 🔍 Debugging Avançado

### **Verificar Contexto do Build**
```bash
# Ver o que está sendo enviado para o Docker
docker build --progress=plain -t fiap-cloudgames-api .
```

### **Verificar Conteúdo da Imagem**
```bash
# Entrar na imagem para debug
docker run -it fiap-cloudgames-api sh

# Listar arquivos
ls -la /app
```

### **Verificar Logs Detalhados**
```bash
# Logs da aplicação
docker-compose logs -f fiap-cloudgames-api

# Logs do SQL Server
docker-compose logs -f sqlserver
```

## 📞 Suporte

Se o problema persistir:

1. **Verifique a estrutura de arquivos** - confirme que os projetos estão na raiz
2. **Limpe o cache do Docker** - `docker system prune -a`
3. **Rebuild sem cache** - `docker build --no-cache -t fiap-cloudgames-api .`
4. **Verifique os logs** - use `docker-compose logs` para debug

## ✅ Status Atual

- **Dockerfile**: ✅ Corrigido para estrutura real
- **Health Check**: ✅ Habilitado no Program.cs
- **Documentação**: ✅ Atualizada
- **Script de Teste**: ✅ Criado 