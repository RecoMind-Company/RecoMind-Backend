# 🏢 TeamService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **TeamService** is responsible for managing teams within a company. It handles the full lifecycle of a team — creation, updates, deletion — as well as managing team membership (adding/removing employees). It also exposes internal gRPC endpoints consumed by other microservices.

### Main Responsibilities

- Create, update, and delete teams scoped to a company
- Add and remove employees from teams
- Expose team information to the AI service and other internal consumers
- Retrieve team membership and job titles via gRPC calls to the AuthService
- Expose gRPC endpoints for other services to query team membership

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Inter-Service (Client) | gRPC (calls AuthService) |
| Inter-Service (Server) | gRPC (exposes endpoints for other services) |
| Object Mapping | AutoMapper 12 |
| Unit Testing | xUnit, Moq, FluentAssertions |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
TeamService/
├── Team.Core/                        # Domain layer (pure business logic)
│   ├── DTOs/                         # Data Transfer Objects (request/response shapes)
│   │   ├── CreateTeamDto.cs
│   │   ├── UpdateTeamDto.cs
│   │   ├── TeamResponseDto.cs
│   │   ├── TeamResponseForAiDto.cs
│   │   ├── EmployeeDto.cs
│   │   ├── UserTeamInfoDto.cs
│   │   └── UserJobTitleDto.cs
│   ├── Interfaces/                   # Abstractions (contracts)
│   │   ├── ITeamRepository.cs
│   │   ├── ITeamService.cs
│   │   └── IAuthGrpcService.cs
│   ├── Models/                       # Domain entities
│   │   ├── TeamModel.cs
│   │   └── TeamEmployee.cs
│   ├── Mapper/
│   │   └── TeamProfile.cs            # AutoMapper profiles
│   ├── Result/                       # Result pattern (Error, Result<T>, TeamErrors)
│   │   ├── Error.cs
│   │   ├── Result.cs
│   │   └── TeamErrors.cs
│   └── Services/
│       └── TeamService.cs            # Core business logic
│
├── Team.Infrastructure/              # Data access & external integrations
│   ├── Data/
│   │   └── TeamDbContext.cs          # EF Core DbContext
│   ├── Migrations/                   # EF Core database migrations
│   ├── Repositories/
│   │   └── TeamRepository.cs        # EF Core repository implementation
│   └── gRPC/                        # gRPC client (calls AuthService)
│       ├── AccountMessages.proto
│       ├── AccountService.proto
│       └── AuthGrpcService.cs
│
├── Team.WebApi/                      # Presentation layer (HTTP + gRPC server)
│   ├── Controllers/
│   │   └── TeamController.cs        # REST API endpoints
│   ├── GrpcServices/
│   │   └── TeamGrpcService.cs       # gRPC server implementation
│   ├── Protos/                      # Proto definitions (server-side)
│   │   ├── TeamService.proto
│   │   └── teamMessages.proto
│   ├── Program.cs                   # App startup & DI configuration
│   └── appsettings.json
│
├── Team.Tests/                       # Unit tests
│   └── Services/
│       └── TeamServiceTests.cs
│
├── Dockerfile
└── TeamService.sln
```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api/team`
> All protected endpoints require a valid **JWT Bearer Token** in the `Authorization` header.

---

### 4.1 Get Team Job Titles (Internal / AI)

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/team/{teamId}/company/{company_id}` |
| **Auth** | None (public — used internally by AI service) |
| **Description** | Returns a distinct list of job titles for all members of a given team. Fetches employee data from AuthService via gRPC. |

**Response Example (200 OK):**
```json
["Developer", "Manager", "QA Engineer"]
```

**Response (404 Not Found):**
```json
"Team not found or has no employees."
```

---

### 4.2 Get All Teams for a Company (AI)

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/team/company/{company_id}` |
| **Auth** | None (public — used internally by AI service) |
| **Description** | Returns a lightweight list of teams (id + name only) for the given company. |

**Response Example (200 OK):**
```json
[
  { "id": "team-uuid-1", "name": "Backend Team" },
  { "id": "team-uuid-2", "name": "QA Team" }
]
```

---

### 4.3 Get All Teams for Authenticated Company

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/team/get-all` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Returns full team details for the company extracted from the JWT `CompanyId` claim. |

**Response Example (200 OK):**
```json
[
  {
    "id": "team-uuid-1",
    "name": "Backend Team",
    "teamLeadId": "user-uuid-lead",
    "companyId": "company-uuid",
    "employees": ["emp-uuid-1", "emp-uuid-2"],
    "createdAt": "2026-04-01T10:00:00Z",
    "updatedAt": "2026-04-15T12:00:00Z"
  }
]
```

---

### 4.4 Get Team by ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/team/{teamId}` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Returns full details of a specific team by its ID. |

**Response Example (200 OK):**
```json
{
  "id": "team-uuid-1",
  "name": "Backend Team",
  "teamLeadId": "user-uuid-lead",
  "companyId": "company-uuid",
  "employees": ["emp-uuid-1"],
  "createdAt": "2026-04-01T10:00:00Z",
  "updatedAt": "2026-04-15T12:00:00Z"
}
```

**Response (404 Not Found):** HTTP 404

---

