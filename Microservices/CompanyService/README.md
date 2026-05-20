# 🧩 CompanyService - RecoMind

> Part of the **RecoMind** Project — ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **CompanyService** is the core microservice responsible for managing companies within the RecoMind platform. It supports adding, updating, retrieving, and deleting company profile information (including industry, country, size, access codes, and subscriptions). It serves as a central domain model, communicating with `SubscriptionService` via gRPC to validate and assign subscription plans, and exposing secure REST and gRPC interfaces for administrative workflows.

### Main Responsibilities

- Manage CRUD operations on company entities
- Maintain company metadata (e.g., industry, country, size, registration code, and descriptions)
- Coordinate with **SubscriptionService** via gRPC clients to validate subscription IDs before creating/updating companies or assigning plans
- Expose a full gRPC server so other microservices (like `AuthenticationService`, `TeamService`, and `PlaneService`) can fetch company records or map relations internally
- Enforce strict authorization rules allowing only users in the `admin` role to manipulate company resources

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens (Role-Based Authorization) |
| Inter-Service Communication | gRPC (Server for internal services; Client for SubscriptionService) |
| Object Mapping | Manual DTO mappings / AutoMapper |
| Unit Testing | xUnit (infrastructure ready) |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
CompanyService/
├── Core/                                  # Domain Layer
│   ├── DTOs/                              # Data Transfer Objects
│   │   ├── AssignSubscriptionDto.cs
│   │   ├── CreateCompanyDTO.cs
│   │   ├── DeleteCompanyDTO.cs
│   │   ├── GetCompanyDTO.cs
│   │   └── UpdateCompanyDTO.cs
│   ├── Interfaces/                        # Service & Repo abstractions
│   │   ├── IGenericRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   ├── CompanyMapping.cs              # Manual DTO mapping logic
│   │   └── MappingForRpc.cs               # Mapping to gRPC Protobuf messages
│   ├── Models/                            # Domain entities
│   │   ├── Company.cs                     # Core Company entity
│   │   └── JwtSettings.cs
│   ├── Service/                           # Core service business logic
│   │   ├── Interface/
│   │   │   └── ICompanyService.cs
│   │   ├── Protos/
│   │   │   └── GrpcCompanyService.proto   # gRPC Proto service definitions
│   │   └── CompanyService.cs
│   └── Core.csproj
│
├── Infrastructure/                        # Data access & Infrastructure Layer
│   ├── Data/
│   │   └── CompanyDbContext.cs            # EF Core DbContext for SQL Server
│   ├── Migrations/                        # Database migrations
│   ├── Repository/
│   │   └── GenericRepo.cs                 # Generic Repository implementation
│   ├── UnitOfWork/
│   │   └── unitOfWork.cs                  # Unit of Work implementation
│   └── Infrastructure.csproj
│
├── webApi/                                # Presentation Layer
│   ├── Controllers/
│   │   └── CompanyController.cs           # REST endpoints
│   ├── Grpc/                              # Generated gRPC stubs & server implementation
│   │   └── GrpcImplementations/
│   │       └── CompanyServiceImpl.cs      # gRPC service implementation
│   ├── Properties/
│   ├── Protos/
│   │   └── SubscriptionService.proto      # Client stub proto for SubscriptionService
│   ├── appsettings.json                   # Configurations
│   ├── Program.cs                         # Application bootstrapper
│   └── webApi.csproj
│
├── Tests/                                 # Unit Tests Layer
│   ├── CompanyServiceTests.cs
│   └── Tests.csproj
│
├── CompanyService.sln                     # Visual Studio Solution
└── Dockerfile                             # Containerization file
```

---

## 4. API Endpoints

> **Base Path:** `/api/Companies`
> All REST API endpoints require standard `Bearer JWT` token authorization, and most are constrained to users in the **`admin`** role.

### 4.1 Get All Companies

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Companies/getAll` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Returns a list of all companies registered in the platform. |

**Response Example (200 OK):**
```json
[
  {
    "id": "comp-7cc3f4c6-e91b",
    "name": "Acme Corp",
    "industry": "Software & Tech",
    "country": "Egypt",
    "size": "50-100",
    "code": "ACME77",
    "description": "Tech Solutions Provider",
    "subscriptionId": "sub-91b4f76",
    "createdAt": "2026-05-20T19:35:00Z"
  }
]
```

