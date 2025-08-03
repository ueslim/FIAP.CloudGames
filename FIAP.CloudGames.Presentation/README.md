# FIAP Cloud Games - Frontend

Frontend React com TypeScript e Vite para o projeto FIAP Cloud Games.

## Pré-requisitos

- Node.js (versão 16 ou superior)
- npm ou yarn

## Instalação

1. Instale as dependências:
```bash
npm install
```

2. Configure as variáveis de ambiente:
   - Copie o arquivo `env.example` para `.env`
   - Ajuste a URL da API conforme necessário:
   ```
   VITE_API_URL=http://localhost:5000
   ```

## Executando o projeto

### Desenvolvimento
```bash
npm run dev
```

O projeto estará disponível em `http://localhost:3000`

### Build para produção
```bash
npm run build
```

### Preview da build
```bash
npm run preview
```

## Estrutura do projeto

```
src/
├── components/          # Componentes React
│   ├── Login.tsx       # Componente de login
│   └── Register.tsx    # Componente de registro
├── services/           # Serviços de API
│   └── api.ts         # Configuração do axios e serviços de autenticação
├── styles/            # Estilos CSS
│   └── global.css     # Estilos globais
├── types/             # Definições de tipos TypeScript
│   └── auth.ts        # Tipos para autenticação
├── App.tsx            # Componente principal
└── main.tsx           # Ponto de entrada da aplicação
```

## Funcionalidades

- **Login**: Formulário de login com validação e integração com a API
- **Registro**: Formulário de registro com validação e confirmação de senha
- **Autenticação**: Gerenciamento de token JWT e estado de autenticação
- **Interface moderna**: Design responsivo com feedback visual
- **Validação**: Validação de formulários com mensagens de erro
- **Loading states**: Indicadores de carregamento durante requisições

## Endpoints utilizados

- `POST /api/Auth/login` - Login do usuário
- `POST /api/Auth/register` - Registro de novo usuário

## Tecnologias utilizadas

- React 18
- TypeScript
- Vite
- Axios
- CSS3 (sem frameworks)

## Scripts disponíveis

- `npm run dev` - Inicia o servidor de desenvolvimento
- `npm run build` - Gera a build de produção
- `npm run preview` - Visualiza a build de produção
- `npm run lint` - Executa o linter 