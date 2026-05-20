# ?? ReportService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **ReportService** is responsible for generating, storing, retrieving, and managing AI-powered analysis reports for teams within companies. It orchestrates with an external AI generation service to produce detailed reports based on company data, stores the generated content securely in file storage, and provides endpoints for teams to retrieve their historical reports.

### Main Responsibilities

- Generate AI-powered analysis reports based on company requests
- Monitor the asynchronous status of report generation tasks
- Store generated report content in file storage with metadata tracking
- Retrieve individual reports or collections of reports for teams
- Manage report deletion and cleanup
- Assign and process company data for analysis
- Integrate with gRPC team service for user/team validation

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# 12.0 (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| File Storage | Custom IFileStorageService (abstracted) |
| Inter-Service (Client) | gRPC (calls TeamService) |
| Inter-Service (Task Queue) | External AI Generation Service (HTTP-based) |
| Object Mapping | DTOs (manual mapping) |
| Unit Testing | xUnit, Moq, FluentAssertions |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
ReportService/
??? Core/                                  # Domain layer
?   ??? DTOs/
?   ?   ??? AnalysisRequestDto.cs         # Input for AI report generation
?   ?   ??? AnalysisResponseDto.cs        # Task ID & status from AI service
?   ?   ??? GetReportFromAiDto.cs         # Input for retrieving generated report
?   ?   ??? AiReportResponseDto.cs        # Report response (content + metadata)
?   ?   ??? ReportDto.cs                  # Report metadata DTO (team listing)
?   ?   ??? TeamToReturnDto.cs            # Team info from gRPC
?   ?   ??? AI/
?   ?       ??? TaskStatusResponseDto.cs  # Task status from AI service
?   ??? Models/
?   ?   ??? Report.cs                     # Domain entity
?   ?   ??? Periodic.cs                   # Report frequency enum
?   ??? Interfaces/
?   ?   ??? IReportService.cs             # Service contract
?   ?   ??? IReportRepository.cs          # Repository contract
?   ?   ??? IGenerateReportService.cs     # AI service contract
?   ?   ??? IFileStorageService.cs        # File storage contract
?   ?   ??? IGrpcTeamService.cs           # gRPC team service contract
?   ?   ??? IUnitOfWork.cs                # UnitOfWork contract
?   ?   ??? IDataAssignService.cs         # Company data assignment contract
?   ?   ??? Generic/
?   ?       ??? IGenericRepository.cs     # Generic repository contract
?   ??? Enums/
?   ?   ??? Periodic.cs                   # Report frequency (One-time, Weekly, Monthly, etc.)
?   ??? Services/
?       ??? ReportService.cs              # Core business logic
?
??? Infrastructure/                        # Data access layer
?   ??? Context/
?   ?   ??? ApplicationDbContext.cs       # EF Core DbContext
?   ??? Migrations/
?   ?   ??? *.cs                          # Database schema migrations
?   ??? Repositories/
?   ?   ??? ReportRepository.cs           # Report-specific queries
?   ?   ??? Generic/
?   ?       ??? GenericRepository.cs      # Generic CRUD operations
?   ??? Services/
?   ?   ??? FileStorageService.cs         # File storage implementation
?   ?   ??? GrpcTeamService.cs            # gRPC TeamService client
?   ?   ??? DataAssignService.cs          # Company data assignment client
?   ??? UnitOfWork/
?       ??? UnitOfWork.cs                 # UnitOfWork implementation
?
??? WebApi/                                # Presentation layer
?   ??? Controllers/
?   ?   ??? ReportController.cs           # REST API endpoints
?   ??? Extensions/
?   ?   ??? ApiServicesExtention.cs       # Dependency injection setup
?   ??? Program.cs
?   ??? appsettings.json
?   ??? Dockerfile
?
??? Core.Tests/                            # Unit tests
?   ??? ReportServiceTests.cs             # ReportService unit tests
?
??? ReportService.sln

```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api/report`

---

### 4.1 Get User Data (Validation)

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/report/user/{userId}` |
| **Auth** | None (internal validation) |
| **Description** | Retrieves team information for a user via gRPC. Used to validate user belongs to a team before report operations. |

**Response Example (200 OK):**

```json
{
  "teamName": "Analytics Team",
  "teamId": "team-uuid",
  "companyId": "company-uuid"
}

```

**Response (404 Not Found):** HTTP 404 when user doesn't exist.

---

### 4.2 Create Report by AI

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/report/create` |
| **Auth** | None (intended for internal AI orchestration service) |
| **Description** | Initiates an asynchronous AI-powered report generation task. Returns a task ID for polling the generation status. |

**Request Body:**

