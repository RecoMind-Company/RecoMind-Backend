# 🔔 NotificationService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **NotificationService** is responsible for sending, storing, and managing notifications for all users in the RecoMind system. It supports two real-time delivery channels simultaneously: **SignalR** for active web/desktop users and **Firebase Cloud Messaging (FCM)** for mobile users. Notifications are consumed asynchronously from a **RabbitMQ** message queue via **MassTransit**.

### Main Responsibilities

- Consume notification events from RabbitMQ and process them
- Save notifications to the database for history and audit
- Deliver real-time notifications to online users via **SignalR**
- Deliver push notifications to mobile devices via **Firebase (FCM)**
- Provide users with their notification history, unread count, and status filtering
- Allow users to mark notifications as read (individually or all at once)
- Allow users to delete notifications
- Register and manage user device tokens for push notifications

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens |
| Real-Time (Web) | **ASP.NET Core SignalR** |
| Push Notifications | **Firebase Admin SDK (FCM)** |
| Message Broker | **RabbitMQ** via **MassTransit** |
| Object Mapping | AutoMapper 13 |
| Unit Testing | xUnit, Moq, FluentAssertions |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
NotificationService/
├── Notification.Core/                         # Domain layer
│   ├── DTOs/
│   │   ├── NotificationEventDto.cs            # Message shape consumed from queue
│   │   ├── NotificationResponseDto.cs         # Full notification response
│   │   └── RegisterDeviceTokenDto.cs          # Request for registering FCM device token
│   ├── Interfaces/
│   │   ├── INotificationRepository.cs         # Repository contract
│   │   ├── INotificationService.cs            # Service contract
│   │   ├── INotificationHubContext.cs         # SignalR abstraction
│   │   └── IPushNotificationService.cs        # FCM abstraction
│   ├── Mapper/
│   │   └── NotificationProfile.cs            # AutoMapper profiles
│   ├── Models/
│   │   ├── NotificationModel.cs               # Notification entity
│   │   └── UserDeviceToken.cs                 # FCM device token entity
│   ├── Result/                                # Result pattern
│   │   ├── Error.cs
│   │   ├── Result.cs
│   │   └── NotificationErrors.cs
│   └── Services/
│       └── NotificationService.cs             # Core business logic
│
├── Notification.Infrastructure/               # Data access & integrations
│   ├── Data/
│   │   └── NotificationDbContext.cs           # EF Core DbContext
│   ├── Messaging/
│   │   ├── NotificationConsumer.cs            # MassTransit consumer (RabbitMQ)
│   │   └── PushNotificationService.cs         # Firebase FCM implementation
│   ├── Migrations/
│   └── Repositories/
│       └── NotificationRepository.cs
│
├── Notification.WebApi/                       # Presentation layer
│   ├── Controllers/
│   │   └── NotificationsController.cs        # REST API endpoints
│   ├── Hubs/
│   │   ├── NotificationHub.cs                # SignalR hub
│   │   └── NotificationHubContext.cs         # SignalR hub context wrapper
│   ├── Program.cs
│   └── appsettings.json
│
├── Notification.Tests/                        # Unit tests
│   └── Services/
│       └── NotificationServiceTests.cs
│
├── Dockerfile
└── NotificationService.sln
```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api/notifications`
> All endpoints require the `AllEmployees` policy (JWT with role: `admin`, `manager`, `teamleader`, or `employee`).
> The `userId` is always extracted from the JWT `sub` (NameIdentifier) claim.

---

### 4.1 Get Notification History

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/notifications` |
| **Auth** | Required — Policy: `AllEmployees` |
| **Description** | Returns all notifications for the authenticated user, sorted by newest first. |

**Response Example (200 OK):**
```json
[
  {
    "id": "notif-uuid-1",
    "title": "New Plan Assigned",
    "message": "You have been assigned to Plan Q3.",
    "senderId": "manager-uuid",
    "receiverId": "user-uuid",
    "planId": "plan-uuid",
    "isRead": false,
    "createdAt": "2026-05-17T09:00:00Z"
  }
]
```

---

### 4.2 Get Notifications by Read Status

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/notifications/filter?isRead={true\|false}` |
| **Auth** | Required — Policy: `AllEmployees` |
| **Description** | Returns notifications filtered by read/unread status for the authenticated user. |

