# 🗂️ MappingService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **MappingService** manages the assignment of database tables to company departments (teams). In RecoMind's AI workflow, the AI agent needs to know which database tables are relevant to a given department before it can generate queries. This service is the bridge between the company's schema and its organizational structure.

### Main Responsibilities

- Track which database tables belong to which department within a company
- Provide a list of **available (unassigned)** tables for a department to choose from
- Provide a list of **assigned** tables for a department to review
- Allow managers to **add** tables to a department
- Allow managers to **remove** tables from a department

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | **PostgreSQL** (via Npgsql + Entity Framework Core 8) |
| ORM | Entity Framework Core 8 + Npgsql Provider |
| Authentication | JWT Bearer Tokens |
| Object Mapping | AutoMapper 13 |
| Unit Testing | xUnit, Moq, FluentAssertions |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

> **Note:** This is the only service in the project that uses **PostgreSQL** instead of SQL Server. The `client_schema_vectors` table appears to be shared with a vector/AI database layer (note the `jsonb` column for table relations).

---

## 3. Project Structure

```
MappingService/
├── Data-Mapping.Core/                    # Domain layer
│   ├── DTOs/
│   │   ├── MappingRequestDto.cs          # Request for add/remove operations
│   │   └── TableSummaryDto.cs            # Lightweight table info (id, name, description)
│   ├── Interfaces/
│   │   ├── IMappingRepository.cs         # Repository contract
│   │   └── IMappingService.cs            # Service contract
│   ├── Mapper/
│   │   └── MappingProfile.cs             # AutoMapper: ClientSchemaVector → TableSummaryDto
│   ├── Models/
│   │   └── ClientSchemaVector.cs         # Domain entity (maps to DB table)
│   └── Services/
│       └── MappingService.cs             # Business logic
│
├── Data-Mapping.Infrastructure/          # Data access layer
│   ├── Data/
│   │   └── MappingDbContext.cs           # EF Core DbContext (PostgreSQL)
│   └── Repositories/
│       └── MappingRepository.cs          # Repository implementation
│
├── Data-Mapping.WebApi/                  # Presentation layer
│   ├── Controllers/
│   │   └── MappingController.cs         # REST API endpoints
│   ├── Program.cs
│   └── appsettings.json
│
├── Data-Mapping.Tests/                   # Unit tests
│   └── Services/
│       └── MappingServiceTests.cs
│
├── Dockerfile
└── MappingService.sln
```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api/mapping`
> All endpoints require a valid **JWT Bearer Token** and the `Management` policy (`admin` or `manager` role).

---

### 4.1 Get Available Tables for a Department

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/mapping/available/{deptName}` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Returns all tables for the authenticated company that are **not yet assigned** to the given department. Tables assigned to other departments are still shown as available. |

**Response Example (200 OK):**
```json
[
  {
    "id": 1,
    "name": "orders",
    "description": "Contains all customer orders"
  },
  {
    "id": 3,
    "name": "invoices",
    "description": "Invoice records"
  }
]
```

**Response (400 Bad Request):**
```json
"Company ID and Department Name cannot be null or empty."
```

---

### 4.2 Review Tables Assigned to a Department

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/mapping/review/{deptName}` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Returns all tables currently assigned to the given department. |

**Response Example (200 OK):**
```json
[
  {
    "id": 2,
    "name": "employees",
    "description": "HR employee records"
  }
]
```

**Response (200 OK — no tables found):**
```json
{
  "message": "No tables found for this department",
  "data": []
}
```

---

### 4.3 Add Tables to a Department

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/mapping/add` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Assigns one or more tables to a department. Tables already assigned to the department are silently skipped (no duplication). Non-existent table IDs are also silently skipped. |

**Request Body:**
```json
{
  "companyId": "company-uuid",
  "deptName": "HR",
  "tableIds": [1, 3, 5]
}
```

**Response Example (200 OK):**
```json
{ "success": true }
```

