# 🧩 PlaneService - RecoMind

> Part of the **RecoMind** Project — ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **PlaneService** (Plan Service) is the microservice responsible for managing operational plans, objectives, and planning structures within the RecoMind platform. It supports adding, modifying, retrieving, and deleting plans associated with specific companies and teams. Furthermore, it manages customized statuses (e.g., Pending, In-Progress, Completed) and plan categories (Plan Types). It validates owner permissions internally and exposes gRPC endpoints so other microservices (like `CommentService`) can verify plan details and ownership during collaborative sessions.

### Main Responsibilities

- Perform full CRUD actions on team/corporate plans
- Associate plans with specific teams and companies based on authentication claims
- Validate user creation and modification permissions via a gRPC client calling the **TeamService**
- Manage plan attributes, including specific planning goals, timeline duration, starting/ending dates, approval flags, statuses, and types
- Expose a gRPC server providing helper queries like `GetPlan` and `isOwner` for other microservices (such as `CommentService`)
- Manage administrative categories like `PlanType` (Add, Delete, List) and `Status` (Add, Delete, List)

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Inter-Service Communication | gRPC (Server for internal query stubs; Client for TeamService context verification) |
| Object Mapping | Manual DTO mappings / AutoMapper profiles |
| Unit Testing | xUnit |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
PlaneService/
├── Core/                                  # Domain Layer
│   ├── DTOs/                              # Data Transfer Objects
│   │   ├── PlanDtos/
│   │   │   ├── AddPlanDto.cs
│   │   │   ├── GetPlanDto.cs
│   │   │   └── UpdatePlanDto.cs
│   │   └── PlnaTypeDtos/                  # Typo in source directory name
│   │       ├── AddPlantypeDto.cs
│   │       └── GetPlanTypeDto.cs
│   ├── Interfaces/                        # Service & Repo abstractions
│   │   ├── IGenericRepository.cs
│   │   ├── ITeamGrpcClient.cs             # Outbound gRPC client contract
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   └── PlanMapper.cs                  # Mapping domain plans to DTOs
│   ├── Models/                            # Domain entities
│   │   ├── JwtSettings.cs
│   │   ├── Plan.cs                        # Core Plan entity
│   │   ├── PlanType.cs                    # Plan Type entity
│   │   ├── Result.cs                      # Operation results wrapper
│   │   └── Status.cs                      # Status entity
│   ├── Service/                           # Business logic
│   │   ├── Interface/
│   │   │   ├── IPlanService.cs
│   │   │   ├── IPlanType.cs
│   │   │   └── IStatus.cs
│   │   ├── PlanService.cs
│   │   ├── PlanType.cs
│   │   └── Status.cs
│   └── Core.csproj
│
├── Infrastructure/                        # Data access & Infrastructure Layer
│   ├── Data/
│   │   └── PlanDbContext.cs               # EF Core DbContext for SQL Server
│   ├── GrpcClients/                       # Outbound client stubs
│   │   └── Team/
│   │       ├── TeamGrpcClientImpl.cs      # Calls TeamService via gRPC
│   │       └── TeamService.proto
│   ├── Migrations/                        # Database migrations
│   ├── Repository/
│   │   └── GenericRepo.cs                 # Generic Repository implementation
│   ├── UnitOfWork/
│   │   └── UOW.cs                         # Unit of Work implementation
│   └── Infrastructure.csproj
│
├── WebApi/                                # Presentation Layer
│   ├── Controllers/
│   │   └── PlanController.cs              # REST endpoints
│   ├── GrpcServer/                        # Inbound gRPC implementations
│   │   ├── PlanService.proto              # Proto definitions for PlanServiceGrpc
│   │   └── PlanServiceImpl.cs             # Implements GetPlan & isOwner RPCs
│   ├── Properties/
│   ├── appsettings.json                   # Configurations
│   ├── Program.cs                         # Application bootstrapper
│   └── WebApi.csproj
│
├── Test/                                  # Unit Tests Layer
│   ├── PlanServiceTests.cs
│   └── Tests.csproj
│
├── PlaneService.sln                       # Visual Studio Solution
└── Dockerfile                             # Containerization file
```

---

## 4. API Endpoints

> **Base Path:** `/api/Plan`
> All endpoints require standard `Bearer JWT` token authorization. Claims like `CompanyId` and `UserId` are automatically extracted.

### 4.1 Get All Plans

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Plan/GetAll` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Returns all plans matching the user's `CompanyId`. |

