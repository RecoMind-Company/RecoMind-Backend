# Chapter: Database Design & Architecture

## RecoMind Platform — Graduation Project

---

## 1. Database Architecture Overview

The RecoMind platform is built on a microservices architecture in which each service is independently deployable, independently scalable, and fully responsible for its own data. The foundational principle governing the data layer is **Database-per-Service**: no two services share a database, and no service queries another service's tables directly. All inter-service data exchange flows through well-defined contracts — either REST APIs or gRPC calls.

The system spans thirteen microservices, each responsible for managing its own persistent data store. The majority use **Microsoft SQL Server** via Entity Framework Core, while the **MappingService** uses **PostgreSQL** for reasons tied to its role in the AI/vector workflow, discussed in Section 3. The following table provides a system-wide summary:

| Service | Database Engine | Primary Responsibility |
|---|---|---|
| AuthenticationService | SQL Server | User identity, roles, and job titles |
| CompanyService | SQL Server | Company profiles and registration |
| SubscriptionService | SQL Server | Subscription plans and billing management |
| DatabaseSettingService | SQL Server | Encrypted client database credentials |
| MappingService | PostgreSQL | AI schema-to-team table mappings |
| TeamService | SQL Server | Teams and organizational membership |
| PlanService | SQL Server | Plans, statuses, and plan types |
| NotificationService | SQL Server | Notifications and device tokens |
| ChatbotService | SQL Server | AI chat interaction and query history |
| CommentService | SQL Server | Plan-level user feedback and comments |
| InvitationService | SQL Server | Company onboarding invitation workflow |
| ReportService | SQL Server | Analytical report generation metadata |
| TaskService | SQL Server | Tasks (Quests) and employee assignments |

This distributed design means that the system has no single database schema. Instead, it has a federation of smaller, purpose-built schemas, each reflecting the domain model of its owning service.

---

## 2. Distributed Database Approach

### Rationale

A shared database for a multi-service system creates tight coupling at the data layer: schema changes in one area cascade into breakages elsewhere, deployment coordination becomes mandatory, and a single database becomes both a scalability bottleneck and a single point of failure. The Database-per-Service pattern treats each service’s data as a private resource, reducing coupling and improving service autonomy.

In RecoMind, this separation allows each service to evolve its schema independently, choose the most appropriate storage technology, and fail in isolation without affecting the rest of the platform.

### Cross-Service Data Consistency

Because services cannot join across databases, cross-service references are maintained as **logical references** — a service stores an identifier (typically a GUID string) that points to a record owned by another service, without a database-level foreign key constraint. Consistency is maintained through three mechanisms:

* **gRPC validation calls** are made before write operations when strong consistency is required. For example, CompanyService validates a `SubscriptionId` against SubscriptionService via gRPC before saving a company record, and PlanService validates a `TeamId` against TeamService in the same way.

* **JWT claims** carry authoritative context — including `UserId`, `CompanyId`, and assigned roles — embedded and signed by the AuthenticationService at login. Every subsequent request presents this token, and each service extracts the relevant claims without making a synchronous call back to AuthService. This keeps authentication fast and stateless while distributing identity context across all services.

* **Asynchronous messaging** handles eventual-consistency scenarios. The NotificationService, for instance, does not receive direct calls from plan or task services. Instead, those services publish events to a **RabbitMQ** queue, and the NotificationService consumes them independently via **MassTransit**, decoupling delivery from the originating operation.

---

## 3. Service Database Ownership

This section describes the data model of each service — its entities, key constraints, and the role each database plays within the broader system.

### 3.1 AuthenticationService

The AuthenticationService is the identity authority for the entire platform. It is built on **ASP.NET Core Identity**, which provides the standard `AspNetUsers`, `AspNetRoles`, and `AspNetUserRoles` tables. The base `User` entity is extended with four application-specific fields: `FullName`, `PhotoUrl`, `IsActive`, and `CreatedOn`. A companion table, `UsersJobTitle`, stores each user's job title separately to allow updates without touching the core identity record.