**Response (400 Bad Request):** HTTP 400

---

### 4.4 Remove Tables from a Department

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/mapping/delete` |
| **Auth** | Required — Roles: `admin`, `manager` |
| **Description** | Removes one or more tables from a department. Tables not assigned to the department or non-existent IDs are silently skipped. |

**Request Body:**
```json
{
  "companyId": "company-uuid",
  "deptName": "HR",
  "tableIds": [1, 3]
}
```

**Response Example (200 OK):**
```json
{ "success": true }
```

---

## 5. Database Design

### Table: `client_schema_vectors`

> This table lives in a **PostgreSQL** database. The naming convention (`snake_case`, `jsonb`) suggests it may be shared with or populated by a vector/AI pipeline.

| Column | DB Type | Notes |
|---|---|---|
| `id` | `integer` (serial) | PK |
| `company_id` | `varchar(255)` | Tenant identifier |
| `table_name` | `varchar(255)` | Name of the client DB table |
| `table_description` | `text` | Optional human-readable description |
| `table_relations` | `jsonb` | Optional JSON describing table relationships |
| `team_name` | `text[]` | Array of department names assigned to this table |

### Unique Constraint
```sql
UNIQUE (company_id, table_name)  -- enforced as uk_company_table
```

### How Assignment Works

The `team_name` column is a **PostgreSQL text array**. Adding a department appends its name to the array; removing it takes it out. This allows a single table to be shared across multiple departments simultaneously.

```
company_id = "comp-1"
table_name = "orders"
team_name  = ["HR", "Finance"]   ← assigned to two departments
```

---

## 6. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token |
| **Claim used** | `CompanyId` — extracted from JWT token to scope queries |
| **HTTPS** | Required |

### Authorization Policies

| Policy | Allowed Roles |
|---|---|
| `Management` | `admin`, `manager` |
| `Admin` | `admin` |

> All four API endpoints use the `Management` policy. The `companyId` used in queries comes from the authenticated user's JWT claim, not from the request body — except for Add/Remove operations where it is explicitly provided in the body (`MappingRequestDto`).

---

## 7. Service Communication

The MappingService operates **independently** — it does not communicate with other microservices via REST or messaging. It only exposes HTTP endpoints and reads from its own PostgreSQL database.

Other services (e.g., the AI service) call this service's REST API to retrieve table-to-department mappings before generating queries.

| Communication Type | Direction |
|---|---|
| REST (HTTP) | Inbound from API Gateway / AI Service |
| Database (PostgreSQL) | Internal only |

---

## 8. Architecture Notes

### Exception-Based Validation (vs. Result Pattern)
Unlike other services in this project, `MappingService` uses `ArgumentException` for invalid inputs (null/empty company or department names) rather than the `Result<T>` pattern. The controller catches these and returns `400 Bad Request`.

### Silent Skip Semantics
The Add and Remove operations intentionally skip records silently:
- **Add:** If a table ID doesn't exist or is already assigned → skip, continue.
- **Remove:** If a table ID doesn't exist or isn't assigned to the dept → skip, continue.

This makes bulk operations idempotent and prevents partial-failure scenarios.

### Array-Based Department Assignment
Storing department assignments as a PostgreSQL `text[]` array on the table record (rather than a separate join table) is an unconventional design. It works well for read-heavy, low-cardinality scenarios (few departments per table) and avoids extra joins, but makes querying by department require array-contains operators.

### Clean Architecture (3-Layer)
- **Core** — interfaces, models, service logic, DTOs, AutoMapper profiles.
- **Infrastructure** — EF Core + Npgsql, repository implementation.
- **WebApi** — controllers, DI setup.

### Dual-Port Kestrel
The service configures both an HTTP/1 and HTTP/2 port via Kestrel, though no gRPC services are currently registered. The gRPC port configuration appears to be a shared template across all services in this project.

---

*RecoMind Project — MappingService*
