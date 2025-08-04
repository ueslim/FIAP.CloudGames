# FIAP Cloud Games API

## Visão Geral do Projeto

Este projeto consiste na primeira fase do desenvolvimento da **FIAP Cloud Games (FCG)**, uma plataforma de venda de jogos digitais e gestão de servidores para partidas online. Nesta fase inicial, o foco principal é a criação de um serviço robusto de cadastro de usuários e a gestão da biblioteca de jogos adquiridos, servindo como base essencial para as futuras funcionalidades da plataforma.

A API foi desenvolvida em .NET 8, utilizando o padrão **Controllers MVC**, e segue os princípios de boas práticas de desenvolvimento, garantindo persistência de dados e qualidade de software, preparando o terreno para futuras funcionalidades como matchmaking e gerenciamento de servidores.

## Funcionalidades Implementadas

### Cadastro de Usuários
* Identificação do cliente por nome, e-mail e senha.
* Validação rigorosa do formato de e-mail (não duplicado) e de senha segura (mínimo de 8 caracteres, com números, letras e caracteres especiais).

### Autenticação e Autorização (JWT)
* Sistema de autenticação baseado em JSON Web Tokens (JWT).
* Dois níveis de acesso:
    * **Usuário** (`role: 0`): Acesso à plataforma e à sua biblioteca de jogos.
    * **Administrador** (`role: 1`): Permite cadastrar novos jogos, administrar usuários e criar promoções.

### Gerenciamento de Jogos
* CRUD (Create, Read, Update, Delete) de jogos.
* Os endpoints de `POST`, `PUT` e `DELETE` para jogos são acessíveis apenas por usuários com perfil de `Administrador`.
* Os endpoints de `GET` para jogos são públicos (não requerem autenticação).

## Tecnologias Utilizadas

* **Linguagem:** C#
* **Framework:** .NET 8 (com Controllers MVC)
* **Banco de Dados:** SQL Server (via Entity Framework Core)
    * **Ambiente de Desenvolvimento:** SQL Server Express LocalDB (`(localdb)\\mssqllocaldb`)
* **ORM:** Entity Framework Core
* **Autenticação:** JWT (JSON Web Tokens)
* **Documentação da API:** Swagger/OpenAPI
* **Testes:** xUnit (para testes unitários)
* **Padrões de Projeto:** DDD (Domain-Driven Design)

## Pré-requisitos