**Response Example (200 OK):** Same shape as 4.1.

---

### 4.3 Get Unread Count

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/notifications/unread-count` |
| **Auth** | Required — Policy: `AllEmployees` |
| **Description** | Returns the number of unread notifications for the authenticated user. |

**Response Example (200 OK):**
```json
{ "count": 5 }
```

---

### 4.4 Mark Single Notification as Read

| Field | Value |
|---|---|
| **Method** | `PATCH` |
| **Route** | `/api/notifications/{id}/mark-as-read` |
| **Auth** | Required — Policy: `AllEmployees` |
| **Description** | Marks a single notification as read and returns the updated notification. If already read, no update is made (idempotent). |

**Response Example (200 OK):**
```json
{
  "id": "notif-uuid-1",
  "title": "New Plan Assigned",
  "message": "You have been assigned to Plan Q3.",
  "isRead": true,
  "createdAt": "2026-05-17T09:00:00Z"
}
```

**Response (404 Not Found):** Notification ID does not exist.

---

### 4.5 Delete Notification

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/notifications/{id}` |
| **Auth** | Required — Policy: `AllEmployees` |
| **Description** | Deletes a notification by ID. |

**Response:** HTTP 204 No Content

**Response (404 Not Found):** Notification ID does not exist.

---

### 4.6 Register Device Token (FCM)

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/notifications/register-device` |
| **Auth** | Required — Policy: `AllEmployees` |
| **Description** | Registers or updates a Firebase device token for the authenticated user. If the token already exists, it is updated (timestamp + device type). Used to enable mobile push notifications. |

**Request Body:**
```json
{
  "deviceToken": "fcm-device-token-string",
  "deviceType": "Android"
}
```

> `deviceType` is optional. Accepted values (by convention): `"Android"`, `"iOS"`.

**Response Example (200 OK):**
```json
{ "message": "Device token registered or updated successfully." }
```

---

## 5. SignalR Hub

> **Hub URL:** `/hubs/notifications`
> Authentication: JWT token passed as query parameter `?access_token=...`

### Connection Flow

1. Client connects to `/hubs/notifications?access_token=<jwt>`
2. On connect, the server adds the connection to a SignalR **Group** named after the `userId`
3. On disconnect, the connection is removed from the group

### Client Event

When a notification is sent, connected clients in the target user's group receive:

```javascript
connection.on("ReceiveNotification", (notification) => {
  // notification is a NotificationResponseDto object
  console.log(notification.title, notification.message);
});
```

> If the user is offline, no error is thrown — the push notification (FCM) handles the offline case.

---

## 6. Message Queue (RabbitMQ)

The NotificationService is a **consumer** — it does not publish messages. Other services publish `NotificationEventDto` messages, and this service picks them up.

### Queue Configuration

| Setting | Value |
|---|---|
| Queue name | `notification-queue` |
| Exchange | MassTransit default |
| Consumer | `NotificationConsumer` |
| Library | MassTransit 8 + RabbitMQ |

### Message Shape (`NotificationEventDto`)

```json
{
  "title": "Task Updated",
  "message": "Your task status changed to In Progress.",
  "receiverId": "user-uuid",
  "senderId": "manager-uuid",
  "planId": "plan-uuid"
}
```

### Processing Flow

```
RabbitMQ (notification-queue)
    ↓  MassTransit Consumer
NotificationConsumer.Consume()
    ↓
NotificationService.SendNotificationAsync()
    ├── 1. Save to SQL Server (always)
    ├── 2. Send via SignalR (web/active users)
    └── 3. Send via FCM (mobile users — best effort, errors logged)
