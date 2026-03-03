# CSINV — Sistema de Inventário Cacau Show

Sistema web para gerenciamento de inventário de produtos da Cacau Show. Permite criar sessões de inventário, registrar produtos com quantidades, filtrar e consultar o histórico de entradas.

---

## Stack

| Camada | Tecnologia |
|--------|-----------|
| Backend | C# .NET 10.0 · ASP.NET Core Web API |
| Banco de dados | MySQL / MariaDB |
| Frontend | React 19 · TypeScript · Vite |

---

## Estrutura do projeto

```
csinv/
├── backend/
│   ├── .env                          # Variáveis de ambiente (banco + URLs)
│   └── Modules/
│       ├── Database/
│       │   └── DatabaseConnection.cs # Factory de conexão MySQL
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
│       ├── modules/
│       │   ├── Products/
│       │   │   ├── ListProducts.tsx      # Página de listagem e filtros
│       │   │   ├── InsertProducts.tsx    # Página de inserção
│       │   │   └── Models/ProductModel.tsx
│       │   └── Sessions/
│       │       ├── Sessions.tsx          # Página de gerenciamento de inventários
│       │       └── Models/SessionModel.tsx
│       ├── App.tsx                   # Roteamento e layout
│       └── App.css                   # Design system
├── Program.cs                        # Startup, DI, CORS
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

CREATE TABLE cs_inventory_sessions (
    ses_id          INT AUTO_INCREMENT PRIMARY KEY,
    ses_year        INT NOT NULL,
    ses_status      ENUM('active', 'finished') NOT NULL DEFAULT 'active',
    ses_started_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ses_finished_at DATETIME NULL
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
    FOREIGN KEY (ses_id) REFERENCES cs_inventory_sessions(ses_id)
);

CREATE VIEW vw_inventory_items AS
SELECT
    s.ses_year        AS Year,
    i.pro_code        AS Code,
    p.pro_name        AS ProductName,
    SUM(i.inv_quantity) AS TotalQuantity
FROM cs_inventory_items i
JOIN cs_inventory_sessions s ON i.ses_id = s.ses_id
LEFT JOIN cs_products p ON i.pro_code = p.pro_code
GROUP BY s.ses_year, i.pro_code, p.pro_name;

CREATE VIEW vw_inventory_sessions AS
SELECT
    s.ses_id,
    s.ses_year,
    s.ses_status,
    s.ses_started_at,
    s.ses_finished_at,
    COUNT(i.inv_id) AS totalqnt_items
FROM cs_inventory_sessions s
LEFT JOIN cs_inventory_items i ON s.ses_id = i.ses_id
GROUP BY s.ses_id, s.ses_year, s.ses_status, s.ses_started_at, s.ses_finished_at;
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

### Inventários (Sessões) — `/api/sessions`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/sessions/active` | Retorna o inventário ativo atual |
| `POST` | `/api/sessions/create` | Cria um novo inventário |
| `PATCH` | `/api/sessions/{sessionId}/finish` | Finaliza o inventário ativo |
| `GET` | `/api/sessions/getall` | Lista todos os inventários com total de itens |

**POST /api/sessions/create — body:**
```json
{
  "year": 2025
}
```

**Resposta (200):**
```json
{
  "id": 1,
  "year": 2025,
  "status": "active",
  "startDate": "2025-03-01T10:00:00",
  "finishDate": null,
  "totalItems": 0
}
```

> Regra: só pode existir **um inventário ativo** por vez. Tentar criar um segundo retorna erro 400.

---

### Produtos — `/api/products`

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/products/insert` | Insere um produto no inventário |
| `GET` | `/api/products/filter` | Lista produtos com filtros e paginação |
| `GET` | `/api/products/details/{code}` | Detalha todas as entradas de um código |
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
| `page` | int | não | Página (padrão: 1) |
| `pageSize` | int | não | Itens por página (padrão: 10) |

**Resposta (200):**
```json
[
  {
    "productName": "Trufa de Chocolate",
    "code": "ABC123",
    "year": 2025,
    "totalQuantity": 50
  }
]
```

---

## Arquitetura

O backend segue o padrão **Controller → Service → Repository**:

- **Controller** — recebe requisições HTTP, valida entrada básica, retorna respostas
- **Service** — contém as regras de negócio (ex: impedir inventário duplicado, validar sessão ativa)
- **Repository** — executa as queries SQL via MySql.Data
- **DTOs** — desacoplam o contrato da API dos modelos internos

Todas as queries usam **parâmetros nomeados** para prevenção de SQL Injection.

As views `vw_inventory_items` e `vw_inventory_sessions` centralizam as agregações no banco, evitando lógica de cálculo no código da aplicação.

---

## Páginas do frontend

### Inventários (`/sessions`)
- Exibe o inventário ativo com botão para finalizar
- Formulário para criar um novo inventário (só disponível quando não há ativo)
- Tabela com todos os inventários: ID, ano, status, datas e **total de itens**

### Listagem de Produtos (`/`)
- Filtros por nome, código e ano
- Tabela paginada (10 itens por página)
- Botão `...` abre modal com todas as entradas individuais do produto
- Cada entrada pode ser excluída diretamente no modal

### Inserção de Produto (`/insert`)
- Formulário com código do produto, quantidade e ID da sessão
- Validação feita pelo backend

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
