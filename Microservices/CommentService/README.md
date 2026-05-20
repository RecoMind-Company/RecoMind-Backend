# ?? CommentService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **CommentService** manages user comments on plans within the RecoMind platform. It enables real-time collaboration through SignalR, validates user access to plans via gRPC calls to other microservices, and provides secure REST API endpoints along with WebSocket support for live comment updates. Each comment is tied to a specific plan and user, with immutable history and time-limited editing capabilities.

### Main Responsibilities

- Store and manage comments on plans with full create, read, update, and delete operations
- Validate user authorization against plans using gRPC calls to PlanService and TeamService
- Provide real-time comment synchronization across multiple clients via SignalR
- Enforce business rules: 5-minute edit window, ownership-based access control
- Maintain comment history with creation and modification timestamps
- Support plan-based comment grouping and filtering

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API + SignalR) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Real-Time Communication | SignalR (WebSocket) |
| Inter-Service Communication | gRPC (client only) |
| Object Mapping | AutoMapper 12 |
| Validation | FluentValidation |
| Unit Testing | xUnit, Moq, FluentAssertions |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
CommentService/
??? Core/                                  # Domain layer
?   ??? DTOs/
?   ?   ??? AddCommentDto.cs               # Input for creating a comment
?   ?   ??? UpdateCommentDto.cs            # Input for updating a comment
?   ?   ??? CommentDto.cs                  # Response DTO
?   ?   ??? Plan/
?   ?   ?   ??? PlanIdsDto.cs              # gRPC response wrapper for plan validation
?   ?   ??? ...
?   ??? Models/
?   ?   ??? Comment.cs                     # Domain entity
?   ??? Interfaces/
?   ?   ??? ICommentService.cs             # Service contract
?   ?   ??? IGenericRepository.cs          # Generic repository contract
?   ?   ??? IUnitOfWork.cs                 # Unit of Work pattern
?   ?   ??? ...
?   ??? ServicesAbstraction/
?   ?   ??? ICommentService.cs             # Comment service interface
?   ?   ??? IGrpcPlanService.cs            # gRPC Plan service client
?   ?   ??? IGrpcTeamService.cs            # gRPC Team service client
?   ?   ??? IUserQuestGrpcService.cs       # gRPC UserQuest service client
?   ??? MappingProfiles/
?   ?   ??? CommentProfile.cs              # AutoMapper configuration
?   ??? Result/                            # Result pattern for error handling
?   ?   ??? Error.cs                       # Error record
?   ?   ??? Result.cs                      # Result<T> wrapper
?   ?   ??? CommentErrors.cs               # Comment-specific errors
?   ?   ??? PlanErrors.cs                  # Plan validation errors
?   ?   ??? TeamErrors.cs                  # Team validation errors
?   ??? Settings/
?   ?   ??? JwtOptions.cs                  # JWT configuration
?   ??? Services/
?       ??? CommentService.cs              # Core business logic
?
??? Infrastructure/                        # Data access layer
?   ??? Context/
?   ?   ??? ApplicationDbContext.cs        # Entity Framework DbContext
?   ??? Repository/
?   ?   ??? GenericRepository.cs           # Generic repository implementation
?   ?   ??? UnitOfWork.cs                  # Unit of Work implementation
?   ??? gRPC/
?   ?   ??? Plan/
?   ?   ?   ??? GrpcPlanService.cs         # gRPC Plan service client
?   ?   ??? Team/
?   ?   ?   ??? GrpcTeamService.cs         # gRPC Team service client
?   ?   ??? UserQuests/
?   ?       ??? UserQuestGrpcService.cs    # gRPC UserQuest service client
?   ??? Migrations/
?       ??? 20260318203647_InitialMigration.cs
?       ??? 20260326023536_AddCreatedAtAndUpdatedAtColumns.cs
?
??? WebApi/                                # Presentation layer
?   ??? Controllers/
?   ?   ??? BaseApiController.cs           # Base controller with error handling
?   ?   ??? CommentController.cs           # REST API endpoints
?   ??? Hubs/
?   ?   ??? CommentHub.cs                  # SignalR Hub for real-time comments
?   ?   ??? ICommentClient.cs              # SignalR client interface (contracts)
?   ?   ??? HubFilters/
?   ?       ??? CommentHubFilter.cs        # Hub-level request/response filters
?   ??? Validators/
?   ?   ??? AddCommentDtoValidator.cs      # FluentValidation rules for creating comments
?   ?   ??? UpdateCommentDtoValidator.cs   # FluentValidation rules for updating comments
?   ??? Program.cs                         # Application startup & DI configuration
?   ??? appsettings.json                   # Configuration file
?   ??? Dockerfile                         # Docker container configuration
?
??? Core.Tests/                            # Unit tests
?   ??? Services/
?       ??? CommentServiceTests.cs         # Service logic tests
?
??? CommentService.sln
```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api/comment`