```json
{
  "company_id": "company-uuid",
  "user_request": "Analyze sales trends for Q4 2025",
  "team_name": "Sales Team"
}

```

> All fields are required. `team_name` is optional but recommended.

**Response Example (200 OK):**

```json
{
  "task_id": "task-12345-abcde",
  "status": "PENDING",
  "message": "Report generation initiated"
}

```

---

### 4.3 Get Report Generation Status & Retrieve

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/report/status` |
| **Auth** | None (intended for internal use) |
| **Description** | Polls the status of a report generation task. If `SUCCESS`, automatically saves the report to the database and returns the content. If `PENDING` or `PROGRESS`, returns a status message. |

**Request Body:**

```json
{
  "teamId": "team-uuid",
  "userId": "user-uuid",
  "taskId": "task-12345-abcde",
  "periodic": "Weekly"
}

```

> `taskId`, `teamId`, `userId`, and `periodic` are all required.

**Response Example (200 OK — PENDING/PROGRESS):**

```json
{
  "isSuccess": false,
  "message": "your report is being generated."
}

```

**Response Example (200 OK — SUCCESS):**

```json
{
  "isSuccess": true,
  "aiResponse": "Sales analysis report content...",
  "generatedDate": "2026-05-17T10:30:00Z"
}

```

---

### 4.4 Retrieve Report by ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/report/{reportId}` |
| **Auth** | None (intended for team members) |
| **Description** | Retrieves a previously generated report by its ID. Returns the full content from file storage along with metadata. |

**Response Example (200 OK):**

```json
{
  "isSuccess": true,
  "aiResponse": "Sales analysis report content...",
  "generatedDate": "2026-04-15T14:22:00Z"
}

```

**Response (404 Not Found):** HTTP 404 when report doesn't exist.

**Response (400 Bad Request):** HTTP 400 when report file is missing or corrupted.

---

### 4.5 Get All Reports for Team

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/report/team/{teamId}` |
| **Auth** | None (intended for team members) |
| **Description** | Retrieves metadata for all reports belonging to a team. Does **not** include report content (for performance). |

**Response Example (200 OK):**

```json
[
  {
    "id": "report-uuid-1",
    "teamId": "team-uuid",
    "userId": "user-uuid-1",
    "periodic": "Weekly",
    "generatedDate": "2026-05-17T10:30:00Z",
    "content": null
  },
  {
    "id": "report-uuid-2",
    "teamId": "team-uuid",
    "userId": "user-uuid-2",
    "periodic": "Monthly",
    "generatedDate": "2026-05-10T09:15:00Z",
    "content": null
  }
]

```

**Response (200 OK — Empty):** HTTP 200 with empty array if team has no reports.

---

### 4.6 Delete Report

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/report/{reportId}` |
| **Auth** | None (intended for team managers) |
| **Description** | Deletes a report record from the database. The associated file in storage should be cleaned up by the file storage service. |

**Response Example (200 OK):**

```json
{
  "message": "Report deleted successfully"
}

```

**Response (404 Not Found):** HTTP 404 when report doesn't exist.

---

### 4.7 Assign Company Data (Background Task)

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/report/assign-data` |
| **Auth** | None (intended for internal admin/scheduler) |
| **Description** | Initiates an asynchronous task to assign and process company data. Returns a task ID for polling completion. |

**Request Body:**

```json
{
  "companyId": "company-uuid"
}

```

**Response Example (200 OK):**

```json
{
  "taskId": "data-assign-task-uuid"
}

```

---

### 4.8 Get Company Data Assignment Status

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/report/assign-data-status/{taskId}` |
| **Auth** | None (intended for internal monitoring) |
| **Description** | Polls the status of a company data assignment task. Returns status string (e.g., "PENDING", "SUCCESS", "FAILURE"). |

**Response Example (200 OK):**

```json
{
  "status": "SUCCESS"
}

```

---

## 5. Database Design

### Table: `Reports`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `TeamId` | `nvarchar(max)` | Required — foreign key (implicit, no FK constraint) |
| `UserId` | `nvarchar(max)` | Required — user who requested the report |
| `FilePath` | `nvarchar(max)` | Required — path to report file in storage |
| `FileType` | `nvarchar(max)` | Required (e.g., `.txt`, `.pdf`) |
| `Periodic` | `int` | Required — enum value (0=OneTime, 1=Weekly, etc.) |
| `GeneratedDate` | `datetime2` | Required — report generation timestamp |

### Relationships
- **Team** (implicit): Reports are grouped by `TeamId` for multi-tenancy.
- **User**: Reports track which user requested the analysis.

---

## 6. Domain Models

### Report Entity