---

### 4.2 Get Company by Admin ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Companies/GetByAdminId` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Extracts the `Admin ID` claim from the JWT token and returns all companies associated with that administrator. |

**Response Example (200 OK):**
```json
[
  {
    "id": "comp-7cc3f4c6-e91b",
    "name": "Acme Corp",
    "industry": "Software & Tech",
    "country": "Egypt",
    "size": "50-100",
    "code": "ACME77",
    "description": "Tech Solutions Provider",
    "subscriptionId": "sub-91b4f76",
    "createdAt": "2026-05-20T19:35:00Z"
  }
]
```

---

### 4.3 Get Company by ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Companies/GetByCompanyId/{id}` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Returns a specific company matching the provided ID. |

---

### 4.4 Create Company

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/Companies/create` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Creates a new company. If a `SubscriptionId` is provided, the service will validate it against the `SubscriptionService` via gRPC before saving. |

**Request Body:**
```json
{
  "name": "Beta Tech",
  "industry": "Consulting",
  "country": "UAE",
  "size": "10-50",
  "code": "BETA88",
  "description": "Premium Consulting",
  "subscriptionId": "sub-uuid-12345"
}
```

---

### 4.5 Update Company

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/Companies/Update/{id}` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Updates the company details for the given ID. |

---

### 4.6 Assign Subscription

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/Companies/AssignSubscription` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Explicitly assigns or upgrades a company's subscription plan. Calls `SubscriptionService` via gRPC client to verify the subscription ID. |

**Request Body:**
```json
{
  "companyId": "comp-7cc3f4c6-e91b",
  "subscriptionId": "sub-91b4f76"
}
```

---

### 4.7 Delete Company

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/Companies/Delete/{id}` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Deletes a company by its ID. |

---

## 5. Database Design

### `Company` Entity

| Property | Type | Constraints | Description |
|---|---|---|---|
| **Id** | `string` | Primary Key (GUID) | Unique identifier for the company |
| **Name** | `string` | Required | Corporate name |
| **Industry** | `string` | Required | Industry category |
| **Country** | `string` | Nullable | Headquarter country |
| **Size** | `string` | Nullable | Employee bracket (e.g., 50-100) |
| **Code** | `string` | Nullable | Unique registration / join code |
| **Description** | `string` | Nullable | Brief about description |
| **SubscriptionId**| `string` | Nullable | References a subscription (validated externally) |
| **AdminId** | `string` | Required | Foreign identity reference of the managing Admin |
| **CreatedAt** | `DateTime` | Nullable | Date of record creation |

---

## 6. Authentication & Security

- **Role-Based Authorization**: All REST endpoints are marked with `[Authorize(Roles = "admin")]` to ensure access is restricted to system administrators.
- **Security Claims**: Admin identity is retrieved directly from the token principal's claims to guarantee secure audit trails.

---

## 7. Inter-Service Communication

### gRPC Client (Outbound)
- Calls `SubscriptionService` via `SubscriptionService.proto` on channel `http://subscription-svc:5004` to validate subscription status:
  ```protobuf
  rpc getById (getByIdRequest) returns (SubscriptionResponse);
  ```

### gRPC Server (Inbound)
- Exposes `GrpcCompanyService.proto` to allow other internal microservices to fetch records and validate company details using the following endpoints:
  - `GetById`
  - `GetAll`
  - `Create`
  - `Update`
  - `Delete`
  - `GetCompanyByAdminId`
  - `AssignSubscripion`

---

## 8. Configuration Reference

```json
{
  "GrpcSettings": {
    "SubscriptionServiceUrl": "http://subscription-svc:5004"
  }
}
```

---

## 9. Architecture Notes

- **Separation of Concerns**: Splitting gRPC mapping (`MappingForRpc.cs`) from internal mapping profiles (`CompanyMapping.cs`) ensures decoupling.
- **Transactional Consistency**: Relies on `IUnitOfWork` to save edits or updates sequentially.

---

## 10. Running the Service

```bash
# Navigate to webApi folder and run:
dotnet run
```

---

## 11. Testing

Unit tests are written using **xUnit** and reside in the `Tests` project.

```bash
# Run unit tests:
dotnet test
```