The system defines four application roles — `Admin`, `Manager`, `Teamleader`, and `Employee` — each granting progressively narrower access scope. At login, the service generates a signed JWT embedding the user's `Id`, `CompanyId`, and roles as claims. This token is the primary mechanism by which all other services receive identity context; they verify and read it without calling back to AuthService, making the authentication flow stateless.

**Key Entities:**

| Entity | Primary Key | Notable Columns |
|---|---|---|
| `AspNetUsers` (User) | `Id` (GUID) | `FullName`, `PhotoUrl`, `IsActive`, `CreatedOn` |
| `UsersJobTitle` | `Id` (GUID) | `UserId` (FK → User), `JobTitle` |
| `AspNetRoles` | `Id` (GUID) | `Name` (Admin / Manager / Teamleader / Employee) |
| `AspNetUserRoles` | Composite (`UserId`, `RoleId`) | Junction table — user-to-role assignments |

### 3.2 CompanyService

The CompanyService stores the core company profiles that anchor almost every other domain entity in the system. A `Company` record holds metadata such as industry, country, employee size, and a unique registration code. It also stores two logical references: `AdminId` (pointing to the administering user in AuthService) and `SubscriptionId` (pointing to the active subscription in SubscriptionService). The latter is validated via a gRPC call before any write operation.

**Key Entity:**

| Column | Type | Description |
|---|---|---|
| `Id` | GUID PK | Unique company identifier |
| `Name`, `Industry`, `Description`, `Country`, `Size`, `Code` | string | Company profile metadata |
| `AdminId` | string | Logical FK → AuthService User |
| `SubscriptionId` | string | Logical FK → SubscriptionService Subscription |
| `CreatedAt` | datetime | Record creation timestamp |

### 3.3 SubscriptionService

The SubscriptionService manages the billing layer. It maintains two entities: `SubscriptionType`, which defines available plan templates (e.g., Basic, Premium, Enterprise) with their names and prices; and `Subscription`, which represents an active subscription assignment for a company, recording the billing cycle, active dates, and a reference to the plan type. CompanyService references `Subscription.Id` and validates it via gRPC before storing.

**Key Entities:**

| Entity | Primary Key | Notable Columns |
|---|---|---|
| `Subscription` | `Id` (GUID) | `Price`, `StartDate`, `EndDate`, `IsActive`, `BillingCycle`, `SubscriptionTypeId` (FK) |
| `SubscriptionType` | `SubscriptionTypeId` (GUID) | `PlanName`, `Price` |


The relationship between these two entities is the only direct foreign key in this service: one `SubscriptionType` may be referenced by many `Subscription` records.

### 3.4 DatabaseSettingService

This service acts as a secure credentials vault. Because the AI layer must connect directly to each company's own database to run generated SQL queries, those connection credentials must be stored somewhere trusted. The `DbSettings` entity records the server address, database name, user, and password for a company's database — with the password stored encrypted at rest using **AES-256**.

A strict one-to-one constraint between a company and its settings is enforced at the service layer: the create operation checks for an existing record before inserting. Two response shapes exist by design — a safe, credential-free response for manager-facing endpoints, and a decrypted response served exclusively to the internal AI service endpoint.

**Key Entity:**

| Column | Type | Description |
|---|---|---|
| `Id` | GUID PK | Unique settings record |
| `CompanyId` | string | Logical FK → CompanyService |
| `Server`, `DbName`, `User` | string | Connection parameters |
| `Password` | string | **AES-256 encrypted at rest** |
| `Name`, `DbType` | string | Optional label and engine type |

### 3.5 MappingService

The MappingService is the bridge between a company's database schema and its organizational structure. It tracks which tables from the client's database are assigned to which team or department. Before the AI service generates a SQL query, it consults this service to determine which tables are in scope for the requesting team.

This is the only service using **PostgreSQL**. The choice is deliberate: the `client_schema_vectors` table is part of the AI embedding and vector workflow. PostgreSQL's native `jsonb` type supports structured schema metadata (table relationships stored as binary JSON), and its `text[]` array type allows department assignments to be stored directly on the table record without a separate join table. The `snake_case` naming convention on this table reflects its shared lineage with the AI/vector pipeline.

