# 🗄️ DatabaseSettingService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **DatabaseSettingService** stores and manages the database connection credentials for each company. Because RecoMind's AI layer needs to query each client's own database, this service acts as the secure vault for those credentials — storing passwords with AES-256 encryption and exposing decrypted connection details only to trusted internal consumers.

### Main Responsibilities

- Store and manage a company's database connection settings (server, DB name, user, password)
- Encrypt passwords at rest using AES-256
- Expose a safe public-facing response (no credentials) for managers
- Expose a decrypted response for the internal AI service
- Expose gRPC endpoints for internal service-to-service queries

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Encryption | AES-256 (System.Security.Cryptography) |
| Inter-Service (Server) | gRPC (exposes endpoints) |
| Object Mapping | AutoMapper 12 |
| Unit Testing | xUnit, Moq, FluentAssertions |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
DatabaseSettingService/
├── DatabaseSetting.Core/                  # Domain layer
│   ├── DTOs/
│   │   ├── CreateDbSettingDto.cs          # Input for creating a setting
│   │   ├── UpdateDbSettingDto.cs          # Input for updating (all fields optional)
│   │   ├── DbSettingResponseDto.cs        # Safe public response (no credentials)
│   │   └── DbSettingResponseForAiDto.cs   # Full response with decrypted password (AI use)
│   ├── Entities/
│   │   └── DbSettingModel.cs              # Domain entity
│   ├── Interfaces/
│   │   ├── IDbSettingRepository.cs        # Repository contract
│   │   ├── IDbSettingService.cs           # Service contract
│   │   └── IEncryptionService.cs          # Encryption contract
│   ├── Mapper/
│   │   └── MappingProfile.cs              # AutoMapper profiles
│   ├── Result/                            # Result pattern
│   │   ├── Error.cs
│   │   ├── Result.cs
│   │   └── DbSettingErrors.cs
│   └── Services/
│       ├── DbSettingService.cs            # Core business logic
│       └── EncryptionService.cs           # AES-256 encrypt/decrypt
│
├── DatabaseSetting.Infrastructure/        # Data access layer
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Migrations/
│   └── Repositories/
│       └── DbSettingRepository.cs
│
├── DatabaseSetting.WebApi/                # Presentation layer
│   ├── Controllers/
│   │   └── DbSettingController.cs        # REST API endpoints
│   ├── GrpcServices/
│   │   └── DbSettingGrpcService.cs       # gRPC server
│   ├── Protos/
│   │   └── dbsetting.proto
│   ├── Program.cs
│   └── appsettings.json
│
├── DatabaseSetting.Tests/                 # Unit tests
│   └── Services/
│       ├── DbSettingServiceTests.cs
│       └── EncryptionServiceTests.cs
│
├── Dockerfile
└── DatabaseSettingService.sln
```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api/dbsetting`

---

### 4.1 Get Settings for AI (Internal)

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/dbsetting/company/{company_Id}` |
| **Auth** | None (public — intended for internal AI service calls) |
| **Description** | Returns the full connection details **including the decrypted password**. Intended for internal use by the AI service only. |

**Response Example (200 OK):**
```json
{
  "id": "setting-uuid",
  "companyId": "company-uuid",
  "server": "sql.example.com",
  "dbName": "ClientDatabase",
  "user": "db_user",
  "password": "plain_text_password"
}
```

---

### 4.2 Get Settings for Manager

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/dbsetting` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Returns non-sensitive metadata (no credentials) for the authenticated company. |

**Response Example (200 OK):**
```json
{
  "id": "setting-uuid",
  "companyId": "company-uuid",
  "name": "Production DB",
  "dbType": "SqlServer",
  "createdAt": "2026-04-26T11:33:00Z"
}
```

---

### 4.3 Get Setting by ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/dbsetting/{id}` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Returns metadata for a specific setting, validated against the caller's company. |

**Response Example (200 OK):** Same shape as 4.2.

**Response (404 Not Found):** HTTP 404

---

