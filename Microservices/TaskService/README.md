# ?? TaskService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **TaskService** is a microservice responsible for managing all aspects of quests, tasks, and their assignments to users within the RecoMind platform. It handles the lifecycle of a quest from creation to completion, manages user participation, and provides data to other internal services.

### Main Responsibilities

-   Create, read, update, and delete quests.
-   Assign users to quests and track their progress.
-   Manage quest statuses (e.g., `Pending`, `InProgress`, `Completed`).
-   Expose RESTful API endpoints for client applications (e.g., web/mobile frontends).
-   Expose gRPC endpoints for internal service-to-service communication.
-   Validate business rules, such as ensuring a user is not added to the same quest twice.

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Inter-Service (Server) | gRPC |
| Object Mapping | AutoMapper |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Validation | FluentValidation |
| Unit Testing | xUnit, Moq (assumed from project structure) |
| Containerization | Docker |

---

## 3. Project Structure

```
TaskService/
??? Core/                                  # Domain layer
?   ??? Dtos/
?   ?   ??? QuestDto.cs                    # Input for creating a quest
?   ?   ??? UpdateQuestDto.cs              # Input for updating a quest
?   ?   ??? QuestToReturnDto.cs            # Standard quest response
?   ?   ??? QuestByStatusDto.cs            # Input for filtering quests by status
?   ?   ??? AddUserToQuestDto.cs           # Input for adding a user to a quest
?   ??? Models/
?   ?   ??? Quest.cs                       # Quest entity
?   ?   ??? UserQuests.cs                  # Join entity for user-quest relationship
?   ?   ??? QuestStatusEnum.cs             # Enum for quest statuses
?   ??? MappingProfiles/
?   ?   ??? QuestProfile.cs                # AutoMapper profiles
?   ??? Result/
?   ?   ??? Result.cs                      # Result pattern for service responses
?   ?   ??? QuestErrors.cs                 # Domain-specific errors for Quests
?   ?   ??? UserQuestsErrors.cs            # Domain-specific errors for UserQuests
?   ??? ServicesAbstractions/
?   ?   ??? IQuestService.cs               # Quest service contract
?   ?   ??? IUserQuestsService.cs          # UserQuests service contract
?   ??? Services/
?       ??? QuestService.cs                # Core business logic for quests
?       ??? UserQuestsService.cs           # Core business logic for user assignments
?
??? Infrastructure/                        # Data access layer
?   ??? Data/
?   ?   ??? AppDbContext.cs
?   ??? Repositories/
?       ??? QuestRepository.cs
?       ??? UserQuestsRepository.cs
?
??? WebApi/                                # Presentation layer
?   ??? Controllers/
?   ?   ??? QuestController.cs             # REST endpoints for quests
?   ?   ??? UserQuestsController.cs        # REST endpoints for user assignments
?   ??? gRPC/
?   ?   ??? UserQuestGrpcService.cs        # gRPC server for user-quest queries
?   ??? Validators/
?   ?   ??? QuestDtoValidator.cs
?   ?   ??? UpdateQuestDtoValidator.cs
?   ?   ??? AddUserToQuestDtoValidator.cs
?   ??? Program.cs
?
??? Core.Tests/                            # Unit tests for the domain layer
??? WebApi.Tests/                          # Integration tests for the API layer
?
??? Dockerfile
??? TaskService.sln
```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api`

---

### Quest Management (`/api/Quest`)

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/` | Required | Retrieves all quests. |
| `GET` | `/{id}` | Required | Retrieves a single quest by its ID. |
| `POST` | `/` | Required | Creates a new quest. |
| `PUT` | `/{id}` | Required | Updates an existing quest. |
| `DELETE` | `/{id}` | Required | Deletes a quest. |
| `POST` | `/GetQuestsByStatus` | Required | Filters quests by their status (`Pending`, `InProgress`, etc.). |

---

### User-Quest Management (`/api/UserQuests`)

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/AddUserToQuest` | Required | Assigns a user to a quest. |
| `GET` | `/GetUserQuests/{userId}` | Required | Retrieves all quests assigned to a specific user. |

---

## 5. Database Design

### Table: `Quests`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `Title` | `nvarchar(max)` | Required |
| `Description` | `nvarchar(max)` | Required |
| `Status` | `int` | Mapped to `QuestStatusEnum` |
| `CreatedAt` | `datetime2` | Timestamp of creation |
| `TeamId` | `nvarchar(450)` | FK to an external `Teams` service/table |

### Table: `UserQuests`

| Column | Type | Notes |
|---|---|---|
| `Id` | `uniqueidentifier` | PK |
| `UserId` | `nvarchar(450)` | Composite Key 1, FK to an external `Users` service/table |
| `QuestId` | `uniqueidentifier` | Composite Key 2, FK to `Quests` table |

### Business Constraints

-   A user cannot be added to the same quest more than once. This is enforced by a unique constraint on (`UserId`, `QuestId`) in the `UserQuests` table and validated in the `UserQuestsService`.

---

## 6. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token |
| **Identity Provider** | An external identity service provides the JWT. |
| **Claims** | The service expects standard claims like `sub` (user ID) for authorization. |
| **HTTPS** | Required for all endpoints. |

---

## 7. Service Communication

### As a gRPC Server

The TaskService exposes a gRPC service for internal queries related to user quests. This allows other microservices to efficiently check quest data without the overhead of HTTP/JSON.

| gRPC Method | Description |
|---|---|
| `CheckUserQuest` | Determines if a specific user is assigned to a specific quest. |

-   **Proto Definition**: `user_quests.proto` (inferred)
-   **Service**: `UserQuestGrpcService`

---

## 8. Architecture Notes

### Clean Architecture

The solution follows the principles of Clean Architecture, separating concerns into distinct layers:
-   **Core**: Contains domain entities, business logic, service interfaces, and DTOs. It has no dependencies on external frameworks like EF Core or ASP.NET Core.
-   **Infrastructure**: Implements data access logic using Entity Framework Core and repositories.
-   **WebApi**: The presentation layer, responsible for exposing API endpoints (REST and gRPC) and handling HTTP requests.

### Result Pattern

Service methods return a `Result<T>` object instead of throwing exceptions for predictable business errors (e.g., "Quest not found," "User already in quest"). This makes error handling explicit and separates expected failures from true exceptions.

### Validation

FluentValidation is used to enforce validation rules on incoming DTOs at the API boundary, ensuring that invalid data is rejected early with a clear `400 Bad Request` response.

---

*RecoMind Project — TaskService*
