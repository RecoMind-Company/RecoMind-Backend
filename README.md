# RecoMind — Backend Architecture

> ASP.NET Core Microservices · C# .NET 8 · SQL Server · PostgreSQL · gRPC · RabbitMQ

---

## 1. Backend Overview

RecoMind is an AI-powered business intelligence platform that allows companies to query their own databases using natural language. The backend is built as a distributed microservices system, where each service owns a specific business domain and communicates with others through gRPC or asynchronous messaging.

The system supports multi-tenant company structures with role-based access, subscription management, team organization, real-time notifications, and an AI chatbot layer that generates SQL queries from user questions.

---

## 2. Architecture Overview

```
                          ┌─────────────────────┐
                          │     API Gateway      │
                          │  (Routing / Auth)    │
                          └────────┬────────────┘
                                   │ REST
          ┌────────────────────────┼─────────────────────────┐
          │                        │                          │
   ┌──────▼──────┐         ┌───────▼──────┐         ┌───────▼──────┐
   │    Auth     │         │   Company    │         │     Team     │
   │   Service   │         │   Service   │         │   Service    │
   └──────┬──────┘         └───────┬──────┘         └───────┬──────┘
          │ gRPC                   │ gRPC                    │ gRPC
          │              ┌─────────▼──────────┐             │
          │              │  Subscription Svc  │             │
          │              └────────────────────┘             │
          │                                                  │
   ┌──────▼──────────────────────────────────────────────────▼──────┐
   │               Internal gRPC Communication Layer                 │
   └───────┬──────────────┬────────────────┬───────────────┬────────┘
           │              │                │               │
   ┌───────▼───┐   ┌──────▼─────┐  ┌──────▼──────┐  ┌────▼──────────┐
   │  Chatbot  │   │   Mapping  │  │  DbSetting  │  │  Notification │
   │  Service  │   │  Service   │  │   Service   │  │    Service    │
   └───────┬───┘   └────────────┘  └─────────────┘  └────▲──────────┘
           │ HTTP                                         │ RabbitMQ
   ┌───────▼───────────┐                    ┌────────────┴──────────┐
   │   AI Copilot Svc  │                    │  Other Services (pub) │
   │  (External HTTP)  │                    │  Plan, Task, Comment  │
   └───────────────────┘                    └───────────────────────┘
```

**Key design principles:**
- Each service owns its own database — no shared data stores
- Inter-service calls use gRPC for synchronous queries
- Notifications are delivered asynchronously via RabbitMQ + MassTransit
- All public endpoints sit behind JWT-based role authorization
- The AI chatbot fetches team context, queries the AI Copilot service, and stores results locally

---

## 3. Technology Stack

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Primary Database | SQL Server (via Entity Framework Core 8) |
| Secondary Database | PostgreSQL (MappingService only) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens (Role-Based) |
| Inter-Service (Sync) | gRPC (Protobuf) |
| Inter-Service (Async) | RabbitMQ via MassTransit 8 |
| Real-Time (Web) | ASP.NET Core SignalR |
| Push Notifications | Firebase Admin SDK (FCM) |
| Object Mapping | AutoMapper 12/13 |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Unit Testing | xUnit, Moq, FluentAssertions |
| Containerization | Docker |
| Encryption | AES-256 (System.Security.Cryptography) |

---

## 4. Microservices Overview

| Service | Responsibility | DB | gRPC Role |
|---|---|---|---|
| **AuthenticationService** | User registration, login, JWT issuance, role management | SQL Server | Server (exposes user/job-title queries) |
| **CompanyService** | Company CRUD, subscription assignment | SQL Server | Server + Client (calls SubscriptionService) |
| **SubscriptionService** | Subscription plans and billing cycle management | SQL Server | Server (validates subscriptions for CompanyService) |
| **TeamService** | Team CRUD, employee membership | SQL Server | Server + Client (calls AuthService) |
| **DatabaseSettingService** | Stores AES-256 encrypted DB credentials per company | SQL Server | Server (exposes safe metadata) |
| **MappingService** | Maps database tables to company departments | PostgreSQL | None (REST only) |
| **InvitationService** | Manages employee invitations to companies | SQL Server | — |
| **PlaneService** | Operational plans and planning structures | SQL Server | Server + Client (calls TeamService) |
| **TaskService** | Tasks linked to plans and employees | SQL Server | — |
| **CommentService** | Comments on plans/tasks | SQL Server | Client (calls PlaneService) |
| **NotificationService** | Real-time + push notifications, history | SQL Server | None (RabbitMQ consumer) |
| **ReportService** | Business report generation | SQL Server | — |
| **chatbotservice** | AI chatbot: NL → SQL, chat history | SQL Server | Client (calls TeamService) |