### 4.4 Create Database Setting

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/dbsetting/create` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Creates the database connection settings for the company. Each company can have only **one** setting. The password is encrypted before storage. |

**Request Body:**
```json
{
  "name": "Production DB",
  "dbType": "SqlServer",
  "server": "sql.example.com",
  "dbName": "ClientDatabase",
  "user": "db_user",
  "password": "plain_text_password"
}
```

> `name` and `dbType` are optional. `server`, `dbName`, `user`, `password` are required.

**Response Example (200 OK):**
```json
{
  "id": "new-setting-uuid",
  "companyId": "company-uuid",
  "name": "Production DB",
  "dbType": "SqlServer",
  "createdAt": "2026-05-17T10:00:00Z"
}
```

**Response (400 Bad Request — already exists):**
```json
{ "message": "Database settings for this company already exist." }
```

---

### 4.5 Update Database Setting

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/dbsetting/update/{id}` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Updates the database settings. All fields are optional. If `password` is provided, it will be re-encrypted. |

**Request Body:**
```json
{
  "name": "Staging DB",
  "server": "staging.sql.example.com",
  "password": "new_plain_password"
}
```

**Response Example (200 OK):** Same shape as Create response.

**Response (404 Not Found):** HTTP 404

---

### 4.6 Delete Database Setting

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/dbsetting/delete/{id}` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Deletes the database settings for the given ID, validated against the caller's company. |

**Response:** HTTP 204 No Content

**Response (404 Not Found):** HTTP 404

---

## 5. Database Design

### Table: `DbSettings`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `Name` | `nvarchar(max)` | Optional label |
| `CompanyId` | `nvarchar(max)` | Required — one record per company |
| `DbType` | `nvarchar(max)` | Optional (e.g., `SqlServer`, `PostgreSQL`) |
| `Server` | `nvarchar(max)` | Required |
| `DbName` | `nvarchar(max)` | Required |
| `User` | `nvarchar(max)` | Required |
| `Password` | `nvarchar(max)` | Required — **stored encrypted (AES-256)** |
| `CreatedAt` | `datetime2` | Nullable |
| `UpdatedAt` | `datetime2` | Nullable |

### Business Constraint
There is a **one-to-one** relationship between a company and its database settings, enforced at the service layer:
- `CreateAsync` checks `GetByCompanyIdAsync` first and returns `AlreadyExists` if a record is found.

---

## 6. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token |
| **Claim used** | `CompanyId` — scopes all manager operations to their company |
| **HTTPS** | Required |
| **Password Encryption** | AES-256 (CBC mode) using a fixed Key (32 bytes) and IV (16 bytes) from configuration |

### Authorization Policies

| Policy | Allowed Roles |
|---|---|
| `ManagerRole` | `admin`, `manager` |

---

## 7. Service Communication

### As a gRPC Server

The DatabaseSettingService exposes two gRPC methods for internal service consumption:

| gRPC Method | Description |
|---|---|
| `GetById(id, companyId)` | Returns safe metadata for a specific setting |
| `GetByCompanyId(companyId)` | Returns safe metadata for a company's setting |

- Proto: `dbsetting.proto`
- Namespace: `DbSetting.Grpc`
- gRPC Port: configured via `GRPC_PORT` environment variable (default: `5001`)
- HTTP Port: configured via `HTTP_PORT` environment variable (default: `8001`)

**Proto Response Shape:**
```protobuf
message DbSettingResponse {
  string id = 1;
  string companyId = 2;
  string name = 3;
  string dbType = 4;
  string createdAt = 5;
}
```

---

## 8. Architecture Notes

### Result Pattern
All service methods return `Result<T>`. Business errors (not found, already exists) are returned as `Error` records rather than exceptions, keeping the happy path and error path explicit and testable.

### Two Response DTOs — By Design
The service has two distinct response shapes:

- `DbSettingResponseDto` — safe, no credentials. Used for manager-facing endpoints and gRPC.
- `DbSettingResponseForAiDto` — includes the **decrypted** password. Used exclusively by the AI service endpoint.

This separation ensures credentials are never accidentally leaked through the wrong endpoint.

### Clean Architecture
- **Core** — zero infrastructure dependencies. Business rules and interfaces only.
- **Infrastructure** — EF Core, repository implementation.
- **WebApi** — controllers, gRPC server, DI wiring.

### Encryption as a Service
`EncryptionService` is registered as a **Singleton** (stateless, no shared mutable state). The service is injected into `DbSettingService` through the `IEncryptionService` interface, making it mockable in tests.

### One-Record-Per-Company Constraint
Enforced purely in the service layer — no unique database constraint exists on `CompanyId`. If concurrent creation requests race, a duplicate could theoretically be inserted. A database-level unique index on `CompanyId` would make this bulletproof.

---

*RecoMind Project — DatabaseSettingService*
