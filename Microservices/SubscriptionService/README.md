# 🧩 SubscriptionService - RecoMind

> Part of the **RecoMind** Project — ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **SubscriptionService** is a foundational microservice within the RecoMind ecosystem responsible for managing corporate subscriptions and pricing plans. It defines and handles plan types (such as Basic, Premium, and Enterprise) and handles active company subscriptions, tracking critical fields like billing cycle (Monthly, Annually), pricing tiers, activation dates, expiration dates, and overall status. It exposes secure administrative REST APIs and serves internal microservices (like `CompanyService`) via gRPC to validate client tiers and process registration limits.

### Main Responsibilities

- Define and manage available subscription types (tiers) and prices
- Authorize and manage active company subscriptions
- Track active and expired subscriptions, pricing records, and billing cycles (e.g., Monthly, Yearly)
- Expose a robust gRPC server so internal microservices (such as `CompanyService`) can securely validate subscription IDs and query active features
- Restrict endpoint manipulations solely to platform administrators (role-based security)

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens (Role-Based Authorization) |
| Inter-Service Communication | gRPC Server |
| Object Mapping | Manual DTO mappings / AutoMapper profiles |
| Unit Testing | xUnit |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
SubscriptionService/
├── Core/                                  # Domain Layer
│   ├── Consts/
│   │   └── BillingCycle.cs                # Enum: Monthly, Annually
│   ├── DTOs/                              # Data Transfer Objects
│   │   ├── SubscriptionTypeDto/
│   │   │   ├── CreateDto.cs
│   │   │   ├── DeleteDto.cs
│   │   │   └── GetDto.cs
│   │   ├── CreateSubscriptionCompanyDto.cs
│   │   ├── DeleteSubscriptionCompanyDto.cs
│   │   ├── GetSubscriptionCompanyDto.cs
│   │   └── UpdateSubscriptionCompanyDto.cs
│   ├── Interfaces/                        # Service & Repo abstractions
│   │   ├── IGenericRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   ├── MappingForGrpc.cs              # Converts entities to Protobuf responses
│   │   ├── SubscriptionCompanyMapping.cs  # DTO to Entity mapping
│   │   └── SubscriptionTypeMapping.cs     # Type DTO mapping
│   ├── Models/                            # Domain entities
│   │   ├── JwtSettings.cs
│   │   └── Subscription.cs                # SubscriptionCompany & SubscriptionType models
│   ├── Service/                           # Business logic
│   │   ├── Interface/
│   │   │   ├── ISubscriptionCompanyService.cs
│   │   │   └── ISubscriptionTypeService.cs
│   │   ├── Protos/
│   │   │   └── SubscriptionService.proto  # gRPC Proto definitions
│   │   ├── SubscriptionCompanyService.cs
│   │   └── SubscriptionTypeSevice.cs      # Typo in source filename
│   └── Core.csproj
│
├── Infrastructure/                        # Data access & Infrastructure Layer
│   ├── Data/
│   │   └── SubscriptionDbContext.cs       # EF Core DbContext for SQL Server
│   ├── Migrations/                        # Database migrations
│   ├── Repository/
│   │   └── GenericRepo.cs                 # Generic Repository implementation
│   ├── UnitOfWork/
│   │   └── UnitOfWork.cs                  # Unit of Work implementation
│   └── Infrastructure.csproj
│
├── WebApi/                                # Presentation Layer
│   ├── Controllers/
│   │   └── SubscriptionController.cs      # REST endpoints
│   ├── Grpc/                              # gRPC service & handler implementations
│   │   ├── GrpcExceptionHandler.cs        # Exception middleware for RPC routines
│   │   └── SubscriptionGrpcService.cs     # Implements subscriptionService RPCs
│   ├── Properties/
│   ├── appsettings.json                   # Configurations
│   ├── Program.cs                         # Application bootstrapper
│   └── WebApi.csproj
│
├── Tests/                                 # Unit Tests Layer
│   ├── SubscriptionCompanyServiceTests.cs
│   ├── SubscriptionTypeServiceTests.cs
│   └── Tests.csproj
│
├── SubscriptionService.sln                # Visual Studio Solution
└── Dockerfile                             # Containerization file
```

---

## 4. API Endpoints

> **Base Path:** `/api/Subscription`
> All REST API endpoints require standard `Bearer JWT` token authorization, constrained to users in the **`admin`** role.

### 4.1 Get All Active Corporate Subscriptions

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Subscription/GetAll` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Returns a list of all active or registered corporate subscriptions. |