> Each service has its own README with full endpoint details, DTOs, and database schema.

---

## 5. Service Communication

### Synchronous — gRPC

Internal service-to-service queries use gRPC over HTTP/2. Each service that exposes gRPC runs it on a dedicated port (separate from the REST port) configured via a `GRPC_PORT` environment variable.

**Key gRPC channels:**

| Caller | Callee | Purpose |
|---|---|---|
| CompanyService | SubscriptionService | Validate subscription ID before creating/updating a company |
| TeamService | AuthService | Fetch job titles for team members |
| PlaneService | TeamService | Verify team context before plan operations |
| chatbotservice | TeamService | Fetch company ID and team name for AI context |
| CommentService | PlaneService | Verify plan existence and ownership |
| AuthService, TeamService, PlaneService | CompanyService | Fetch company records / validate relations |

### Asynchronous — RabbitMQ

**NotificationService** is the sole consumer. Other services (Plan, Task, Comment, etc.) publish `NotificationEventDto` messages to the `notification-queue`. MassTransit handles retry, deserialization, and DI-friendly consumer registration.

```
Publisher (any service)  →  RabbitMQ (notification-queue)  →  NotificationConsumer
                                                                   ↓
                                                           Save to DB + SignalR + FCM
```

### Real-Time — SignalR

NotificationService maintains a SignalR hub at `/hubs/notifications`. Each user joins a group named by their `userId`. JWT is passed via query string for WebSocket compatibility.

---

## 6. Authentication & Security

- **Mechanism:** JWT Bearer Tokens issued by `AuthenticationService`
- **Claims used across services:** `sub` (UserId), `CompanyId`, `role`
- **Roles:** `admin`, `manager`, `teamleader`, `employee`
- **Multi-tenancy:** All data operations are scoped by `CompanyId` extracted from the verified JWT — never from the request body
- **Password encryption:** DB credentials in `DatabaseSettingService` are encrypted at rest with AES-256 (CBC mode)
- **Clock skew:** Zero tolerance enforced across services
- **HTTPS:** Required on all services

### Common Authorization Policies

| Policy | Roles |
|---|---|
| `AllEmployees` | admin, manager, teamleader, employee |
| `Management` | admin, manager |
| `Leadership` / `TeamLeadership` | admin, manager, teamleader |
| `Admin` | admin |

---

## 7. Database Strategy

- **Isolation:** Each service maintains its own database and runs its own EF Core migrations independently
- **No shared schemas** — cross-service data is fetched via gRPC, not DB joins
- **SQL Server** is the default for all services except `MappingService`, which uses **PostgreSQL** (for `jsonb` and array column support)
- **Repository + Unit of Work** pattern is used consistently across services for transactional consistency
- **Result pattern** (`Result<T>`) replaces exception-driven flow for expected business errors (not found, duplicates, validation failures) in services that follow Clean Architecture strictly

### Notable Design Decisions

| Service | Decision |
|---|---|
| DatabaseSettingService | One-to-one company→credentials, enforced at service layer; a DB-level unique index on `CompanyId` is a known improvement |
| MappingService | Department assignments stored as a PostgreSQL `text[]` array on the table record rather than a join table |
| NotificationService | FCM failures are caught and logged — DB save + SignalR delivery define overall success |

---

## 8. Running the Project

### Prerequisites

- .NET 8 SDK
- Docker & Docker Compose
- SQL Server instance (or use the Docker Compose setup)
- PostgreSQL instance (for MappingService)
- RabbitMQ instance
- Firebase project credentials (for push notifications)

### Running a Single Service

