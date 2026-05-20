# 🧩 chatbotservice - RecoMind

> Part of the **RecoMind** Project — ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **chatbotservice** is a specialized microservice designed to handle AI-driven conversations and natural language interactions for the RecoMind platform. It integrates with an external AI Copilot service to process user queries (e.g., generating SQL queries based on company and team contexts), stores interaction logs in a database, and facilitates real-time and asynchronous chat processing. It communicates internally with other services like `TeamService` via gRPC to retrieve context details like company ID and team name.

### Main Responsibilities

- Receive and validate user natural-language questions
- Communicate with the **TeamService** via gRPC to fetch the caller's team context (Company ID and Team Name)
- Forward queries asynchronously to the external AI Service (AI Copilot)
- Poll or retrieve final results from the AI Service using a task-based tracking ID
- Securely store chat history, including the user's question, AI response, generated SQL query, and timestamp
- Retrieve and clear chat history for individual users
- Expose secure REST endpoints with role-based access controls

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Inter-Service Communication | gRPC (client for TeamService) |
| Object Mapping | AutoMapper |
| JSON Serialization | Custom converters (e.g., handling polymorphic response messages) |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
chatbotservice/
├── Core/                                  # Domain Layer
│   ├── Const/
│   │   └── Status.cs                      # Enum for AI transaction status
│   ├── DTOs/                              # Data Transfer Objects
│   │   ├── AiService/                     # External AI service integration DTOs
│   │   │   ├── JesonConverter/            # Custom converters
│   │   │   ├── AiRequestDto.cs
│   │   │   ├── AiResponseDto.cs
│   │   │   └── FinalResponseDto.cs
│   │   ├── Chatbot/                       # Controller-level DTOs
│   │   │   ├── CreateChatRequestDto.cs
│   │   │   ├── GetHistoryDto.cs
│   │   │   ├── GetMethodDto.cs
│   │   │   ├── QuestionDto.cs
│   │   │   ├── SaveDto.cs
│   │   │   └── UserResponseDto.cs
│   │   └── TeamService/
│   │       └── GetTeamInformationDto.cs   # DTO for gRPC team response
│   ├── Interfaces/                        # Service & Repo abstractions
│   │   ├── IGenericRepository.cs
│   │   ├── ITeamService.cs
│   │   └── IUnitOfWork.cs
│   ├── Mapping/
│   │   └── ChatMappingProfile.cs          # AutoMapper profile
│   ├── Models/                            # Domain entities & settings
│   │   ├── AiServiceOptions.cs
│   │   ├── ChatMessage.cs
│   │   └── JwtSettings.cs
│   ├── Services/                          # Core service business logic
│   │   ├── Interface/
│   │   │   ├── IAiClientService.cs
│   │   │   └── IChatBotService.cs
│   │   ├── Protos/
│   │   │   └── GrpcChatbotService.proto
│   │   ├── AiClientService.cs
│   │   └── ChatBotService.cs
│   └── Core.csproj
│
├── Infrastructure/                        # Data & External gRPC Clients Layer
│   ├── Data/
│   │   └── ChatbotDbContext.cs            # EF Core DbContext
│   ├── Grpc/                              # Generated gRPC stubs & client implementations
│   │   ├── teamMessages.proto
│   │   ├── TeamService.proto
│   │   └── TeamServiceImpl.cs             # Client calling TeamService gRPC
│   ├── Migrations/                        # Database migrations
│   ├── Repository/
│   │   └── GenericRepo.cs                 # Generic Repository implementation
│   ├── UnitOfWork/
│   │   └── UnitOfWork.cs                  # Unit of Work implementation
│   └── Infrastructure.csproj
│
├── WebApi/                                # Presentation Layer
│   ├── Controllers/
│   │   └── ChatbotController.cs           # REST endpoints
│   ├── Properties/
│   ├── appsettings.json                   # Main configurations
│   ├── Program.cs                         # Application startup
│   └── WebApi.csproj
│
├── Tests/                                 # Unit Tests Layer
│   ├── ChatbotServiceTests.cs
│   └── Tests.csproj
│
├── chatbotservice.sln                     # Visual Studio Solution
└── Dockerfile                             # Containerization file
```

---

## 4. API Endpoints

### 4.1 Create Chat Query

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/Chatbot/CreateQuery` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Initiates an asynchronous query task with the AI Copilot. Fetches the caller's team details first, registers the request with AI, and returns a Task ID. |