---

### 4.1 Get Comments by Plan ID

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/comment/plans/{planId}` |
| **Auth** | Required — JWT Bearer Token |
| **Description** | Retrieves all comments for a specific plan, sorted by creation date (newest first). The plan must exist and the user must have access to it. |

**Response Example (200 OK):**

```json
[
  {
    "id": "comment-uuid-1",
    "userComment": "Great plan structure!",
    "userId": "user-uuid",
    "planId": "plan-uuid",
    "createdAt": "2026-05-17T10:30:00Z",
    "updatedAt": null
  },
  {
    "id": "comment-uuid-2",
    "userComment": "I suggest adding more details.",
    "userId": "user-uuid-2",
    "planId": "plan-uuid",
    "createdAt": "2026-05-17T10:25:00Z",
    "updatedAt": "2026-05-17T10:26:00Z"
  }
]
```

**Response (404 Not Found — plan does not exist):**

```json
{
  "message": "Plan not found"
}
```

---

## 5. Real-Time SignalR Hub

> **WebSocket Hub URL:** `wss://api.recomind.site/hubs/comments`

The **CommentHub** provides real-time, bidirectional communication for comment updates. Clients must pass a valid JWT token and `planId` query parameter during connection.

---

### 5.1 Hub Connection

**Connection Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| `planId` | Query String | Yes | The ID of the plan to connect to. Connection fails if missing. |
| `access_token` | Query String | Yes | JWT Bearer token passed as query parameter (extracted from Authorization header if not found). |