Department assignment works by appending or removing team names from the `team_name` array column. A single table can therefore be assigned to multiple departments simultaneously.

**Key Entity:**

| Column | DB Type | Description |
|---|---|---|
| `id` | integer (serial) PK | Auto-incrementing record ID |
| `company_id` | varchar(255) | Logical FK → CompanyService |
| `table_name` | varchar(255) | Name of the client database table |
| `table_description` | text | Optional human-readable description |
| `table_relations` | jsonb | JSON describing table relationships |
| `team_name` | text[] | Array of assigned department names |

A unique constraint on `(company_id, table_name)` prevents duplicate registrations.

### 3.6 TeamService

The TeamService manages teams and their membership within a company. Its data model is straightforward: a `Teams` table and a `TeamEmployees` junction table. Deleting a team cascades to remove all of its membership records. The `EmployeeId` stored in `TeamEmployees` is a logical reference to a user managed by AuthService; when job title data is needed, the TeamService calls AuthService via gRPC (`GetJobTitlesList`) rather than duplicating that data locally.

**Key Entities:**

| Entity | Primary Key | Notable Columns |
|---|---|---|
| `Teams` | `Id` (GUID) | `Name`, `CompanyId`, `TeamLeadId`, timestamps |
| `TeamEmployees` | `Id` (GUID) | `TeamId` (FK → Teams, cascade delete), `EmployeeId` |

### 3.7 PlanService

The PlanService manages operational and strategic plans associated with teams. A `Plan` record captures the goal, description, timeline, approval flag, and references to its owning company, team, and creator. Two supporting entities — `PlanType` and `Status` — are managed as configurable lookup tables, allowing administrators to define custom categories and lifecycle states without code changes.

**Key Entities:**

| Entity | Primary Key | Notable Columns |
|---|---|---|
| `Plan` | `Id` (GUID) | `Goal`, `Description`, `Duration`,`Status`, `PlanType`, `IsApproved`, `StartDate`, `EndDate`, `OwnerId`, `CompanyId`, `TeamId` |
| `PlanType` | `Id` (GUID) | `Name` (e.g., Strategic, Tactical) |
| `Status` | `Id` (GUID) | `Name` (e.g., Pending, In-Progress, Completed) |

The `Plan` entity stores status and plan type as string values (denormalized), referencing the lookup tables by name rather than by foreign key. This approach provides flexibility in managing configurable plan types and statuses while keeping the service loosely coupled and easy to extend. The service exposes gRPC endpoints (`GetPlan`, `isOwner`) consumed by CommentService and TaskService to validate plan existence and ownership.

### 3.8 NotificationService

The NotificationService handles both the persistence and delivery of notifications. It maintains two tables: `Notifications` for the notification records themselves, and `UserDeviceTokens` for FCM push notification tokens. Notifications are consumed asynchronously from a RabbitMQ queue via MassTransit and delivered through two parallel channels — SignalR for active web users, and Firebase Cloud Messaging for mobile users.

**Key Entities:**

| Entity | Primary Key | Notable Columns |
|---|---|---|
| `Notifications` | `Id` (GUID) | `Title`, `Message`, `SenderId`, `ReceiverId`, `PlanId`, `IsRead`, `CreatedAt` |
| `UserDeviceTokens` | `Id` (GUID) | `UserId`, `DeviceToken`, `DeviceType`, `UpdatedAt` |

### 3.9 ChatbotService

The ChatbotService logs every AI chatbot interaction for history and auditability. Each `ChatMessage` record stores the user's natural-language question alongside the AI's structured response, which is an embedded object containing both the natural-language answer and the generated SQL query. The service calls TeamService via gRPC before forwarding a query to the external AI Copilot, retrieving the team's `CompanyId` and name to scope the AI's context correctly.

**Key Entity:**

