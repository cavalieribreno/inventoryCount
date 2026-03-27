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
│   ├── .env                            # Variáveis de ambiente (banco + URLs + JWT)
│   ├── Program.cs                      # Startup, DI, CORS, JWT
│   ├── csinv.csproj
│   └── Modules/
│       ├── Database/
│       │   └── DatabaseConnection.cs   # Factory de conexão MySQL
│       ├── Users/
│       │   ├── Interfaces/             # IUserRepository, IUserService
│       │   ├── UserController.cs
│       │   ├── UserService.cs
│       │   ├── UserRepository.cs
│       │   └── UsersDTO.cs
│       ├── Inventory/
│       │   ├── Interfaces/             # IInventoryProductsRepository, IInventoryProductsService
│       │   ├── InventoryProductsController.cs
│       │   ├── InventoryProductsService.cs
│       │   ├── InventoryProductsRepository.cs
│       │   └── InventoryProductsDTO.cs
│       └── InventorySessions/
│           ├── Interfaces/             # ISessionRepository, ISessionService
│           ├── SessionController.cs
│           ├── SessionService.cs
│           ├── SessionRepository.cs
│           └── SessionDTO.cs
├── frontend/
│   └── src/
│       ├── context/
│       │   └── AuthContext.tsx          # Context de autenticação (JWT + localStorage)
│       ├── services/
│       │   └── api.ts                  # Fetch wrapper com token JWT e redirect 401
│       ├── modules/
│       │   ├── Auth/
│       │   │   └── Login.tsx           # Página de login
│       │   ├── Products/
│       │   │   └── Models/
│       │   │       └── ProductModel.tsx # GroupedProduct, ProductDetails
│       │   └── Sessions/
│       │       ├── Sessions.tsx         # Página de inventários (listagem, filtros, criação)
│       │       ├── SessionProducts.tsx  # Produtos agrupados, modal de detalhes, inserção
│       │       └── Models/
│       │           └── SessionModel.tsx # Session
│       ├── App.tsx                     # Roteamento, layout e header com perfil
│       └── App.css                     # Design system completo
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
    usr_id            INT AUTO_INCREMENT PRIMARY KEY,
    usr_name          VARCHAR(100) NOT NULL,
    usr_email         VARCHAR(150) NOT NULL UNIQUE,
    usr_password_hash VARCHAR(255) NOT NULL,
    usr_created_at    DATETIME DEFAULT CURRENT_TIMESTAMP,
    usr_active        TINYINT(1) DEFAULT 1
);