**Example Connection URL:**
```
wss://api.recomind.site/hubs/comments?planId=plan-uuid-123&access_token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Authorization Flow:**
1. User connects with a valid JWT token
2. The token is extracted from query string or Authorization header
3. gRPC calls verify the user is:
   - The owner of the plan, **OR**
   - A team member associated with the plan's team
4. If authorized, the user is added to a group for that `planId`
5. If not authorized, the connection is immediately aborted

---

### 5.2 Hub Methods (Server ? Client)

Methods the server invokes on connected clients:

#### **ReceiveComment(CommentDto)**

Broadcasts a newly created comment to all clients in the plan's group.

```csharp
await Clients.Group(planId).ReceiveComment(comment);
```

**Payload Example:**

```json
{
  "id": "comment-uuid",
  "userComment": "This is a new comment",
  "userId": "user-uuid",
  "planId": "plan-uuid",
  "createdAt": "2026-05-17T11:00:00Z",
  "updatedAt": null
}
```

---

#### **ReceiveEditedComment(CommentDto)**

Broadcasts an updated comment to all clients in the plan's group.

```csharp
await Clients.Group(planId).ReceiveEditedComment(comment);
```

**Payload Example:**

```json
{
  "id": "comment-uuid",
  "userComment": "Updated comment text",
  "userId": "user-uuid",
  "planId": "plan-uuid",
  "createdAt": "2026-05-17T11:00:00Z",
  "updatedAt": "2026-05-17T11:05:00Z"
}
```

---

#### **ReceiveDeletedComment(string commentId, string message)**

Broadcasts a deleted comment notification to all clients in the plan's group.

```csharp
await Clients.Group(planId).ReceiveDeletedComment(commentId, "This comment has been deleted.");
```

**Payload Example:**

```json
{
  "commentId": "comment-uuid",
  "message": "This comment has been deleted."
}
```

---

#### **ReceiveErrors(IEnumerable<Error>)**

Sends error messages to the caller (specific client) when an operation fails.

```csharp
await Clients.Caller.ReceiveErrors(errors);
```

**Payload Example:**

```json
{
  "message": "Edit timeout exceeded",
  "code": "EDIT_TIMEOUT"
}
```

---

### 5.3 Hub Methods (Client ? Server)

Methods clients invoke on the server:

#### **CreateComment(AddCommentDto)**

Creates a new comment on the plan.

**Request Payload:**

```json
{
  "userComment": "This is my comment",
  "planId": "plan-uuid",
  "userId": "user-uuid"
}
```

> Note: `planId` and `userId` are automatically set from the hub's context and cannot be overridden by the client.

**Business Rules:**
- User must be authenticated (JWT token valid)
- User must be the plan owner or a team member
- `userComment` is required and cannot be empty
- Comment is created with current UTC timestamp

**Success Response:** Broadcasts `ReceiveComment` to all clients in the group.

**Failure Response:** Sends `ReceiveErrors` to the caller.

---

#### **EditComment(UpdateCommentDto)**

Updates an existing comment.

**Request Payload:**

```json
{
  "commentId": "comment-uuid",
  "userComment": "Updated comment text"
}
```

> Note: `userId` is automatically set from the hub's context.

**Business Rules:**
- Comment must exist
- Only the original author can edit the comment
- Edit must occur within **5 minutes** of creation
- `userComment` is required and cannot be empty

**Success Response:** Broadcasts `ReceiveEditedComment` to all clients in the group.

**Failure Response:** Sends `ReceiveErrors` to the caller with one of:
- `COMMENT_NOT_FOUND` — comment does not exist
- `ACCESS_DENIED` — user is not the author
- `EDIT_TIMEOUT` — 5-minute edit window has expired

---

#### **DeleteComment(string commentId)**

Deletes an existing comment.

**Request Payload:**

```
commentId: "comment-uuid"
```

**Business Rules:**
- Comment must exist
- Only the original author can delete the comment

**Success Response:** Broadcasts `ReceiveDeletedComment` to all clients in the group.

**Failure Response:** Sends `ReceiveErrors` to the caller with one of:
- `COMMENT_NOT_FOUND` — comment does not exist
- `ACCESS_DENIED` — user is not the author

---

## 6. Database Design

### Table: `Comments`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `UserComment` | `nvarchar(max)` | Required — comment text content |
| `UserId` | `nvarchar(max)` | Required — author ID (foreign reference to User) |
| `PlanId` | `nvarchar(max)` | Required — associated plan ID (foreign reference to Plan) |
| `CreatedAt` | `datetime2` | Nullable — defaults to UTC now |
| `UpdatedAt` | `datetime2` | Nullable — set only when comment is edited |

### Business Constraints

- **One-to-Many:** One plan can have many comments; each comment belongs to exactly one plan
- **One-to-Many:** One user can have many comments; each comment is authored by exactly one user
- **Edit Window:** Comments can only be edited within 5 minutes of creation
- **Ownership:** Only the comment author can edit or delete their own comments

---

## 7. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token |
| **Token Source** | Authorization header or query string (for SignalR) |
| **Claim Used** | `NameIdentifier` — extracts `UserId` from claims |
| **HTTPS** | Required |
| **SignalR Security** | Custom `OnMessageReceived` event handles token extraction from query params |

### Authorization Flow

1. **REST Endpoint Access:** JWT token required in Authorization header
2. **SignalR Connection:** JWT token required (extracted from query string or header)
3. **Plan Validation:** gRPC call to PlanService verifies:
   - Plan exists
   - User is either the plan owner or a team member
4. **Comment Operations:** User must be the comment author (for edit/delete)

---

## 8. Inter-Service Communication (gRPC)

The CommentService acts as a **gRPC client** to other microservices:

| Service | Method | Purpose |
|---|---|---|
| **PlanService** | `GetPlanIds(planId)` | Verify plan exists and retrieve team ID |
| **PlanService** | `IsOwnerOfPlan(userId, planId)` | Check if user owns the plan |
| **TeamService** | `IsUserExist(userId, teamId)` | Check if user is a team member |
| **UserQuestService** | `IsUserInPlan(userId, planId)` | Check if user has access to plan quests |

**Configuration (appsettings.json):**

```json
{
  "GrpcSettings": {
    "PlanServiceUrl": "http://plan-svc:5005",
    "TeamServiceUrl": "http://team-svc:5010",
    "UserQuestsServiceUrl": "http://task-svc:5013"
  }
}
```

**gRPC Client Registration:**

```csharp
builder.Services.AddGrpcClient<PlanServiceGrpc.PlanServiceGrpcClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:PlanServiceUrl"]);
});

builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:TeamServiceUrl"]);
});