**Response Example (200 OK):**
```json
[
  {
    "id": "sub-uuid-abcde",
    "price": 299.99,
    "startDate": "2026-05-20T00:00:00Z",
    "endDate": "2027-05-20T00:00:00Z",
    "isActive": true,
    "billingCycle": "Annually",
    "subscriptionTypeId": "type-premium"
  }
]
```

---

### 4.2 Get Subscription by ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Subscription/GetById/{id}` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Fetches a subscription record matching the provided ID. |

---

### 4.3 Manage Billing Cycles & Types

- **Get All Billing Cycles**: `GET /api/Subscription/GetAllBillingCycles`
  - Returns `["Monthly", "Annually"]`
- **Get All Subscription Plans**: `GET /api/Subscription/AllSubscriptionType`
  - Returns list of `SubscriptionType` definitions (PlanName, Price, etc.)
- **Create Subscription Type**: `POST /api/Subscription/CreateSubscriptionType`
  - Body: `{ "planName": "PremiumPlus", "price": 499.00 }`
- **Update Subscription Type**: `PUT /api/Subscription/UpdateSubscriptionType/{OldPlanTypeName}`
- **Delete Subscription Type**: `DELETE /api/Subscription/SubscriptionType/{PlanName}`

---

### 4.4 Create Corporate Subscription

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/Subscription/Create` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Registers a new subscription tier package for a company context. |

**Request Body:**
```json
{
  "price": 99.99,
  "startDate": "2026-05-20T00:00:00Z",
  "endDate": "2026-06-20T00:00:00Z",
  "isActive": true,
  "billingCycle": 0,
  "subscriptionTypeId": "type-basic"
}
```

---

### 4.5 Update Subscription

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/Subscription/Update/{id}` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Updates the subscription settings (expiration dates, prices, status) of an active subscriber. |

---

### 4.6 Delete Subscription

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/Subscription/Delete/{id}` |
| **Auth** | Bearer JWT Token (`admin` role required) |
| **Description** | Deletes a subscription record from the registry database. |

---

## 5. Database Design

### `SubscriptionCompany` Entity

Matches corporate subscription assignments.

| Property | Type | Constraints | Description |
|---|---|---|---|
| **Id** | `string` | Primary Key (GUID) | Unique identifier |
| **Price** | `double` | Required | Paid amount |
| **StartDate** | `DateTime` | Required | Start date of active service |
| **EndDate** | `DateTime` | Required | Expiration date of active service |
| **IsActive** | `bool` | Required | Flag indicating validity |
| **BillingCycle** | `BillingCycle` | Required | Enum representing billing term (Monthly/Annually) |
| **SubscriptionTypeId**| `string` | Foreign Key | References `SubscriptionType` |

### `SubscriptionType` Entity

Defines the template plan models.

- **SubscriptionTypeId** (`string`, PK)
- **PlanName** (`string`, Required)
- **Price** (`double`, Required)

---

## 6. Authentication & Security

- **Restricted Access**: The entire controller is annotated with `[Authorize(Roles = "admin")]` to block non-administrative operations.

---

## 7. Inter-Service Communication (gRPC Server)

Exposes a gRPC service defined by `SubscriptionService.proto` on internal channels to allow microservices like `CompanyService` to validate registration details.

### Exposed Endpoints
- `create(createSubscriptionRequest)` -> `subscriptionResponse`
- `getById(getByIdRequest)` -> `subscriptionResponse`
- `getAll(empty)` -> `getAllSubscriptionResponse`
- `update(updateSubscriptionRequest)` -> `subscriptionResponse`
- `delete(getByIdRequest)` -> `deleteSubscriptionResponse`

---

## 8. Architecture Notes

- **GrpcExceptionHandler**: Implements RPC interceptors to safely catch and format service-level exceptions into RPC status codes (e.g., NOT_FOUND, INVALID_ARGUMENT).
- **AutoMapper and Custom Mappers**: Ensures clean data conversion flows.

---

## 9. Running the Service

```bash
# Navigate to WebApi folder and run:
dotnet run
```

---

## 10. Testing

Tests are written using **xUnit** framework.

```bash
# Run unit tests:
dotnet test
```