| Column | Type | Description |
|---|---|---|
| `Id` | GUID PK | Unique message record |
| `UserId` | string | Logical FK → AuthService User |
| `UserRole` | string | Role of user at query time |
| `UserQuestion` | string | Natural language input |
| `Response` | object | Embedded: `Answer` + `Sql_Query` |
| `TimeStamp` | datetime | When the message was saved |

### 3.10 CommentService

The CommentService stores user comments attached to plans. Before persisting a comment, the service validates the referenced plan's existence by calling PlanService via gRPC (`GetPlan`). The `UserId` is always sourced from the JWT, never from the request body, preventing comment spoofing. Edit and delete operations additionally verify authorship before permitting changes.

**Key Entity:**

| Column | Type | Description |
|---|---|---|
| `Id` | GUID PK | Unique comment identifier |
| `UserComment` | string | Comment body text |
| `UserId` | string | Logical FK → AuthService User |
| `PlanId` | string | Logical FK → PlanService Plan |
| `CreatedAt` | datetime | Creation timestamp |
| `UpdatedAt` | datetime? | Edit timestamp (null if unedited) |

### 3.11 InvitationService

The InvitationService manages the workflow for onboarding users into a company. An admin or manager issues an invitation specifying the invitee's email and intended role. The `Status` field tracks the invitation through its lifecycle (`Pending` → `Accepted` or `Rejected`), and the `IsActive` flag provides a soft-expiry mechanism that allows an invitation to be invalidated without deleting the audit trail. Upon acceptance, the AuthenticationService creates the user account with the specified role and `CompanyId`.

**Key Entity:**

| Column | Type | Description |
|---|---|---|
| `Id` | int PK (auto-increment) | Unique invitation record |
| `SenderId` | string | Logical FK → AuthService User |
| `CompanyId` | string | Logical FK → CompanyService |
| `Email` | string | Invitee's email address |
| `ReceiverRole` | string | Role to assign on acceptance |
| `Status` | enum | `Pending`, `Accepted`, `Rejected` |
| `IsActive` | bool | Whether the invitation is still usable |
| `CreatedAt` | datetime | Invitation creation timestamp |

### 3.12 ReportService

The ReportService records metadata about generated analytical reports without storing the binary file content in the database. Each `Report` record captures who generated it, for which team, over what period, and where the output file is located. Storing file paths rather than binary content keeps the database lightweight and allows file storage to scale independently of the database.

**Key Entity:**

| Column | Type | Description |
|---|---|---|
| `Id` | GUID PK | Unique report record |
| `TeamId` | string | Logical FK → TeamService Team |
| `UserId` | string | Logical FK → AuthService User |
| `Periodic` | enum | Report period (Weekly, Monthly, Quarterly) |
| `GeneratedDate` | datetime | Generation timestamp |
| `FileType` | string | Output format (PDF, Excel) |
| `FilePath` | string | Storage path or URL to the file |

### 3.13 TaskService

The TaskService manages tasks — referred to as "Quests" in the domain model — which represent the actionable units of work within a plan. The data model consists of two tables: `Quest`, which holds task details and a reference to its parent plan, and `UserQuests`, a junction table that assigns quests to individual users. This resolves the many-to-many relationship between tasks and users without duplication.

**Key Entities:**

| Entity | Primary Key | Notable Columns |
|---|---|---|
| `Quest` | `QuestId` (GUID) | `Title`, `Description`, `Status` (enum), `StartDate`, `DeadLine`, `PlanId` |
| `UserQuests` | `Id` (GUID) | `QuestId` (FK → Quest), `UserId` |

The `Quest.Status` field uses a typed enum (`QuestStatusEnum`) — for example, `Pending`, `InProgress`, `Completed` — providing structured lifecycle tracking at the database level.

---

## 4. Cross-Service Logical Relationships

Since each microservice owns its database independently, cross-service relationships are implemented as logical references rather than physical foreign keys.

The table below provides a complete reference for all such relationships in the system:

| Storing Service | Field | Referenced Service | Referenced Entity | Validation Mechanism |
|---|---|---|---|---|
| AuthService | `User.CompanyId` | CompanyService | `Company.Id` | gRPC Call |
| CompanyService | `Company.AdminId` | AuthService | `User.Id` | JWT Claims |
| CompanyService | `Company.SubscriptionId` | SubscriptionService | `Subscription.Id` | gRPC Call |
| DatabaseSettingService | `DbSettings.CompanyId` | CompanyService | `Company.Id` | JWT Claims |
| MappingService | `client_schema_vectors.company_id` | CompanyService | `Company.Id` | JWT Claims |
| MappingService | `client_schema_vectors.team_name[]` | TeamService | `Team.Name` | Service Logic |
| TeamService | `Teams.CompanyId` | CompanyService | `Company.Id` | JWT Claims |
| TeamService | `TeamEmployees.EmployeeId` | AuthService | `User.Id` | gRPC Call |
| PlanService | `Plan.OwnerId` | AuthService | `User.Id` | JWT Claims |
| PlanService | `Plan.CompanyId` | CompanyService | `Company.Id` | JWT Claims |
| PlanService | `Plan.TeamId` | TeamService | `Team.Id` | gRPC Call |
| NotificationService | `Notification.ReceiverId` | AuthService | `User.Id` | JWT Claims |
| NotificationService | `Notification.SenderId` | AuthService | `User.Id` | JWT Claims |
| NotificationService | `Notification.PlanId` | PlanService | `Plan.Id` | Service Logic |
| NotificationService | `UserDeviceTokens.UserId` | AuthService | `User.Id` | JWT Claims |
| ChatbotService | `ChatMessage.UserId` | AuthService | `User.Id` | JWT Claims |
| CommentService | `Comment.UserId` | AuthService | `User.Id` | JWT Claims |
| CommentService | `Comment.PlanId` | PlanService | `Plan.Id` | gRPC Call |
| InvitationService | `Invitation.SenderId` | AuthService | `User.Id` | JWT Claims |
| InvitationService | `Invitation.CompanyId` | CompanyService | `Company.Id` | JWT Claims |
| ReportService | `Report.UserId` | AuthService | `User.Id` | JWT Claims |
| ReportService | `Report.TeamId` | TeamService | `Team.Id` | JWT Claims / Logic |
| TaskService | `Quest.PlanId` | PlanService | `Plan.Id` | Service Logic |
| TaskService | `UserQuests.UserId` | AuthService | `User.Id` | JWT Claims |

Three primary validation approaches are used across the platform. The most common approach relies on JWT claims, where authenticated tokens provide trusted identity and tenant information without requiring synchronous service calls. gRPC validation is used in operations that require strict existence checks before data modification, such as validating plans, teams, or subscriptions. Finally, certain lightweight references are maintained through service-level logic where full consistency validation is not required.

---

## 5. Indexing & Performance Optimization

Database indexes in RecoMind were introduced selectively based on actual query patterns and high-frequency operations rather than being applied uniformly across all tables. The following table summarizes the most important indexes used within the system and their primary purpose:

| Service             | Table                   | Indexed Column(s)                       | Purpose                                              |
| ------------------- | ----------------------- | --------------------------------------- | ---------------------------------------------------- |
| AuthService         | `AspNetUsers`           | `NormalizedEmail`, `NormalizedUserName` | Fast authentication and user lookup                  |
| AuthService         | `AspNetUserRoles`       | `UserId`, `RoleId`                      | Efficient role resolution during authorization       |
| NotificationService | `Notifications`         | `ReceiverId`                            | Fast retrieval of user-specific notifications        |
| NotificationService | `UserDeviceTokens`      | `UserId`                                | Efficient device token lookup for push notifications |
| MappingService      | `client_schema_vectors` | `(company_id, table_name)`              | Optimized schema mapping and AI lookup operations    |
| TaskService         | `Quest`                 | `PlanId`                                | Fast retrieval of tasks associated with plans        |

The indexing strategy focuses primarily on frequently accessed tables and real-time operations, particularly within the notification, task management, and AI mapping workflows. This approach improves query performance while avoiding unnecessary indexing overhead on low-frequency operations.

---

## 6. Security & Multi-Tenancy

### Tenant Isolation