Para rodar este projeto localmente, você precisará ter instalado:

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* **SQL Server Express LocalDB**: Geralmente instalado junto com o Visual Studio ou como um componente do SQL Server Express.
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (recomendado) ou [Visual Studio Code](https://code.visualstudio.com/) com as extensões C#.
* Uma ferramenta para testar APIs, como [Postman](https://www.postman.com/downloads/) ou [Insomnia](https://insomnia.rest/download), ou utilizar a interface Swagger gerada.

## Configuração do Ambiente de Desenvolvimento

Siga os passos abaixo para configurar e rodar a API em sua máquina local:

### 1. Clonar o Repositório

```bash
git clone https://github.com/ueslim/FIAP.CloudGames
cd FIAP.CloudGames # Navegue para a pasta raiz do projeto
```
## 2. Configuração do Banco de Dados Local

A API está configurada para utilizar o SQL Server Express LocalDB, que é ideal para desenvolvimento local, pois não requer configurações complexas ou dependência do nome da sua máquina.

A Connection String padrão no `appSettings.json` é:

```json
"DefaultConnection": "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=FIAPCloudGames;Integrated Security=True;Trust Server Certificate=True"
```

### Verificar/Instalar o LocalDB:

- **Com Visual Studio:**  
  Abra o Visual Studio Installer, clique em "Modificar" na sua instalação do VS e certifique-se de que a opção **"SQL Server Express LocalDB"** esteja marcada em "Componentes Individuais".

- **Via Terminal:**  
  Abra o Prompt de Comando ou PowerShell e digite:
  ```bash
  sqllocaldb info
  ```
  Você deverá ver uma instância chamada `mssqllocaldb`. Se ela estiver "Stopped", inicie-a com:
  ```bash
  sqllocaldb start mssqllocaldb
  ```

### Aplicar Migrations (Criação do Banco de Dados):

A API utiliza Entity Framework Core Migrations para gerenciar o esquema do banco de dados. O banco de dados `FIAPCloudGames` e suas tabelas são criados automaticamente (se não existirem) na sua instância `(localdb)\\mssqllocaldb` quando você executa as Migrations.

Navegue até a pasta do projeto principal no terminal (onde está o `.csproj` da API) e execute o seguinte comando:

```bash
dotnet ef database update
```

Este comando aplicará todas as migrations pendentes, criando o banco de dados e as tabelas necessárias.

---

# Rodar a Aplicação

Navegue até a pasta raiz do projeto da API no terminal (onde está o arquivo `.csproj` principal) e execute:

```bash
dotnet run
```

Ou, abra a solução no Visual Studio e execute-a (pressionando F5).

A API estará acessível em `https://localhost:<PORTA>` (a porta será exibida no console ao iniciar a aplicação).

---

# Acessar a Documentação da API (Swagger UI)

Após iniciar a aplicação, abra seu navegador e acesse a interface Swagger UI em:

```
https://localhost:<PORTA>/swagger
```

Você poderá visualizar todos os endpoints, testá-los diretamente no navegador e ver exemplos de requisições e respostas.

---

# Endpoints Principais da API

## Níveis de Acesso (Roles)

Ao criar ou visualizar usuários, o campo `role` da API utiliza os seguintes valores para representar os níveis de acesso:

- **User = 0:** Usuário comum (acesso à plataforma e biblioteca de jogos).
- **Administrator = 1:** Usuário com privilégios de administrador (pode gerenciar usuários, jogos e promoções).

---

## Autenticação

### POST `/api/Auth/login`

Realiza o login de um usuário (com e-mail e senha) e retorna um token JWT para acesso autenticado.

**Requisição (LoginDto):**

```json
{
  "email": "seu_email@dominio.com",
  "password": "sua_senha_segura"
}
```

**Resposta (Sucesso):**

```json
{
  "token": "seu_token_jwt_aqui",
  "expiration": "data_expiracao_utc"
}
```

---

### POST `/api/Auth/register`

Registra um novo usuário no sistema. Não requer autenticação.

**Requisição (CreateUserDto):**

```json
{
  "name": "Nome do Usuario",
  "email": "email@dominio.com",
  "password": "SenhaSegura123!",
  "role": 0 
}
```

Para criar um Administrador, use `role: 1`.

**Resposta (Sucesso):** Retorna o objeto do usuário criado (geralmente sem a senha).

---

## Usuários (Requer token de Administrador para a maioria das operações)

- **POST** `/api/usuarios`: Cria um novo usuário.  
  Exemplo de DTO de Requisição (CreateUserDto):

  ```json
  {
    "name": "Novo Usuario",
    "email": "novo.usuario@example.com",
    "password": "MinhaSenhaSegura123!",
    "role": 0 
  }
  ```

- **GET** `/api/usuarios`: Lista todos os usuários cadastrados.
- **GET** `/api/usuarios/{id}`: Obtém detalhes de um usuário específico por ID.
- **PUT** `/api/usuarios/{id}`: Atualiza as informações de um usuário existente.
- **DELETE** `/api/usuarios/{id}`: Remove um usuário do sistema.

---

## Jogos (GETs públicos; POST/PUT/DELETE requerem Administrador)

- **POST** `/api/jogos`: Adiciona um novo jogo à biblioteca.  
  Exemplo de DTO de Requisição (CreateGameDto):

  ```json
  {
    "title": "Aventura do Zé",
    "description": "Um jogo emocionante de aventura e mistério.",
    "developer": "Estúdio X",
    "publisher": "Editora Y",
    "genre": "Aventura",
    "releaseDate": "2023-10-26T00:00:00Z",
    "price": 59.99,
    "coverImageUrl": "https://example.com/cover.jpg",
    "tags": ["Ação", "RPG"]
  }
  ```

- **GET** `/api/jogos`: Lista todos os jogos disponíveis na plataforma.
- **GET** `/api/jogos/{id}`: Obtém detalhes de um jogo específico por ID.
- **PUT** `/api/jogos/{id}`: Atualiza as informações de um jogo existente.
- **DELETE** `/api/jogos/{id}`: Remove um jogo da biblioteca.

---

# Tratamento de Erros, Logs e Validações

- **Tratamento de Erros:**  
  A API utiliza um `ErrorHandlingMiddleware` global que captura exceções não tratadas e retorna respostas padronizadas em JSON com status HTTP apropriados (400, 401, 404, 500), garantindo uma experiência consistente ao consumidor da API.

- **Logs:**  
  O sistema de logs padrão do ASP.NET Core (`Microsoft.Extensions.Logging`) é utilizado pelo middleware de erro para registrar exceções e eventos importantes.

- **Validações Principais:**  
  - **Nome:** Obrigatório, mínimo de 3 caracteres, máximo de 100 caracteres.  
  - **Email:** Obrigatório, formato válido de e-mail, e não pode ser duplicado no sistema.  
  - **Senha:** Obrigatória, mínimo de 8 caracteres, e deve conter pelo menos uma letra, um número e um caractere especial.

- **Mensagens de Erro:** As mensagens de validação são claras e descritivas para cada regra violada.

---

# Estrutura do Projeto

A estrutura do projeto (`FIAP.CloudGames.API`) é organizada para seguir princípios de responsabilidade e facilitar a manutenção:

- **Controllers:** Contém os controladores da API, responsáveis por receber as requisições HTTP, validar dados de entrada e acionar os serviços da aplicação.
- **Middlewares:** Armazena middlewares customizados, como o de tratamento global de erros (`ErrorHandlingMiddleware`).
- **Configurations:** Centraliza configurações da aplicação, como setup de Swagger, banco de dados, autenticação, e injeção de dependências.
- **DTOs:** (Conforme sua estrutura) Contém Data Transfer Objects para requisição e resposta.
- **Interfaces:** (Conforme sua estrutura) Define contratos para serviços e repositórios.
- **Raiz do projeto:** Contém arquivos principais como `Program.cs` e o arquivo de projeto `.csproj`, que configuram o pipeline da aplicação e suas dependências.

(Considerar que outras camadas como Services, Data, Repositories, Models/Domain podem estar em projetos separados dentro da solução, conforme a arquitetura DDD.)

---

# Testes Unitários

O projeto inclui um conjunto de testes unitários para as funcionalidades críticas, garantindo a validação das regras de negócio e o comportamento esperado da API:

- **Autenticação (`AuthTest.cs`):**  
   - Teste para verificar autenticação bem-sucedida com credenciais válidas.
   - Teste para verificar falha na autenticação quando o e-mail ou a senha são inexistentes.
   - Teste para verificar criação de usuário com dados válidos.
   - Teste para verificar exibição de mensagem ao tentar criar usuário com e-mail já existente.
   - Teste para validar que o nome do usuário possui no mínimo 3 caracteres e não é nulo.
   - Teste para validar que o nome do usuário não excede 100 caracteres.
   - Teste para validar que o e-mail fornecido está em um formato válido.
   - Teste para validar que a senha possui no mínimo 8 caracteres, incluindo letras, números e caracteres especiais.

- **Usuário (`UserTest.cs`):**  
   - Teste para verificar a consulta de todos os usuários cadastrados.
   - Teste para verificar a consulta de um único usuário existente.
   - Teste para verificar o retorno apropriado ao consultar um usuário inexistente.
   - Teste para verificar a consulta da biblioteca de jogos associada ao usuário.
   - Teste para validar atualização de usuário existente ou inativo.
   - Teste para verificar falha na atualização quando o novo e-mail informado já está em uso.
   - Teste para verificar atualização de usuário com sucesso.
   - Teste para verificar remoção de usuário com sucesso.
   - Teste para verificar falha na remoção de usuário em caso de erro.

- **Jogos (`GameTest.cs`):**
   - Teste para verificar a consulta de um game específico existente.
   - Teste para verificar a consulta de todos os games disponíveis.
   - Teste para verificar o retorno apropriado ao consultar um game inexistente.
   - Teste para verificar atualização de game com sucesso por um administrador.
   - Teste para verificar falha na tentativa de atualização de game por um usuário não autorizado.
   - Teste para verificar remoção de game com sucesso por um administrador.
   - Teste para verificar falha na tentativa de remoção de game por um usuário não autorizado.
   - Teste para verificar criação de game com sucesso por um administrador.
   - Teste para verificar falha na tentativa de criação de game por um usuário não autorizado.
   - Teste para validar falha na criação de game com campos inválidos.

---

# Documentação DDD

A modelagem do domínio do projeto foi realizada utilizando Event Storming para mapear os fluxos de usuários e jogos. Os princípios de DDD foram seguidos na organização das entidades e regras de negócio.

A documentação do Domain-Driven Design (DDD) pode ser encontrada em:

[https://miro.com/app/board/uXjVIyOtuxQ=/](https://miro.com/app/board/uXjVIyOtuxQ=/)

---

# Relatório de Entrega

Para a entrega final, por favor, inclua um relatório conforme as instruções do Tech Challenge, contendo as seguintes informações:

- Nome do Grupo: Grupo 83  
- Link da Documentação DDD: [https://miro.com/app/board/uXjVIyOtuxQ=/](https://miro.com/app/board/uXjVIyOtuxQ=/)  
- Link do Repositório: [https://github.com/ueslim/FIAP.CloudGames](https://github.com/ueslim/FIAP.CloudGames)  
- Link do Vídeo: [https://youtu.be/sIG9bVz7vcc](https://youtu.be/sIG9bVz7vcc)

---

# Autores

A seguir, a lista de participantes do projeto, com seus respectivos usernames no Discord (conforme solicitado no relatório de entrega):

- Denis Elvis Santos da Silva - Discord: dennisvr  
- Fernanda Carrijo Ravaglia Nunes - Discord: nanquinh  
- Vinicyus Anchieta Alves De Oliveira - Discord: vinicyusoliveira  
- Wesley Maciel Melo - Discord: uelism