**Request Body:**
```json
{
  "question": "What is our company's highest performing region?"
}
```

**Response Example (200 OK):**
```json
{
  "status": "SUCCESS",
  "message": "Task initialized successfully.",
  "taskId": "a98fdc3e-827d-41a4-b01c-6d8b392a83e0"
}
```

---

### 4.2 Fetch Chatbot Response & Save

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/Chatbot/ChatbotResponse` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Retrieves the final result from the AI service using the `TaskId`. If successful, the answer and the generated SQL query are saved to the database. |

**Request Body:**
```json
{
  "taskId": "a98fdc3e-827d-41a4-b01c-6d8b392a83e0",
  "user_question": "What is our company's highest performing region?"
}
```

**Response Example (200 OK):**
```json
{
  "status": "SUCCESS",
  "response": "The highest performing region is East, generating $4.2M in revenue."
}
```

---

### 4.3 Get Chat History

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/Chatbot/GetHistory` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Retrieves the complete history of chat queries and answers for the authenticated user. |

**Response Example (200 OK):**
```json
[
  {
    "id": "7cc3f4c6-e91b-4f76-88ad-56a5996f01da",
    "userQuestion": "What is our company's highest performing region?",
    "response": {
      "answer": "The highest performing region is East, generating $4.2M in revenue.",
      "sql_Query": "SELECT region, SUM(revenue) FROM sales GROUP BY region ORDER BY SUM(revenue) DESC LIMIT 1;"
    },
    "timeStamp": "2026-05-20T19:35:00Z"
  }
]
```

---

### 4.4 Delete Chat History

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/Chatbot/DeleteHistory` |
| **Auth** | Bearer JWT Token Required |
| **Description** | Deletes all saved chat history for the authenticated user. |

**Response Example (204 No Content):**
```
(No Content)
```

---

## 5. Database Design

### `ChatMessage` Entity

Stores the history of user queries and the respective AI responses.

| Property | Type | Constraints | Description |
|---|---|---|---|
| **Id** | `string` | Primary Key (GUID) | Unique identifier for the message |
| **UserId** | `string` | Required | ID of the user who initiated the query |
| **UserRole** | `string` | Required | Role of the user at the time of query |
| **UserQuestion** | `string` | Required | Natural language question asked by the user |
| **Response** | `ResponseMessage` | Nullable (Embedded object) | Complex type containing the `Answer` and `Sql_Query` |
| **TimeStamp** | `DateTime` | Required | Date and time when the message was saved |

---

## 6. Authentication & Security

- **JWT Authentication**: All endpoints require a valid Bearer token issued by `AuthenticationService`.
- **User context propagation**: Endpoints automatically read claims such as `ClaimTypes.NameIdentifier` and `ClaimTypes.Role` to tailor requests and secure operations to the caller.

---

## 7. Inter-Service Communication (gRPC)

The service acts as a client calling `TeamService` to obtain organizational context before contacting AI.

### Protocol Buffer Definitions
- `TeamService.proto` and `teamMessages.proto` are used to query:
  ```protobuf
  rpc GetTeamInformation (GetTeamInformationRequest) returns (GetTeamInformationResponse);
  ```

---

## 8. Configuration Reference

Example values in `appsettings.json`:

```json
{
  "AIService": {
    "BaseUrl": "https://ai.recomind.site/copilot/",
    "PostEndPont": "chat/async",
    "GetEndPoint": "chat/status"
  },
  "GrpcSettings": {
    "TeamServiceUrl": "http://team-svc:5010"
  }
}
```

---

## 9. Architecture Notes

- **Generic Repository & Unit of Work**: Ensures database operations are encapsulated cleanly in the Infrastructure layer.
- **Asynchronous AI Integration**: Leverages a two-step polling pattern to prevent long-running synchronous requests.
- **AutoMapper**: Decouples domain entities (`ChatMessage`) from DTOs exposed to the client.

---

## 10. Running the Service

Ensure SQL Server and `TeamService` are running.

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