builder.Services.AddGrpcClient<GrpcUserQuestsService.GrpcUserQuestsServiceClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:UserQuestsServiceUrl"]);
});
```

---

## 9. Validation

The service uses **FluentValidation** to ensure data integrity:

### AddCommentDtoValidator

```csharp
public class AddCommentDtoValidator : AbstractValidator<AddCommentDto>
{
    public AddCommentDtoValidator()
    {
        RuleFor(x => x.UserComment)
            .NotEmpty().WithMessage("Comment text is required")
            .MaximumLength(5000).WithMessage("Comment cannot exceed 5000 characters");
    }
}
```

### UpdateCommentDtoValidator

```csharp
public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
{
    public UpdateCommentDtoValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("Comment ID is required");
        
        RuleFor(x => x.UserComment)
            .NotEmpty().WithMessage("Comment text is required")
            .MaximumLength(5000).WithMessage("Comment cannot exceed 5000 characters");
    }
}
```

---

## 10. Error Handling

The service uses the **Result Pattern** for explicit error handling:

### CommentErrors

```csharp
public static class CommentErrors
{
    public static Error NotFound => new Error("COMMENT_NOT_FOUND", "Comment not found");
    public static Error AccessDenied => new Error("ACCESS_DENIED", "You do not have permission to perform this action");
    public static Error EditTimeout => new Error("EDIT_TIMEOUT", "You can only edit a comment within 5 minutes of creation");
}
```

### PlanErrors

```csharp
public static class PlanErrors
{
    public static Error PlanNotFound => new Error("PLAN_NOT_FOUND", "Plan not found");
}
```

### TeamErrors

```csharp
public static class TeamErrors
{
    public static Error UserNotInTeam => new Error("USER_NOT_IN_TEAM", "User is not a member of this team");
}
```

---

## 11. Architecture Notes

### Result Pattern
All service methods return `Result<T>`. Business errors (not found, access denied, timeout) are returned as `Error` records, making error handling explicit and testable rather than exception-based.

### Clean Architecture
- **Core** — Zero infrastructure dependencies. Business rules, interfaces, and domain models only.
- **Infrastructure** — Entity Framework Core, repository implementation, gRPC client wrappers.
- **WebApi** — Controllers, SignalR Hub, validators, and dependency injection wiring.

### Unit of Work Pattern
`UnitOfWork` manages multiple repositories and ensures atomic operations:
```csharp
var comment = mapper.Map<Comment>(addCommentDto);
await _commentRepository.AddAsync(comment);
await unitOfWork.SaveChangesAsync();  // All changes committed together
```

### Generic Repository Pattern
`GenericRepository<T>` provides CRUD operations without duplicating code:
```csharp
await _commentRepository.AddAsync(comment);
await _commentRepository.FindAll(c => c.PlanId == planId, orderBy: x => x.OrderByDescending(y => y.CreatedAt));
_commentRepository.Update(comment);
_commentRepository.Delete(comment);
```

### SignalR Hub Filters
`CommentHubFilter` (Hub-level middleware) allows cross-cutting concerns like logging and authentication validation across all hub methods.

### gRPC Short-Circuit Evaluation
The `OnConnectedAsync` method uses short-circuit evaluation to minimize gRPC calls:
```csharp
if (await grpcPlanService.IsOwnerOfPlanAsync(userId!, planId!) 
    || await grpcTeamService.IsUserExist(userId!, plan.TeamId!))
{
    // If first condition is true, second is not evaluated
}
```

### Two-Tier Authorization
1. **SignalR Connection Level** — Validates user can access the plan group
2. **Operation Level** — Validates user can perform the specific action (edit/delete own comment only)

---

## 12. Dependency Injection Configuration

**Program.cs:**

```csharp
// Database
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("CommentDbConnectionString"));
});

// Services
builder.Services.AddScoped<ICommentService, CommentService>();

// gRPC Clients
builder.Services.AddGrpcClient<PlanServiceGrpc.PlanServiceGrpcClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:PlanServiceUrl"]);
});
builder.Services.AddGrpcClient<TeamGrpcService.TeamGrpcServiceClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:TeamServiceUrl"]);
});
builder.Services.AddGrpcClient<GrpcUserQuestsService.GrpcUserQuestsServiceClient>(options =>
{
    options.Address = new Uri(configuration["GrpcSettings:UserQuestsServiceUrl"]);
});

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(CommentProfile).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(AddCommentDtoValidator).Assembly);

// SignalR
builder.Services.AddSignalR(option =>
{
    option.AddFilter<CommentHubFilter>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
```

---

## 13. Configuration Reference

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "CommentDbConnectionString": "Server=tcp:recomind.database.windows.net,1433;Initial Catalog=comment;User ID=recomind;Password=...;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"
  },
  "JwtOptions": {
    "Issuer": "https://api.recomind.site/",
    "Audience": "https://api.recomind.site/",
    "DurationInHours": 2,
    "SecretKey": "your-secret-key-here"
  },
  "GrpcSettings": {
    "PlanServiceUrl": "http://plan-svc:5005",
    "TeamServiceUrl": "http://team-svc:5010",
    "UserQuestsServiceUrl": "http://task-svc:5013"
  },
  "AllowedHosts": "*"
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` — Development, Staging, or Production
- `ConnectionStrings__CommentDbConnectionString` — Database connection string
- `JwtOptions__SecretKey` — JWT signing key (from User Secrets in development)

---

## 14. Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "WebApi/WebApi.csproj"
COPY . .
RUN dotnet build "WebApi/WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
```

---

## 15. Development Setup

### Prerequisites
- .NET 8 SDK
- SQL Server (local or containerized)
- Visual Studio 2022 or VS Code with C# extension

### Getting Started

```bash
# Clone the repository
git clone https://github.com/RecoMind-Company/RecoMind-Backend.git
cd RecoMind-Backend/Microservices/CommentService

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update -p Infrastructure -s WebApi

# Run the service
dotnet run --project WebApi
```

### Running Tests

```bash
dotnet test Core.Tests -v normal
```

---

*RecoMind Project — CommentService*