CREATE TABLE cs_inventory_sessions (
    ses_id     INT AUTO_INCREMENT PRIMARY KEY,
    ses_year   INT NOT NULL,
    ses_month  INT NULL,
    ses_status ENUM('active', 'finished', 'canceled') NOT NULL DEFAULT 'active'
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

CREATE TABLE cs_history (
    his_id     INT AUTO_INCREMENT PRIMARY KEY,
    ses_id     INT NOT NULL,
    usr_id     INT NOT NULL,
    his_action ENUM('created', 'finished', 'canceled') NOT NULL,
    his_date   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
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
    session.ses_id,
    session.ses_year,
    session.ses_month,
    session.ses_status,
    COALESCE(SUM(items.inv_quantity), 0) AS totalqnt_items,
    history_created.his_date AS ses_started_at,
    user_created.usr_name AS created_by_name,
    history_finished.his_date AS ses_finished_at,
    user_finished.usr_name AS finished_by_name,
    history_canceled.his_date AS ses_canceled_at,
    user_canceled.usr_name AS canceled_by_name
FROM cs_inventory_sessions session
LEFT JOIN cs_inventory_items items ON items.ses_id = session.ses_id
LEFT JOIN cs_history history_created ON history_created.ses_id = session.ses_id AND history_created.his_action = 'created'
LEFT JOIN cs_users user_created ON history_created.usr_id = user_created.usr_id
LEFT JOIN cs_history history_finished ON history_finished.ses_id = session.ses_id AND history_finished.his_action = 'finished'
LEFT JOIN cs_users user_finished ON history_finished.usr_id = user_finished.usr_id
LEFT JOIN cs_history history_canceled ON history_canceled.ses_id = session.ses_id AND history_canceled.his_action = 'canceled'
LEFT JOIN cs_users user_canceled ON history_canceled.usr_id = user_canceled.usr_id
GROUP BY session.ses_id, session.ses_year, session.ses_month, session.ses_status,
         history_created.his_date, user_created.usr_name,
         history_finished.his_date, user_finished.usr_name,
         history_canceled.his_date, user_canceled.usr_name;
```

### 2. Variáveis de ambiente

Edite `backend/.env` (backend):

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

Crie `frontend/.env`:

```env
VITE_API_URL=http://localhost:5144
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
| `GET` | `/api/sessions/{sessionId}` | Retorna um inventário pelo ID |
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
  "totalItems": 0,
  "createdByName": "João",
  "finishedByName": null,
  "canceledByName": null
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
| `GET` | `/api/products/catalog?code={code}` | Busca um produto do catálogo pelo código |
| `POST` | `/api/products/insert` | Insere um produto no inventário |
| `GET` | `/api/products/session/{sessionId}/grouped` | Lista produtos agrupados por código com quantidade total |
| `GET` | `/api/products/session/{sessionId}/details/{code}` | Lista todas as inserções individuais de um produto na sessão |
| `DELETE` | `/api/products/delete/{productId}` | Remove uma entrada de inventário |

> Regras:
> - O código do produto deve existir na tabela `cs_products`.
> - A sessão deve estar ativa para inserir, excluir produtos.

**GET /api/products/catalog — resposta (200):**
```json
{
  "code": "ABC123",
  "productName": "Baton"
}
```

**POST /api/products/insert — body:**
```json
{
  "code": "ABC123",
  "quantity": 10,
  "sessionId": 1
}
```

**GET /api/products/session/{sessionId}/grouped — query params:**

| Parâmetro | Tipo | Obrigatório | Descrição |
|-----------|------|-------------|-----------|
| `productName` | string | não | Filtro parcial pelo nome |
| `code` | string | não | Filtro exato pelo código |
| `page` | int | não | Página (padrão: 1) |
| `pageSize` | int | não | Itens por página (padrão: 10) |

**Resposta (200):**
```json
[
  {
    "code": "ABC123",
    "productName": "Baton",
    "totalQuantity": 65
  }
]
```

**GET /api/products/session/{sessionId}/details/{code} — resposta (200):**
```json
[
  {
    "id": 1,
    "code": "ABC123",
    "productName": "Baton",
    "quantity": 10,
    "year": 2026,
    "month": 3,
    "dateHour": "2026-03-14T19:39:33",
    "userName": "Breno"
  }
]
```

---

## Arquitetura

O backend segue o padrão **Controller → Service → Repository**:

- **Controller** — recebe requisições HTTP, extrai userId do token JWT, retorna respostas
- **Service** — contém as regras de negócio (ex: impedir inventário duplicado, validar sessão ativa, gerar JWT)
- **Repository** — executa as queries SQL via MySql.Data
- **DTOs** — desacoplam o contrato da API dos modelos internos

Todas as queries usam **parâmetros nomeados** para prevenção de SQL Injection.

As views `vw_inventory_items` e `vw_inventory_sessions` centralizam as agregações no banco, evitando lógica de cálculo no código da aplicação.

A tabela `cs_history` registra todas as ações de sessão (criação, finalização, cancelamento) com data e usuário responsável, substituindo as colunas de rastreamento que antes ficavam diretamente na tabela de sessões.

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
- Header com nome do usuário e popup de perfil (nome e email) ao passar o mouse
- Exibe o inventário ativo com botões para finalizar ou cancelar
- Formulário para criar um novo inventário com seleção de mês (opcional) e ano (só disponível quando não há ativo)
- Se não selecionar mês, cria inventário **anual** (exibe "Anual" na tabela)
- Filtros por ano, mês e status
- Tabela paginada com todos os inventários: ano, mês, status, início e **total de itens**
- Clique na linha para ver os produtos da sessão

### Produtos da Sessão (`/sessions/:sessionId`)
- Informações da sessão (status, início, criado por, total de itens, finalizado/cancelado por — quando aplicável)
- Formulário de inserção de produto (código + quantidade) — apenas para sessões ativas
- Filtros por nome e código do produto
- Tabela paginada com produtos **agrupados por código**: nome, código, quantidade total
- Botão "Detalhes" abre um **modal** com todas as inserções individuais do produto (quantidade, data, inserido por)
- Botão de excluir entrada no modal — apenas para sessões ativas

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