**Response Example (200 OK):**
```json
[
  {
    "id": "plan-7cc3f4c6",
    "goal": "Q2 Expansion",
    "description": "Expand cloud operations in EMEA.",
    "status": "In-Progress",
    "planType": "Strategic",
    "isApproved": true,
    "duration": "3 Months",
    "startDate": "2026-06-01T00:00:00Z",
    "endDate": "2026-08-31T00:00:00Z",
    "owner_Id": "user-uuid-1",
    "company_Id": "comp-uuid-9",
    "team_Id": "team-uuid-4"
  }
]
```

---

### 4.2 Get Plan by ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Plan/GetId/{id}` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Fetches a single plan by its ID, ensuring it belongs to the caller's company. |

---

### 4.3 Get Plans by Status

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Plan/GetByStatus/{status}` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Filters and lists plans belonging to the caller's company by their status string. |

---

### 4.4 Create Plan

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/Plan/CreatePlan` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Creates a new plan using the user's current `CompanyId` and `UserId`. |

**Request Body:**
```json
{
  "goal": "Onboard 5 clients",
  "description": "Successfully set up environments for new clients.",
  "status": "Pending",
  "planType": "Tactical",
  "duration": "1 Month",
  "startDate": "2026-05-20T00:00:00Z",
  "endDate": "2026-06-20T00:00:00Z",
  "team_Id": "team-uuid-4"
}
```

---

### 4.5 Update Plan

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/Plan/UpdatePlan` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Modifies details of an existing plan. Requires matching ownership or administrator rights. |

---

### 4.6 Delete Plan

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/Plan/Remove` |
| **Auth** | Bearer JWT Token Required |
| **Query Param**| `planId` |
| **Description** | Deletes a plan record by its ID, validating company bounds. |

---

### 4.7 Manage Plan Types & Statuses

- **Add Plan Type**: `POST /api/Plan/PlanType/Add`
- **Get All Plan Types**: `GET /api/Plan/PlanType/GetAll`
- **Delete Plan Type**: `DELETE /api/Plan/PlanType/Remove/{PlanTypeName}`
- **Get All Statuses**: `GET /api/Plan/Status/GetAll`
- **Add Status**: `POST /api/Plan/Status/Add/{Name}`
- **Delete Status**: `DELETE /api/Plan/Status/Remove/{Name}`

---

## 5. Database Design

### `Plan` Entity

| Property | Type | Constraints | Description |
|---|---|---|---|
| **Id** | `string` | Primary Key (GUID) | Unique identifier |
| **Goal** | `string` | Required | Core target of the plan |
| **Description** | `string` | Required | Detailed requirements |
| **Status** | `string` | Required | Current state (Pending, In-Progress, etc.) |
| **PlanType** | `string` | Required | Category type |
| **IsApproved** | `bool` | Required | Verification approval flag |
| **Duration** | `string` | Required | Plain-text duration description |
| **StartDate** | `DateTime` | Required | Plan timeline start date |
| **EndDate** | `DateTime` | Required | Plan timeline end date |
| **Owner_Id** | `string` | Required | Foreign reference to the creating User |
| **Company_Id** | `string` | Required | Foreign reference to the owning Company |
| **Team_Id** | `string` | Required | Foreign reference to the owning Team |

### `PlanType` Entity
- **Id** (`string`, PK)
- **Name** (`string`, Required)

### `Status` Entity
- **Id** (`string`, PK)
- **Name** (`string`, Required)

---

## 6. Authentication & Security

- **Claim validation**: Users can only interact with plans aligned with their `CompanyId` claim.
- **Role checks**: Administrative routines (like adding status codes and deleting plan types) enforce strict administrative controls.

---

## 7. Inter-Service Communication

### gRPC Client (Outbound)
- Calls `TeamService` via `TeamService.proto` on channel `http://team-svc:5010` to query details regarding the caller's team context:
  ```protobuf
  rpc GetTeamInformation (GetTeamInformationRequest) returns (GetTeamInformationResponse);
  ```

### gRPC Server (Inbound)
- Exposes `PlanServiceGrpc` to internal clients (such as `CommentService`) so they can confirm if a resource exists or if a specific user owns a plan:
  - `GetPlan(PlanRequest)` -> Returns metadata like plan IDs, team IDs, owner details, etc.
  - `isOwner(isOwnerRequest)` -> Returns boolean confirmation of ownership.

---

## 8. Configuration Reference

```json
{
  "GrpcSettings": {
    "TeamServiceUrl": "http://team-svc:5010"
  }
}
```

---

## 9. Architecture Notes

- **UOW and Repository Implementation**: Standard patterns isolating core transactions.
- **Fail-Safe Returns**: Leverages a robust wrapper model `Result` containing execution states and errors to prevent application halts.

---

## 10. Running the Service

```bash
# Navigate to webApi folder and run:
dotnet run
```

---

## 11. Testing

Unit tests reside in the `Test` folder.

```bash
# Run unit tests:
dotnet test
```