```csharp
public class Report
{
    public string Id { get; set; }              // GUID
    public string TeamId { get; set; }          // Team identifier
    public string UserId { get; set; }          // User who requested
    public string FilePath { get; set; }        // File storage path
    public string FileType { get; set; }        // File extension (e.g., ".txt")
    public Periodic Periodic { get; set; }      // Report frequency
    public DateTime GeneratedDate { get; set; } // Generation timestamp
}
```

### Periodic Enum

```csharp
public enum Periodic
{
    OneTime = 0,
    Weekly = 1,
    BiWeekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Annually = 5
}
```

---

## 7. Data Transfer Objects

### AnalysisRequestDto

Used when requesting a new AI-powered report:

```json
{
  "company_id": "company-uuid",
  "user_request": "Analyze sales trends for Q4 2025",
  "team_name": "Sales Team"
}
```

### AnalysisResponseDto

Response from the AI service when a generation task is initiated:

```json
{
  "task_id": "task-12345-abcde",
  "status": "PENDING",
  "message": "Report generation initiated"
}
```

### GetReportFromAiDto

Request to check status and retrieve a generated report:

```json
{
  "teamId": "team-uuid",
  "userId": "user-uuid",
  "taskId": "task-12345-abcde",
  "periodic": "Weekly"
}
```

> Note: `userId` is marked with `[JsonIgnore]` to prevent client manipulation.

### AiReportResponseDto

Response containing report content and metadata:

```json
{
  "isSuccess": true,
  "aiResponse": "Report content...",
  "generatedDate": "2026-05-17T10:30:00Z",
  "message": "Success"  // Used for error messages when isSuccess=false
}
```

### ReportDto

Lightweight metadata for listing reports:

```json
{
  "id": "report-uuid",
  "teamId": "team-uuid",
  "userId": "user-uuid",
  "periodic": "Weekly",
  "generatedDate": "2026-05-17T10:30:00Z",
  "content": null  // Always null in list responses (commented-out implementation for future use)
}
```

### TeamToReturnDto

Team information retrieved via gRPC:

```json
{
  "teamName": "Analytics Team",
  "teamId": "team-uuid",
  "companyId": "company-uuid"
}
```

---

## 8. Service Architecture

### ReportService Responsibilities

The `ReportService` class orchestrates report lifecycle:

1. **GetUserData** — Validates user exists and retrieves team info via gRPC
2. **CreateReportByAi** — Delegates to `IGenerateReportService` to initiate async task
3. **GetReportFromAi** — Polls task status; on SUCCESS, saves report to database and file storage
4. **GetReportById** — Retrieves stored report content from file storage
5. **DeleteReport** — Removes report record from database
6. **GetAllReportsByTeamId** — Lists all reports for a team (metadata only)
7. **AssignCompanyData** — Delegates company data processing to `IDataAssignService`
8. **GetAssignCompanyDataStatus** — Polls company data assignment task status

### Key Dependencies (Injected via Constructor)

| Dependency | Purpose |
|---|---|
| `IGenerateReportService` | Communicates with external AI generation service |
| `IUnitOfWork` | Coordinates repository transactions |
| `IReportRepository` | Persists and queries Report entities |
| `IFileStorageService` | Stores and retrieves report file content |
| `IGrpcTeamService` | gRPC client for team/user validation |
| `IDataAssignService` | Manages asynchronous company data assignment |

---

## 9. Integration Points

### Outbound Integrations

| Service | Type | Purpose |
|---|---|---|
| **AI Generation Service** | HTTP (via `IGenerateReportService`) | Initiates and monitors report generation tasks |
| **TeamService** | gRPC | Retrieves team information for user validation |
| **DataAssignService** | HTTP/gRPC (via `IDataAssignService`) | Processes company data assignment |
| **File Storage** | Custom (via `IFileStorageService`) | Persists and retrieves report content |

### Task Lifecycle for Report Generation

```
1. Client calls POST /api/report/create with AnalysisRequestDto
   ?
2. ReportService.CreateReportByAi() initiates task via IGenerateReportService
   ?
3. External AI Service processes report asynchronously (PENDING ? PROGRESS ? SUCCESS/FAILURE)
   ?
4. Client polls POST /api/report/status with task ID
   ?
5. ReportService.GetReportFromAi() checks task status
   ?? If PENDING/PROGRESS ? Returns status message
   ?? If FAILURE ? Returns error (handled by GenerateReportService)
   ?? If SUCCESS ? Saves report to DB + file storage, returns content
```

---

## 10. Authentication & Multi-Tenancy

| Aspect | Detail |
|---|---|
| **Model** | Team-based multi-tenancy via `TeamId` |
| **Isolation** | Enforced at service/repository layer (no explicit auth on most endpoints) |
| **Authorization** | Callers must provide `TeamId` and `UserId` in request |
| **HTTPS** | Recommended for production |

