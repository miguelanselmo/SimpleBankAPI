# SimpleBankAPI

A RESTful banking API built with ASP.NET Core 6 following the Clean Architecture (Ports & Adapters) pattern. It supports user registration and authentication, bank account management, fund transfers, and document uploads.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Local Setup](#local-setup)
  - [Docker Setup](#docker-setup)
- [Configuration](#configuration)
- [Database Schema](#database-schema)
- [API Endpoints](#api-endpoints)
- [Running Tests](#running-tests)

---

## Architecture

The solution is organised into the following layers:

```
SimpleBankAPI/
├── Core/               # Domain entities and enums (no external dependencies)
├── Application/        # Use-case interfaces (ports) and use-case implementations
├── Infrastructure/     # Adapters: database repositories, cache, file storage, authentication provider
└── WebApi/             # ASP.NET Core controllers, request/response models, validators
```

| Layer | Folder | Responsibility |
|-------|--------|----------------|
| Core | `Core/` | `User`, `Account`, `Movement`, `Transfer`, `Session`, `Document` entities and `Currency` enum |
| Application | `Application/` | Business logic use cases: `UserUseCase`, `AccountUseCase`, `TransferUseCase`, `SessionUseCase` |
| Infrastructure | `Infrastructure/` | PostgreSQL repositories (Dapper), Redis cache, AWS S3 file repository, JWT authentication provider |
| WebApi | `WebApi/` | REST controllers, FluentValidation validators, AutoMapper models |

---

## Tech Stack

| Concern | Library / Technology |
|---------|---------------------|
| Framework | ASP.NET Core 6 |
| Database | PostgreSQL (via **Dapper** + **Npgsql**) |
| Cache | Redis (StackExchange.Redis) |
| Messaging | Apache Kafka |
| File Storage | AWS S3 (LocalStack for local development) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Password Hashing | BCrypt.Net-Core |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Logging | Serilog (Console, File, PostgreSQL, Seq sinks) |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Containerisation | Docker + Docker Compose |
| CI | Azure Pipelines |
| Testing | xUnit + Moq |

---

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for the infrastructure services)
- PostgreSQL 11+ (or use the provided Docker Compose file)
- Redis (or use Docker Compose)

---

## Getting Started

### Local Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/miguelanselmo/SimpleBankAPI.git
   cd SimpleBankAPI
   ```

2. **Start the infrastructure services** (PostgreSQL, Redis, Kafka, LocalStack)

   ```bash
   docker-compose -f SimpleBankAPI/docker-compose.yml up -d
   ```

3. **Apply the database schema**

   Connect to your PostgreSQL instance and execute the SQL script:

   ```bash
   psql -h localhost -U postgres -d postgres -f SimpleBankAPI/SimpleBank.sql
   ```

4. **Configure the application** — see the [Configuration](#configuration) section below.

5. **Run the API**

   ```bash
   dotnet run --project SimpleBankAPI/SimpleBankAPI.csproj
   ```

   The API will be available at `https://localhost:5001` (or `http://localhost:5000`).  
   Swagger UI is accessible at `https://localhost:5001/swagger` when running in the Development environment.

### Docker Setup

Build and run the API container alongside its dependencies:

```bash
# Build the image
docker build -t simplebankapi -f SimpleBankAPI/Dockerfile .

# Start everything with Docker Compose (add the API service to docker-compose.yml if desired)
docker-compose -f SimpleBankAPI/docker-compose.yml up --build
```

---

## Configuration

Copy `appsettings.Development.json` and edit the relevant sections in your environment-specific `appsettings.json` (or use environment variables / User Secrets):

```json
{
  "ConnectionStrings": {
    "BankDB": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "<your-secret-key-min-32-chars>",
    "Issuer": "SimpleBank",
    "Audience": "SimpleBank",
    "AccessTokenDuration": 5,
    "RefreshTokenDuration": 60
  },
  "Cache": {
    "Redis": "localhost:6379"
  },
  "FileRepo": {
    "Path": "/tmp/simplebankapi/"
  }
}
```

| Key | Description |
|-----|-------------|
| `ConnectionStrings:BankDB` | PostgreSQL connection string |
| `Jwt:Key` | Symmetric signing key for JWT tokens (≥ 32 characters) |
| `Jwt:AccessTokenDuration` | Access token lifetime in minutes |
| `Jwt:RefreshTokenDuration` | Refresh token lifetime in minutes |
| `Cache:Redis` | Redis connection string |
| `FileRepo:Path` | Local path used by the file repository |

---

## Database Schema

The schema is defined in [`SimpleBankAPI/SimpleBank.sql`](SimpleBankAPI/SimpleBank.sql).

| Table | Description |
|-------|-------------|
| `users` | Registered users (username, email, hashed password, full name) |
| `accounts` | Bank accounts linked to a user (balance, currency) |
| `movements` | Credit / debit movements on an account |
| `sessions` | Active login sessions with refresh-token info |
| `documents` | Metadata for documents uploaded against an account (stored in S3) |
| `operations_log` | Audit log of API operations (JSON payload) |

---

## API Endpoints

All routes are prefixed with `/simplebankapi/v1/`.  
Endpoints marked 🔒 require a valid JWT Bearer token in the `Authorization` header.

### User

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/user` | — | Register a new user |
| `POST` | `/user/login` | — | Log in and receive access & refresh tokens |
| `POST` | `/user/logout` | 🔒 | Invalidate the current session |
| `POST` | `/user/renewlogin` | 🔒 | Exchange a refresh token for new tokens |

#### Register — request body
```json
{
  "userName": "john",
  "email": "john@example.com",
  "password": "P@ssw0rd",
  "fullName": "John Doe"
}
```

#### Login — request body
```json
{
  "userName": "john",
  "password": "P@ssw0rd"
}
```

---

### Account

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/account` | 🔒 | Create a new bank account |
| `GET` | `/account` | 🔒 | List all accounts for the authenticated user |
| `GET` | `/account/{id}` | 🔒 | Get account details including movements |
| `POST` | `/account/{id}/doc` | 🔒 | Upload a document (max 2 MB) for an account |
| `GET` | `/account/{id}/doc` | 🔒 | List all documents for an account |
| `GET` | `/account/{id}/doc/{docId}` | 🔒 | Download a specific document |

#### Create Account — request body
```json
{
  "amount": 1000.00,
  "currency": "EUR"
}
```

Supported currencies: `USD`, `EUR`

---

### Transfer

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/transfer` | 🔒 | Transfer funds between two accounts |

#### Transfer — request body
```json
{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 250.00
}
```

---

## Running Tests

The test project (`SimpleBankAPI.Tests`) uses **xUnit** and **Moq**.

```bash
dotnet test SimpleBankAPI.Tests/SimpleBankAPI.Tests.csproj
```

Test coverage includes:
- `AccountUseCaseTest` — unit tests for account use-case logic
- `TransferUseCaseTest` — unit tests for transfer use-case logic