```bash
cd Microservices/<ServiceName>/webApi   # or WebApi / WebAPI depending on the service
dotnet run
```

### Running Tests

```bash
cd Microservices/<ServiceName>/Tests    # or Test
dotnet test
```

### Running with Docker

Each service includes a `Dockerfile`. Use Docker Compose (at the repository root) to spin up all services with their dependencies:

```bash
docker-compose up --build
```

### Environment Variables

Each service reads its configuration from `appsettings.json` and environment variable overrides. Key variables:

| Variable | Used By | Purpose |
|---|---|---|
| `GRPC_PORT` | All gRPC servers | gRPC listener port |
| `HTTP_PORT` | All services | REST listener port |
| `ConnectionStrings__DefaultConnection` | All services | Database connection |
| `GrpcSettings__*ServiceUrl` | gRPC clients | Target service URL |
| `JwtSettings__*` | All services | JWT validation config |
| `Encryption__Key` / `Encryption__IV` | DatabaseSettingService | AES-256 key material |
| `Firebase__*` | NotificationService | FCM credentials |
| `RabbitMQ__Host` | NotificationService + publishers | Message broker |
| `AIService__BaseUrl` | chatbotservice | External AI Copilot URL |

---

## 9. API Documentation

All services expose Swagger UI at `/swagger` when running in development mode.

**Production base URL:** `https://api.recomind.site`

| Service | Base Path |
|---|---|
| CompanyService | `/api/Companies` |
| SubscriptionService | `/api/Subscription` |
| TeamService | `/api/team` |
| DatabaseSettingService | `/api/dbsetting` |
| MappingService | `/api/mapping` |
| PlaneService | `/api/Plan` |
| NotificationService | `/api/notifications` |
| chatbotservice | `/api/Chatbot` |

> Refer to each service's individual README for full endpoint specifications, request/response shapes, and error codes.

---

## 10. Folder Structure

```
RecoMind-Backend/
├── Microservices/
│   ├── AuthenticationService/
│   ├── CompanyService/
│   ├── SubscriptionService/
│   ├── TeamService/
│   ├── DatabaseSettingService/
│   ├── MappingService/
│   ├── InvitationService/
│   ├── PlaneService/
│   ├── TaskService/
│   ├── CommentService/
│   ├── NotificationService/
│   ├── ReportService/
│   └── chatbotservice/
├── docker-compose.yml
└── README.md                  ← this file
```

Each service follows a consistent 3-layer Clean Architecture structure:

```
<ServiceName>/
├── <Service>.Core/            # Domain: entities, interfaces, DTOs, business logic
├── <Service>.Infrastructure/  # Data: EF Core, repositories, gRPC clients, messaging
├── <Service>.WebApi/          # Presentation: controllers, gRPC server, DI wiring
├── <Service>.Tests/           # Unit tests
└── Dockerfile
```

---

## 11. Deployment / Infrastructure Overview

- Every service is containerized via **Docker**
- Services are isolated by port and communicate over an internal Docker network
- **gRPC ports** are separate from **HTTP ports** to avoid HTTP/1 vs HTTP/2 conflicts (Kestrel dual-port setup)
- The `DatabaseSettingService` AI endpoint (`GET /api/dbsetting/company/{id}`) is public by design — it is intended for internal consumption only and should be protected at the network/gateway level in production
- **RabbitMQ** and database instances are expected as external infrastructure (managed services or dedicated containers)

---

## 12. Future Improvements

- Add a **database-level unique constraint** on `CompanyId` in `DatabaseSettingService` to prevent race-condition duplicates
- **Secure the AI-facing DB credentials endpoint** at the API Gateway level rather than relying on network trust
- Introduce a **distributed tracing** solution (e.g., OpenTelemetry + Jaeger) for cross-service request correlation
- Add an **API Gateway** (e.g., YARP or Ocelot) for unified routing, rate limiting, and centralized auth validation
- Replace `MappingService`'s `text[]` array with a normalized join table for better queryability at scale
- Implement **health check endpoints** (`/health`) across all services for orchestration readiness probes
- Add **integration tests** alongside existing unit tests to cover gRPC and messaging flows

---

*RecoMind Project — Backend Architecture*