RecoMind is designed as a multi-tenant SaaS platform that serves multiple companies on shared infrastructure while maintaining strict data isolation. Tenant separation is enforced using the `CompanyId` claim embedded within authenticated JWT tokens. Each service extracts this claim from the verified token and applies it automatically during database operations, ensuring that users can access only data belonging to their own company.

In addition, sensitive identifiers such as `CompanyId` are always derived from authenticated tokens during write operations rather than client-provided input, preventing unauthorized cross-tenant access and data manipulation.

### Role-Based Authorization

Role-based authorization is implemented consistently across the platform using four primary roles: `Admin`, `Manager`, `TeamLeader`, and `Employee`. Access policies are applied at the service level to ensure that each user can access only the operations and resources permitted by their role.

| Policy       | Permitted Roles                              | Applied By                                                |
| ------------ | -------------------------------------------- | --------------------------------------------------------- |
| Admin only   | `admin`                                      | CompanyService, SubscriptionService                       |
| Management   | `admin`, `manager`                           | DatabaseSettingService, MappingService, InvitationService |
| Leadership   | `admin`, `manager`, `teamleader`             | TeamService                                               |
| AllEmployees | `admin`, `manager`, `teamleader`, `employee` | NotificationService, TaskService, ChatbotService          |

### Credential Security

Database credentials managed by the DatabaseSettingService are encrypted using AES-256 before storage. Sensitive connection information is exposed only through secured internal communication channels used by the AI services, while standard client-facing endpoints return metadata only without exposing credential values.

This separation reduces the risk of credential leakage and improves the overall security of the platform.

### Identity Integrity

Sensitive operations across the RecoMind platform rely exclusively on verified JWT claims as the source of identity and tenant context. Values such as `UserId` and `CompanyId` are extracted from authenticated tokens rather than client request bodies, preventing identity spoofing and unauthorized cross-tenant actions.

This approach is applied consistently across multiple services, including TeamService, CommentService, NotificationService, and plan/task management workflows, ensuring secure and reliable user attribution throughout the system.

---

## 7. Scalability Considerations

The RecoMind platform is designed with scalability and service independence as core architectural principles. Since each microservice manages its own database, scaling pressure in one domain does not directly affect the others, allowing services to scale independently based on workload requirements.

Horizontal scalability is supported through the stateless authentication model, where services rely on JWT claims instead of server-side sessions. This enables multiple service instances to operate behind load balancers without shared application state dependencies.

The platform also utilizes asynchronous communication through RabbitMQ and MassTransit, particularly within the notification workflow. This event-driven approach improves system responsiveness and prevents long-running operations from blocking other services.

Several database-level design decisions were also made to improve scalability and maintainability. For example, ReportService stores file paths instead of binary report files to keep the database lightweight and allow external storage systems to scale independently. In addition, MappingService leverages PostgreSQL features such as `jsonb` and array-based structures to efficiently support AI-driven schema mapping and embedding workflows.

Overall, the distributed database architecture of RecoMind provides a flexible and scalable foundation capable of supporting future system growth and increasing operational demands.

---

## 8. Conclusion

The database architecture of RecoMind is designed around service autonomy and data encapsulation principles. By adopting a database-per-service approach, each microservice independently manages its own data and business logic, enabling loose coupling, scalability, and independent service evolution.

Cross-service consistency is maintained through JWT-based identity propagation, gRPC validation calls, and asynchronous communication using RabbitMQ and MassTransit. This approach allows services to remain independent while ensuring reliable coordination across the platform.

The system also applies performance-oriented optimizations through selective indexing on high-frequency queries and real-time operations, particularly in NotificationService, MappingService, and TaskService.

In addition, the use of PostgreSQL within the MappingService highlights the flexibility of the architecture, where different database technologies can be selected based on service requirements and AI workflow needs.

Overall, the RecoMind data layer provides a scalable, maintainable, and distributed architecture that effectively supports the platform’s AI-driven business intelligence and decision-support capabilities.

---

*RecoMind — Graduation Project | Database Design & Architecture Chapter*