### 4.5 Create Team

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/team/create` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Creates a new team under the authenticated user's company. Team name must be unique per company. |

**Request Body:**
```json
{
  "name": "Backend Team",
  "teamLeadId": "user-uuid-lead"
}
```

> `teamLeadId` is optional.

**Response Example (200 OK):**
```json
{
  "id": "new-team-uuid",
  "name": "Backend Team",
  "teamLeadId": "user-uuid-lead",
  "companyId": "company-uuid",
  "employees": [],
  "createdAt": "2026-05-17T10:00:00Z",
  "updatedAt": "2026-05-17T10:00:00Z"
}
```

**Response (400 Bad Request — duplicate name):**
```json
{ "message": "A team with the same name already exists in this company." }
```

---

### 4.6 Update Team

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/team/update/{teamId}` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Updates team name and/or team lead. Only non-null fields are applied. Name uniqueness is validated only when the name actually changes. |

**Request Body:**
```json
{
  "name": "Frontend Team",
  "teamLeadId": "new-lead-uuid"
}
```

**Response Example (200 OK):** Same shape as Create response.

**Response (404 Not Found):** HTTP 404

---

### 4.7 Delete Team

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/team/delete/{teamId}` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Deletes a team if it belongs to the authenticated user's company. |

**Response:** HTTP 204 No Content

**Response (404 Not Found):** HTTP 404

---

### 4.8 Add Employee to Team

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/team/{teamId}/employees` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Adds an employee to a team. Fails if the employee is already a member. |

**Request Body:**
```json
{
  "employeeId": "emp-uuid-1"
}
```

**Response:** HTTP 200 OK

**Response (400 Bad Request):**
```json
{ "message": "Cannot add employee to team." }
```

---

### 4.9 Remove Employee from Team

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/team/{teamId}/employees/{employeeId}` |
| **Auth** | Required — Roles: `admin`, `manager`, `teamleader`, `employee` |
| **Description** | Removes an employee from a team. |

**Response:** HTTP 204 No Content

---

## 5. Database Design

### Tables

#### `Teams`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `Name` | `nvarchar(max)` | Required |
| `CompanyId` | `nvarchar(max)` | Required — tenant identifier |
| `TeamLeadId` | `nvarchar(max)` | Nullable |
| `CreatedAt` | `datetime2` | Nullable |
| `UpdatedAt` | `datetime2` | Nullable |

#### `TeamEmployees`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `TeamId` | `nvarchar(450)` | FK → `Teams.Id` (Cascade Delete) |
| `EmployeeId` | `nvarchar(max)` | References user in AuthService |

### Relationships

```
Teams (1) ──────< TeamEmployees (many)
```

- One team can have many employees.
- Deleting a team cascades and removes all its `TeamEmployee` records.
- `EmployeeId` is not a foreign key to a local table — it references users managed by the **AuthService** (cross-service ownership).

---

## 6. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token |
| **Claim used** | `CompanyId` — extracted from JWT to scope all operations |
| **HTTPS** | Required (`RequireHttpsMetadata = true`) |
| **Clock Skew** | Zero tolerance |

### Authorization Policies

| Policy | Allowed Roles |
|---|---|
| `AllEmployees` | `admin`, `manager`, `teamleader`, `employee` |
| `Leadership` | `admin`, `manager`, `teamleader` |
| `Management` | `admin`, `manager` |

> **Note:** All company-scoped write operations use the `CompanyId` claim from the JWT, not from the request body, to prevent cross-company data access.

---

## 7. Service Communication

### As a gRPC Client (calls AuthService)

The TeamService calls the **AuthService** via gRPC to fetch job titles for team members:

```
TeamService ──gRPC──> AuthService
  GetJobTitlesList(userIds[]) → List<{userId, jobTitle}>
```

- Proto: `AccountService.proto` + `AccountMessages.proto`
- Used in: `GetTeamMemberJobTitlesAsync()`
- Configured in `appsettings.json` under `Urls:AuthServiceUrl`

### As a gRPC Server (consumed by other services)

The TeamService exposes two gRPC methods:

| gRPC Method | Description |
|---|---|
| `GetTeamByEmployee(userId)` | Returns team info for an employee or team lead |
| `UserExist(userId, teamId)` | Checks whether a user belongs to a specific team |

- Proto: `TeamService.proto` + `teamMessages.proto`
- Port: configured via `GRPC_PORT` environment variable (default: `5001`)

### HTTP Port

- Configured via `HTTP_PORT` environment variable (default: `8001`)
- HTTP/1 only on the HTTP port; HTTP/2 only on the gRPC port

---

## 8. Architecture Notes

### Result Pattern
All service methods return `Result<T>` — a discriminated union wrapping either a success value or an `Error` record. This avoids throwing exceptions for expected business failures (e.g., "not found", "duplicate name") and makes control flow explicit.

```csharp
// Usage in controller
var result = await _service.CreateTeamAsync(companyId, dto);
return result.Map(
    team => Ok(team),
    error => BadRequest(new { message = error.Message })
);
```

### Clean Architecture (3-Layer)
- **Core** — domain models, interfaces, DTOs, business logic. Zero infrastructure dependencies.
- **Infrastructure** — EF Core, repository implementations, gRPC client adapter.
- **WebApi** — controllers, gRPC server, DI wiring, middleware.

### Multi-Tenancy via JWT Claim
Every data operation is scoped by `CompanyId` extracted from the JWT token. The company ID is never trusted from the request body for mutations — only from the verified token.

### Repository Pattern
The `ITeamRepository` interface abstracts all data access. The service layer depends only on the interface, making unit testing straightforward with Moq.

### Dual-Port Kestrel
The service listens on two separate ports — one for HTTP/1 (REST) and one for HTTP/2 (gRPC) — avoiding protocol conflicts.

---

*RecoMind Project — TeamService*