```

> FCM failures are **caught and logged**, not propagated — the overall operation always returns success as long as the DB save and SignalR delivery succeed.

---

## 7. Database Design

### Table: `Notifications`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `Title` | `nvarchar(200)` | Optional |
| `Message` | `nvarchar(max)` | Required |
| `SenderId` | `nvarchar(max)` | Optional — who sent it |
| `ReceiverId` | `nvarchar(450)` | Required — indexed for fast lookup |
| `PlanId` | `nvarchar(max)` | Optional — links to a plan if relevant |
| `IsRead` | `bit` | Default: `false` |
| `CreatedAt` | `datetime2` | Required |

**Index:** `IX_Notifications_ReceiverId` — for efficient per-user queries.

### Table: `UserDeviceTokens`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `UserId` | `nvarchar(max)` | Required — indexed |
| `DeviceToken` | `nvarchar(max)` | Required — FCM token |
| `DeviceType` | `nvarchar(max)` | Optional (`"Android"`, `"iOS"`) |
| `UpdatedAt` | `datetime2` | Nullable — updated on upsert |

**Index:** `IX_UserDeviceTokens_UserId`

### Relationships

```
Users (external, from AuthService)
  ↕  (userId is a reference, not a FK)
Notifications  (1 receiver → many notifications)
UserDeviceTokens  (1 user → many device tokens)
```

---

## 8. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token |
| **Claim used** | `NameIdentifier` (sub) — extracted as `UserId` |
| **HTTPS** | Required |
| **SignalR Auth** | JWT passed via query string `?access_token=` for WebSocket compatibility |

### Authorization Policies

| Policy | Allowed Roles |
|---|---|
| `AllEmployees` | `admin`, `manager`, `teamleader`, `employee` |
| `TeamLeadership` | `admin`, `manager`, `teamleader` |
| `Management` | `admin`, `manager` |

> Note: `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()` is called in `Program.cs` to prevent default claim type remapping, ensuring `ClaimTypes.NameIdentifier` maps cleanly to the JWT `sub` claim.

---

## 9. Architecture Notes

### Dual-Channel Delivery Strategy

The service tries to reach every user through the best available channel:

| Channel | Target | Failure Behavior |
|---|---|---|
| SignalR | Active web/desktop users | Silently ignored if user is offline |
| FCM (Firebase) | Mobile users (any connectivity) | Errors are logged, not propagated |

This means a notification is always saved to the database and always attempted via both channels. The overall operation only fails if the database save fails.

### Result Pattern

All service methods return `Result<T>`. Business validation errors (`InvalidUserId`, `InvalidId`, `NotFound`) are represented as `Error` records and mapped to appropriate HTTP status codes in the controller.

### MassTransit for Decoupled Consumption

Using MassTransit over the raw RabbitMQ client brings automatic retry, error queue management, deserialization, and DI-friendly consumer registration. The `NotificationConsumer` is a single class with a single `Consume` method.

### SignalR Group Strategy

Each user gets their own SignalR group named by their `userId`. This allows targeted delivery without maintaining a connection-to-user mapping manually. Connections are added/removed from the group in `OnConnectedAsync` / `OnDisconnectedAsync`.

### Clean Architecture (3-Layer)

- **Core** — domain entities, service logic, DTOs, interfaces, AutoMapper profiles, Result types. No dependencies on infrastructure.
- **Infrastructure** — EF Core repository, Firebase sender, MassTransit consumer.
- **WebApi** — controllers, SignalR hub + context wrapper, DI configuration.

### Interface Abstraction for Testability

Both the SignalR context (`INotificationHubContext`) and Firebase sender (`IPushNotificationService`) are hidden behind interfaces defined in the Core layer. This allows full unit testing of `NotificationService` with Moq without needing real SignalR or Firebase connections.

---

*RecoMind Project — NotificationService*