### Implicit Authorization Pattern

While endpoints lack JWT validation, the design assumes:
- **Internal services** call these endpoints directly (no public access)
- **TeamId** is the trust boundary — only authorized services know valid team IDs
- **Validation** occurs in calling services (e.g., API Gateway, TeamService)

---

## 11. Error Handling

The service uses **Result/Error pattern** implicitly through DTOs:

| Scenario | Response |
|---|---|
| Report not found | `AiReportResponseDto { message = "there is no report with this id" }` with `isSuccess = false` |
| File content missing | `AiReportResponseDto { message = "there is no content for this report" }` with `isSuccess = false` |
| Report still generating | `AiReportResponseDto { message = "your report is being generated." }` with `isSuccess = false` |
| Successful retrieval | `AiReportResponseDto { isSuccess = true, aiResponse = "...", generatedDate = "..." }` |

---

## 12. Performance Considerations

### GetAllReportsByTeamId Optimization

The commented-out implementation in `GetAllReportsByTeamId` demonstrates a **semaphore-based concurrency pattern** for reading files in parallel while limiting resource usage:

```csharp
const int maxParallelTasks = 50;
var semaphore = new SemaphoreSlim(maxParallelTasks);
// Limits concurrent file reads to prevent storage I/O exhaustion
```

**Current Implementation (Active):** Returns metadata only, avoiding file I/O overhead. This is the recommended approach for list operations.

### Future Optimization Options

1. **Cache** report content for frequently accessed reports
2. **Pagination** when listing large team report collections
3. **Batch file reads** using the semaphore pattern if full content is later required

---

## 13. Testing

### Unit Test Strategy (Core.Tests)

`ReportServiceTests.cs` covers:

- ? `CreateReportByAi` — Delegates correctly to AI service
- ? `GetReportFromAi` — Handles PENDING/PROGRESS/SUCCESS/FAILURE states
- ? `GetReportById` — Returns report or error DTOs
- ? `DeleteReport` — Removes record and returns confirmation
- ? `GetAllReportsByTeamId` — Returns metadata collection

### Mock Dependencies

- `IGenerateReportService` — Simulates AI service responses
- `IReportRepository` — In-memory report store
- `IFileStorageService` — Mock file read/write
- `IGrpcTeamService` — Mock team info
- `IUnitOfWork` — Mock transaction coordination

---

## 14. Deployment & Configuration

### Environment Variables

| Variable | Default | Purpose |
|---|---|---|
| `CONNECTION_STRING` | *(required)* | SQL Server connection |
| `GRPC_TEAM_SERVICE_URL` | `https://team-service:5001` | TeamService gRPC endpoint |
| `AI_SERVICE_URL` | `https://ai-service:8000` | External AI generation service |
| `FILE_STORAGE_ROOT` | `./reports` | File storage base path |

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
```

### Health Checks

Implement health check endpoints to verify:
- ? Database connectivity
- ? File storage accessibility
- ? gRPC TeamService reachability

---

## 15. Architecture Notes

### Clean Architecture

- **Core** — Zero infrastructure dependencies. Business contracts and models only.
- **Infrastructure** — EF Core, repositories, external service clients.
- **WebApi** — Controllers, dependency injection, HTTP/gRPC endpoints.

### Dependency Injection (ApiServicesExtention.cs)

All dependencies are registered in the `AddApiServices()` extension:

```csharp
services.AddScoped<IReportService, ReportService>();
services.AddScoped<IReportRepository, ReportRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IFileStorageService, FileStorageService>();
services.AddScoped<IGrpcTeamService, GrpcTeamService>();
// ... etc
```

### Async-First Design

All I/O operations (database, file storage, gRPC, HTTP) are **fully asynchronous**, enabling efficient resource utilization under load.

### One Report Per Task

Reports are created asynchronously via an external AI service. The task ID is the primary correlation identifier until the report is saved to the database.

---

## 16. Roadmap & Future Enhancements

- [ ] **Report Scheduling** — Auto-generate periodic reports on schedule
- [ ] **Report Filtering** — Filter by date range, user, report type
- [ ] **Export Formats** — Support PDF, Excel, CSV in addition to text
- [ ] **Report Versioning** — Track multiple versions of the same report
- [ ] **Real-time Webhooks** — Notify teams when reports complete
- [ ] **Access Control Lists** — Fine-grained permissions per report
- [ ] **Full Content Caching** — Redis cache for frequently accessed reports
- [ ] **Report Templates** — Customizable templates per team/company

---

*RecoMind Project — ReportService*
