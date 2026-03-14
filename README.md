# CSINV — Sistema de Inventário Cacau Show

Sistema web para gerenciamento de inventário de produtos da Cacau Show. Permite criar sessões de inventário **mensal ou anual**, registrar produtos com quantidades, filtrar e consultar o histórico de entradas. Sistema com autenticação JWT e rastreamento de usuários.

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | C# .NET 10.0 · ASP.NET Core Web API |
| Autenticação | JWT (Bearer) · BCrypt |
| Banco de dados | MySQL / MariaDB |
| Frontend | React 19 · TypeScript · Vite |

---

## Estrutura do projeto

```
csinv/
├── backend/
│   ├── .env                          # Variáveis de ambiente (banco + URLs + JWT)
│   └── Modules/
│       ├── Database/
│       │   └── DatabaseConnection.cs # Factory de conexão MySQL
│       ├── Users/
│       │   ├── Interfaces/           # IUserRepository, IUserService
│       │   ├── UserController.cs
│       │   ├── UserService.cs
│       │   ├── UserRepository.cs
│       │   └── UsersDTO.cs
│       ├── Inventory/
│       │   ├── Interfaces/           # IInventoryProductsRepository, IInventoryProductsService
│       │   ├── InventoryProductsController.cs
│       │   ├── InventoryProductsService.cs
│       │   ├── InventoryProductsRepository.cs
│       │   └── InventoryProductsDTO.cs
│       └── InventorySessions/
│           ├── Interfaces/           # ISessionRepository, ISessionService
│           ├── SessionController.cs
│           ├── SessionService.cs
│           ├── SessionRepository.cs
│           └── SessionDTO.cs
├── frontend/
│   └── src/
│       ├── context/
│       │   └── AuthContext.tsx        # Context de autenticação (JWT + localStorage)
│       ├── services/
│       │   └── api.ts                # Fetch wrapper com token JWT e redirect 401
│       ├── modules/
│       │   ├── Auth/
│       │   │   └── Login.tsx         # Página de login
│       │   ├── Products/
│       │   │   └── Models/ProductModel.tsx
│       │   └── Sessions/
│       │       ├── Sessions.tsx          # Página de gerenciamento de inventários
│       │       ├── SessionProducts.tsx   # Produtos de uma sessão (filtros, paginação, inserção)
│       │       └── Models/SessionModel.tsx
│       ├── App.tsx                   # Roteamento e layout
│       └── App.css                   # Design system
├── Program.cs                        # Startup, DI, CORS, JWT
└── csinv.csproj
```

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- MySQL 8+ ou MariaDB

---

## Configuração

### 1. Banco de dados

Crie o banco, tabelas e views no MySQL:

```sql
CREATE DATABASE csinventory;
USE csinventory;

CREATE TABLE cs_users (
    usr_id       INT AUTO_INCREMENT PRIMARY KEY,
    usr_name     VARCHAR(255) NOT NULL,
    usr_email    VARCHAR(255) NOT NULL UNIQUE,
    usr_password VARCHAR(255) NOT NULL
);

CREATE TABLE cs_inventory_sessions (
    ses_id          INT AUTO_INCREMENT PRIMARY KEY,
    ses_year        INT NOT NULL,
    ses_month       INT NULL,
    ses_status      ENUM('active', 'finished', 'canceled') NOT NULL DEFAULT 'active',
    ses_started_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ses_finished_at DATETIME NULL,
    ses_canceled_at DATETIME NULL,
    usr_created_by  INT NULL,
    usr_finished_by INT NULL,
    usr_canceled_by INT NULL,
    FOREIGN KEY (usr_created_by) REFERENCES cs_users(usr_id),
    FOREIGN KEY (usr_finished_by) REFERENCES cs_users(usr_id),
    FOREIGN KEY (usr_canceled_by) REFERENCES cs_users(usr_id)
);

CREATE TABLE cs_products (
    pro_id   INT AUTO_INCREMENT PRIMARY KEY,
    pro_code VARCHAR(13),
    pro_name VARCHAR(255)
);

CREATE TABLE cs_inventory_items (
    inv_id         INT AUTO_INCREMENT PRIMARY KEY,
    pro_code       VARCHAR(13),
    inv_quantity   INT,
    inv_date_added DATETIME DEFAULT CURRENT_TIMESTAMP,
    ses_id         INT,
    usr_id         INT,
    FOREIGN KEY (ses_id) REFERENCES cs_inventory_sessions(ses_id),
    FOREIGN KEY (usr_id) REFERENCES cs_users(usr_id)
);

CREATE VIEW vw_inventory_items AS
SELECT
    s.ses_id,
    s.ses_year,
    s.ses_month,
    i.pro_code,
    p.pro_name,
    i.usr_id,
    u.usr_name,
    SUM(i.inv_quantity) AS total_quantity
FROM cs_inventory_items i
INNER JOIN cs_products p ON i.pro_code = p.pro_code
INNER JOIN cs_inventory_sessions s ON i.ses_id = s.ses_id
INNER JOIN cs_users u ON i.usr_id = u.usr_id
GROUP BY s.ses_id, s.ses_year, s.ses_month, i.pro_code, p.pro_name, i.usr_id, u.usr_name;

CREATE VIEW vw_inventory_sessions AS
SELECT
    s.ses_id,
    s.ses_year,
    s.ses_month,
    s.ses_status,
    s.ses_started_at,
    s.ses_finished_at,
    s.ses_canceled_at,
    COALESCE(SUM(i.inv_quantity), 0) AS totalqnt_items
FROM cs_inventory_sessions s
LEFT JOIN cs_inventory_items i ON i.ses_id = s.ses_id
GROUP BY s.ses_id, s.ses_year, s.ses_month, s.ses_status, s.ses_started_at, s.ses_finished_at, s.ses_canceled_at;
```

### 2. Variáveis de ambiente

Edite `backend/.env`:

```env
# Banco de dados
DB_HOST=localhost
DB_USER=root
DB_PASSWORD=sua_senha
DB_NAME=csinventory
DB_PORT=3306

# URLs
FRONTEND_URL=http://localhost:5173
BACKEND_URL=http://localhost:5144

# JWT
JWT_SECRET=sua-chave-secreta
JWT_ISSUER=http://localhost:5144
JWT_AUDIENCE=http://localhost:5173
```

---

## Como rodar

### Backend

```bash
# Na raiz do projeto
dotnet run
```

O servidor sobe em `http://localhost:5144`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

O frontend sobe em `http://localhost:5173`.

---

## API Reference

> Todos os endpoints (exceto login e register) exigem autenticação via **Bearer Token** no header `Authorization`.

### Autenticação — `/api/users`

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/users/register` | Registra um novo usuário |
| `POST` | `/api/users/login` | Autentica e retorna JWT |

**POST /api/users/register — body:**
```json
{
  "name": "João",
  "email": "joao@email.com",
  "password": "senha123"
}
```

**POST /api/users/login — body:**
```json
{
  "email": "joao@email.com",
  "password": "senha123"
}
```

**Resposta (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "name": "João",
  "email": "joao@email.com"
}
```

---

### Inventários (Sessões) — `/api/sessions`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/sessions/active` | Retorna o inventário ativo atual |
| `POST` | `/api/sessions/create` | Cria um novo inventário |
| `PATCH` | `/api/sessions/{sessionId}/finish` | Finaliza o inventário ativo |
| `PATCH` | `/api/sessions/{sessionId}/cancel` | Cancela o inventário ativo |
| `GET` | `/api/sessions/getall` | Lista todos os inventários com filtros e paginação |

**POST /api/sessions/create — body:**
```json
// Inventário mensal
{
  "year": 2026,
  "month": 3
}

// Inventário anual (month = null)
{
  "year": 2026,
  "month": null
}
```

**GET /api/sessions/getall — query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `year` | int | não | Filtro pelo ano |
| `month` | int | não | Filtro pelo mês (1–12) |
| `status` | string | não | Filtro pelo status (active, finished, canceled) |
| `page` | int | não | Página (padrão: 1) |
| `pageSize` | int | não | Itens por página (padrão: 10) |

**Resposta (200):**
```json
{
  "id": 1,
  "year": 2026,
  "month": 3,
  "status": "active",
  "startDate": "2026-03-01T10:00:00",
  "finishDate": null,
  "cancelDate": null,
  "totalItems": 0
}
```

> Regras:
> - Só pode existir **um inventário ativo** por vez.
> - Não pode criar inventário duplicado para o mesmo **mês/ano** (exceto cancelados).
> - Ano deve estar entre 2000 e o ano atual + 1.
> - Mês deve estar entre 1 e 12 (ou `null` para inventário anual).

---

### Produtos — `/api/products`

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/products/insert` | Insere um produto no inventário |
| `GET` | `/api/products/filter` | Lista produtos com filtros e paginação |
| `GET` | `/api/products/details/{code}` | Detalha todas as entradas de um código |
| `GET` | `/api/products/session/{sessionId}` | Lista produtos de uma sessão com filtros e paginação |
| `DELETE` | `/api/products/delete/{productId}` | Remove uma entrada de inventário |

**POST /api/products/insert — body:**
```json
{
  "code": "ABC123",
  "quantity": 10,
  "sessionId": 1
}
```

**GET /api/products/filter — query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `productName` | string | não | Filtro parcial pelo nome |
| `code` | string | não | Filtro exato pelo código |
| `year` | int | não | Filtro pelo ano |
| `month` | int | não | Filtro pelo mês (1–12, vazio = todos) |
| `page` | int | não | Página (padrão: 1) |
| `pageSize` | int | não | Itens por página (padrão: 10) |

**GET /api/products/session/{sessionId} — query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `productName` | string | não | Filtro parcial pelo nome |
| `code` | string | não | Filtro exato pelo código |
| `page` | int | não | Página (padrão: 1) |
| `pageSize` | int | não | Itens por página (padrão: 10) |

---

## Arquitetura

O backend segue o padrão **Controller → Service → Repository**:

- **Controller** — recebe requisições HTTP, extrai userId do token JWT, retorna respostas
- **Service** — contém as regras de negócio (ex: impedir inventário duplicado, validar sessão ativa, gerar JWT)
- **Repository** — executa as queries SQL via MySql.Data
- **DTOs** — desacoplam o contrato da API dos modelos internos

Todas as queries usam **parâmetros nomeados** para prevenção de SQL Injection.

As views `vw_inventory_items` e `vw_inventory_sessions` centralizam as agregações no banco, evitando lógica de cálculo no código da aplicação.

### Autenticação

- Senhas armazenadas com **BCrypt** (hash + salt)
- Token **JWT** com expiração de 8 horas
- Claims: `NameIdentifier` (userId), `Name`, `Email`
- Frontend injeta o token automaticamente via `apiFetch()` wrapper
- Token expirado (401) redireciona para a tela de login

---

## Páginas do frontend

### Login (`/`)
- Formulário com email e senha
- Redireciona para inventários após login
- Exibido automaticamente quando não há token válido

### Inventários (`/`)
- Exibe o inventário ativo com botões para finalizar ou cancelar
- Formulário para criar um novo inventário com seleção de mês (opcional) e ano (só disponível quando não há ativo)
- Se não selecionar mês, cria inventário **anual** (exibe "Anual" na tabela)
- Filtros por ano, mês e status
- Tabela paginada com todos os inventários: ID, ano, mês, status, datas e **total de itens**
- Clique na linha para ver os produtos da sessão

### Produtos da Sessão (`/sessions/:sessionId`)
- Informações da sessão (status, início, total de itens)
- Formulário de inserção de produto (código + quantidade) — apenas para sessões ativas
- Filtros por nome e código do produto
- Tabela paginada: nome, código, quantidade, data, **inserido por**, ações
- Botão de excluir entrada — apenas para sessões ativas

---

## Scripts úteis

```bash
# Backend — restaurar pacotes e compilar
dotnet build

# Frontend — lint
cd frontend && npm run lint

# Frontend — build de produção
cd frontend && npm run build
```
