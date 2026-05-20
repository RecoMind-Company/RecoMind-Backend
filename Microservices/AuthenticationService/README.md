# ?? AuthenticationService — RecoMind

> Part of the **RecoMind** Project · ASP.NET Core Microservices Architecture

---

## 1. Service Overview

The **AuthenticationService** is the central hub for user identity management, JWT token generation, and account provisioning in the RecoMind platform. It manages user registration across multiple roles (Admin, Manager, TeamLeader, Employee), handles secure authentication via email and password, issues short-lived JWT tokens paired with refresh tokens for seamless session management, and exposes gRPC endpoints for internal service-to-service communication.

### Main Responsibilities

- Register users with role-based differentiation (Admin, Manager, TeamLeader, Employee)
- Authenticate users via email and password verification
- Generate and validate JWT access tokens (short-lived, ~2 hours)
- Generate and manage refresh tokens (long-lived, 2 days) for token rotation
- Send verification codes via email for password reset workflows
- Reset and create new passwords securely
- Retrieve user profiles with associated job titles
- Validate user invitations before login for non-admin roles
- Expose public profiles (email, ID, name, job title) via REST and gRPC
- Coordinate with Invitation, Team, and Company services for role-based access control

---

## 2. Technologies Used

| Category | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| Language | C# (.NET 8) |
| Database | SQL Server (via Entity Framework Core 8) |
| ORM | Entity Framework Core 8 |
| Authentication | JWT Bearer Tokens + Identity Framework |
| Inter-Service (Server) | gRPC |
| Password Security | BCrypt (via ASP.NET Identity), AES Encryption for tokens |
| Cryptography | RNGCryptoServiceProvider for refresh tokens, MD5/HMAC for verification |
| Object Mapping | Manual DTO mapping (extensible to AutoMapper) |
| Email Service | SMTP (Gmail) with custom verification code delivery |
| Unit Testing | xUnit, Moq (infrastructure ready) |
| API Documentation | Swagger / OpenAPI (Swashbuckle) |
| Containerization | Docker |

---

## 3. Project Structure

```
AuthenticationService/
??? Authentication.Core/                  # Domain layer
?   ??? Constants/
?   ?   ??? Roles.cs                      # Role definitions (Admin, Manager, TeamLeader, Employee)
?   ?   ??? InvitationStatus.cs
?   ??? DTOs/                             # Data Transfer Objects
?   ?   ??? RegisterDto.cs                # Input for user registration
?   ?   ??? LoginDto.cs                   # Input for login
?   ?   ??? AuthenticationDto.cs          # Response with token & user info
?   ?   ??? BasePasswordDto.cs            # Input for password operations
?   ?   ??? ResetPasswordDto.cs           # Extends BasePasswordDto + OldPassword
?   ?   ??? ForgetPasswordDto.cs          # Input for password recovery
?   ?   ??? UserToReturnDto.cs            # User profile (public-safe)
?   ?   ??? UsersToReturnDto.cs           # Multiple users response
?   ?   ??? ProfileDto.cs                 # User profile edit input
?   ?   ??? ProfileToReturnDto.cs         # User profile response
?   ?   ??? JobTitlesDto.cs               # Job title info
?   ?   ??? VerifyCodeDto.cs              # Code verification input
?   ?   ??? TeamDto.cs                    # Team reference
?   ?   ??? BaseToReturnDto.cs            # Generic success response
?   ??? Helpers/
?   ?   ??? SecurePasswordGenerator.cs    # Random password generation for non-admin users
?   ??? Interfaces/
?   ?   ??? IAuthenticationService.cs     # Authentication service contract
?   ?   ??? IAccountService.cs            # Account management contract
?   ?   ??? IVerificationEmailService.cs  # Email verification contract
?   ?   ??? IEmailSender.cs               # Email sending contract
?   ?   ??? IVerificationService.cs       # Code verification contract
?   ?   ??? IGrpcInvitationService.cs     # Invitation service client contract
?   ?   ??? IGrpcTeamService.cs           # Team service client contract
?   ?   ??? IGrpcCompanyService.cs        # Company service client contract
?   ?   ??? IGenericRepository.cs         # Generic CRUD contract
?   ?   ??? IUnitOfWork.cs                # Unit of Work pattern contract
?   ??? Models/
?   ?   ??? AppUser.cs                    # Domain user entity (extends IdentityUser)
?   ?   ??? RefreshToken.cs               # Refresh token entity
?   ?   ??? UsersJobTitle.cs              # User-JobTitle mapping
?   ?   ??? VerificationCode.cs           # Verification code storage
?   ??? Services/
?   ?   ??? AuthenticationService.cs      # Core auth logic (register, login, tokens)
?   ?   ??? AccountService.cs             # User profile management
?   ?   ??? VerificationEmailService.cs   # Email verification logic
?   ?   ??? VerificationService.cs        # Code validation logic
?   ??? Settings/
?   ?   ??? JwtSettings.cs                # JWT configuration
?   ?   ??? EmailConfgSettings.cs         # Email SMTP configuration
?   ?   ??? PhotoSettings.cs              # Photo upload settings
?   ??? Extensions/
?       ??? CoreServicesExtension.cs      # DI registration
?
??? Authentication.Infrastructure/        # Data access layer
?   ??? Context/
?   ?   ??? AuthenticationDbContext.cs    # EF Core DbContext
?   ??? Repository/
?   ?   ??? GenericRepository.cs          # Generic CRUD repository
?   ??? UnitOfWork/
?   ?   ??? UnitOfWork.cs                 # Unit of Work implementation
?   ??? Migrations/
?   ?   ??? 20251031200851_Init.cs
?   ?   ??? 20251211231939_AddUsersJobTitlesTable.cs
?   ??? EmailSender.cs                    # SMTP email sending implementation
?   ??? DataSeeding.cs                    # Initial data seeding (roles, default users)
?   ??? gRPC/
?   ?   ??? GrpcInvitationService.cs      # Invitation service client
?   ?   ??? CompanyGrpc/
?   ?   ?   ??? GrpcCompanyServiceImp.cs  # Company service client
?   ?   ??? TeamGrpc/
?   ?       ??? GrpcTeamService.cs        # Team service client
?   ??? Extensions/
?       ??? InfrastructureServicesExtension.cs  # DI registration
?
??? RecoMindAuthenticationAPI/            # Presentation layer
?   ??? Controllers/
?   ?   ??? AuthenticationController.cs   # REST API: register, login, tokens, password
?   ?   ??? AccountController.cs          # REST API: profile management
?   ??? Grpc/
?   ?   ??? Authentication/
?   ?   ?   ??? Service/
?   ?   ?       ??? GrpcAuthenticationService.cs  # gRPC: token & user queries
?   ?   ??? Account/
?   ?       ??? Service/
?   ?       ?   ??? GrpcAccountService.cs        # gRPC: user profile queries
?   ?       ?   ??? GrpcFormFile.cs
?   ?       ??? AccountService.proto
?   ?       ??? Service.proto
?   ??? Extensions/
?   ?   ??? ApiServicesExtension.cs       # Swagger, middleware setup
?   ??? Program.cs                        # Application bootstrap
?   ??? appsettings.json                  # Configuration
?   ??? Properties/
?   ?   ??? launchSettings.json
?   ??? Dockerfile
?
??? Authentication.Tests/                 # Unit tests
?   ??? (infrastructure-ready)
?
??? AuthenticationService.sln

```

---

## 4. API Endpoints

> **Base URL:** `https://api.recomind.site/api`

---

### 4.1 User Registration

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/authentication/register` |
| **Auth** | None |
| **Description** | Registers a new user. For Admin role, a password is required and a verification email is sent. For other roles (Manager, TeamLeader, Employee), a secure random password is generated and sent via email. |

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "fullName": "John Doe",
  "role": "employee"
}
```

> `email` and `role` are required. `password` is required for Admin; auto-generated for others. `fullName` is optional (auto-extracted from email for non-admin).

**Response Example (200 OK):**

```json
{
  "name": "John Doe",
  "email": "user@example.com",
  "isAuthenticated": true,
  "message": "completed sucessfully!",
  "experiesOn": "2026-01-17T16:45:00Z",
  "roles": ["employee"],
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "4j9...base64...",
  "refreshTokenExp": "2026-01-19T16:45:00Z",
  "userId": "user-uuid"
}
```

**Response (400 Bad Request — user exists):**

```json
{
  "message": "This user is already registered!"
}
```

**Response (400 Bad Request — invalid role):**

```json
{
  "message": "Role is not valid!"
}
```

---

### 4.2 User Login

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/authentication/login` |
| **Auth** | None |
| **Description** | Authenticates a user by email and password. For non-Admin roles, validates that the user has a valid invitation from the Invitation Service. Returns a JWT token and refresh token in cookies. |

**Request Body:**

```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response Example (200 OK):**

```json
{
  "name": "John Doe",
  "email": "user@example.com",
  "isAuthenticated": true,
  "message": "login successfully",
  "experiesOn": "2026-01-17T16:45:00Z",
  "roles": ["employee"],
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "4j9...base64...",
  "refreshTokenExp": "2026-01-19T16:45:00Z"
}
```

**Response (400 Bad Request — auth failed):**

```json
{
  "message": "email or password is incorrect"
}
```

**Response (400 Bad Request — invalid invitation):**

```json
{
  "message": "You do not have a valid invitation"
}
```

---

### 4.3 Refresh Token

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/authentication/refresh-token` |
| **Auth** | None (reads from cookies) |
| **Description** | Rotates the refresh token. Revokes the old token, generates a new one, and returns a new JWT access token. |

**Response Example (200 OK):**

```json
{
  "isAuthenticated": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "experiesOn": "2026-01-17T17:45:00Z",
  "refreshToken": "5k0...base64...",
  "refreshTokenExp": "2026-01-20T17:45:00Z",
  "email": "user@example.com",
  "name": "John Doe",
  "roles": ["employee"],
  "message": "refresh token created successfully"
}
```

**Response (400 Bad Request):**

```json
{
  "message": "Invalid token" or "Inactive token"
}
```

---

### 4.4 Forget Password

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/authentication/forget-password` |
| **Auth** | None |
| **Description** | Initiates the password recovery process by sending a verification code to the user's email. |

**Request Body:**

```json
{
  "email": "user@example.com"
}
```

**Response Example (200 OK):**

```json
{
  "success": true,
  "message": "Email send successfully"
}
```

**Response (400 Bad Request):**

```json
{
  "message": "Email Is NotFound"
}
```

---

### 4.5 Verify Code

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/authentication/verify` |
| **Auth** | None |
| **Description** | Verifies the code sent to the user's email. This code is valid for a limited time. |

**Request Body:**

```json
{
  "email": "user@example.com",
  "code": "123456"
}
```

**Response Example (200 OK):**

```json
{
  "success": true,
  "message": "Code is valid"
}
```

**Response (400 Bad Request):**

```json
{
  "message": "Code is not valid or expired"
}
```

---

### 4.6 Reset Password (Authenticated)

| Field | Value |
|---|---|
| **Method** | `POST` |
| **Route** | `/api/authentication/reset-password` |
| **Auth** | Required — JWT Bearer token |
| **Description** | Resets the password for an authenticated user by verifying the old password and setting a new one. |

**Request Body:**

```json
{
  "oldPassword": "OldPassword123!",
  "newPassword": "NewPassword456!",
  "confirmNewPassword": "NewPassword456!"
}
```

**Response Example (200 OK):**

```json
{
  "success": true,
  "message": "The password Updated successfully"
}
```

**Response (400 Bad Request):**

```json
{
  "message": "Password is not correct"
}
```

---

### 4.7 Create New Password (After Verification)

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/authentication/update-password?email=user@example.com` |
| **Auth** | None |
| **Description** | Creates a new password after email verification. Called as step 3 of the forget-password workflow. |

**Request Body:**

```json
{
  "newPassword": "NewPassword456!",
  "confirmNewPassword": "NewPassword456!"
}
```

**Response Example (200 OK):**

```json
{
  "success": true,
  "message": "The password Changed successfully"
}
```

---

### 4.8 Get User Profile

| Field | Value |
|---|---|
| **Method** | `GET` |
| **Route** | `/api/users/getProfile` |
| **Auth** | Required — JWT Bearer token |
| **Description** | Returns the authenticated user's profile including name, email, photo, and job title. |

**Response Example (200 OK):**

```json
{
  "id": "user-uuid",
  "name": "John Doe",
  "email": "user@example.com",
  "photoUrl": "/UserProfileImage/john-doe.jpg",
  "jobTitle": "Software Engineer"
}
```

**Response (404 Not Found):**

```json
{
  "message": "this user is not found"
}
```

---

### 4.9 Update User Profile

| Field | Value |
|---|---|
| **Method** | `PUT` |
| **Route** | `/api/users/updateProfile` |
| **Auth** | Required — JWT Bearer token |
| **Content-Type** | `multipart/form-data` |
| **Description** | Updates the user's profile including name and profile photo upload. |

**Request Body (Form Data):**

```
fullName: "John Doe Updated"
photo: <binary-file>
```

**Response Example (200 OK):**

```json
{
  "success": true,
  "message": "Profile updated successfully"
}
```

---

### 4.10 Delete User Account

| Field | Value |
|---|---|
| **Method** | `DELETE` |
| **Route** | `/api/users/{userId}` |
| **Auth** | Required — JWT Bearer token |
| **Description** | Permanently deletes a user account. |

**Response:** HTTP 200 OK

```json
{
  "success": true,
  "message": "User deleted successfully"
}
```

---

## 5. Database Design

### Table: `AspNetUsers` (extended by AppUser)

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `UserName` | `nvarchar(256)` | Extracted from email |
| `NormalizedUserName` | `nvarchar(256)` | Indexed |
| `Email` | `nvarchar(256)` | Unique, indexed |
| `NormalizedEmail` | `nvarchar(256)` | Indexed |
| `FullName` | `nvarchar(max)` | User's display name |
| `PhotoUrl` | `nvarchar(max)` | Optional profile photo |
| `IsActive` | `bit` | Soft-delete flag |
| `CreatedOn` | `datetime2` | Account creation timestamp |
| `PasswordHash` | `nvarchar(max)` | **BCrypt hashed (via Identity)** |
| `SecurityStamp` | `nvarchar(max)` | For concurrency / token revocation |
| `ConcurrencyStamp` | `nvarchar(max)` | For EF Core optimistic locking |
| `PhoneNumber` | `nvarchar(max)` | Optional |
| `PhoneNumberConfirmed` | `bit` | Default: 0 |
| `TwoFactorEnabled` | `bit` | Default: 0 |
| `LockoutEnd` | `datetimeoffset` | Account lockout expiry |
| `LockoutEnabled` | `bit` | Default: 0 |
| `AccessFailedCount` | `int` | Failed login attempts |

### Table: `RefreshTokens` (owned by AppUser)

| Column | Type | Notes |
|---|---|---|
| `AppUserId` | `nvarchar(450)` | FK to AspNetUsers |
| `Token` | `nvarchar(max)` | Base64-encoded random bytes |
| `CreatedOn` | `datetime2` | Token creation time |
| `ExpiresOn` | `datetime2` | Expiration (typically +2 days) |
| `RevokeOn` | `datetime2` | Nullable; set when rotated |
| `IsActive` | `computed` | `ExpiresOn > UtcNow AND RevokeOn IS NULL` |

### Table: `UsersJobTitles`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK, GUID string |
| `UserId` | `nvarchar(450)` | FK to AspNetUsers |
| `JobTitle` | `nvarchar(max)` | Job title string |

### Table: `AspNetRoles`

| Column | Type | Notes |
|---|---|---|
| `Id` | `nvarchar(450)` | PK |
| `Name` | `nvarchar(256)` | `admin`, `manager`, `teamleader`, `employee` |
| `NormalizedName` | `nvarchar(256)` | Indexed |
| `ConcurrencyStamp` | `nvarchar(max)` | EF Core optimistic locking |

### Table: `AspNetUserRoles`

| Column | Type | Notes |
|---|---|---|
| `UserId` | `nvarchar(450)` | FK to AspNetUsers |
| `RoleId` | `nvarchar(450)` | FK to AspNetRoles |

### Table: `VerificationCodes`

| Column | Type | Notes |
|---|---|---|
| `Id` | `int` | PK, auto-increment |
| `Email` | `nvarchar(max)` | User's email |
| `Code` | `nvarchar(max)` | Verification code (typically 6 digits) |
| `IsActive` | `bit` | Active/expired flag |

---

## 6. Authentication & Security

| Aspect | Detail |
|---|---|
| **Mechanism** | JWT Bearer Token (short-lived) + Refresh Token (long-lived) |
| **Access Token Lifespan** | ~2 hours (configurable via `JwtSettings.DurationInHours`) |
| **Refresh Token Lifespan** | 2 days |
| **Token Encoding** | HS256 (HMAC SHA-256) |
| **Secret Key** | 256-bit base64-encoded key from `appsettings.json` |
| **Claims** | Email, NameIdentifier (UserId), Name (Username), Roles, CompanyId (if applicable) |
| **Password Hashing** | BCrypt (via ASP.NET Identity) |
| **Refresh Token Generation** | RNGCryptoServiceProvider (cryptographically secure random) |
| **HTTPS** | Required in production |
| **Refresh Token Storage** | HTTP-only cookies (cannot be accessed by JavaScript) |

### Authorization Policies

| Role | Permissions |
|---|---|
| `admin` | Full system access, can manage all users and settings |
| `manager` | Team and user management within their company |
| `teamleader` | Team member management, task oversight |
| `employee` | Own profile access, task execution |

### JWT Claims Example

```
{
  "jti": "c1234567-89ab-cdef-0123-456789abcdef",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "user@example.com",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "user-uuid",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "username",
  "CompanyId": "company-uuid",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "employee",
  "iat": 1705517100,
  "exp": 1705524300,
  "iss": "https://api.recomind.site",
  "aud": "recomind-app"
}
```

---

## 7. Service Communication

### Inbound gRPC Services

The AuthenticationService exposes two gRPC services for internal consumption:

#### 7.1 GrpcAuthenticationService

| gRPC Method | Request | Response | Description |
|---|---|---|---|
| `GetUserById` | `UserId` | `UserResponse` | Returns user's public profile (ID, name, email, job title) |
| `GetUsersByIds` | `List<UserId>` | `UsersListResponse` | Batch retrieve users |
| `GetUserEmail` | `UserId` | `EmailResponse` | Returns user's email |

**Proto Response Shape:**

```protobuf
message UserResponse {
  string id = 1;
  string name = 2;
  string email = 3;
  string jobTitle = 4;
}

message UsersListResponse {
  repeated UserResponse users = 1;
}
```

#### 7.2 GrpcAccountService

| gRPC Method | Request | Response | Description |
|---|---|---|---|
| `GetUserProfile` | `UserId` | `UserProfileResponse` | Returns full profile including photo URL |
| `UpdateUserPhoto` | `UserId, PhotoFile` | `StatusResponse` | Updates user's profile photo |

### Outbound gRPC Service Calls

The AuthenticationService **calls** these services:

| Service | Method | Purpose |
|---|---|---|
| **CompanyService** | `GetCompanyByUserId(userId)` | Fetch company ID for Admin users |
| **TeamService** | `GetTeamByUserId(userId)` | Fetch team ID for non-Admin users |
| **InvitationService** | `LoginAttempt(email)` | Validate non-Admin user login (must have valid invitation) |

---

## 8. Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "email-config": {
    "Host": "smtp.gmail.com",
    "Port": "587"
  },
  "Urls": {
    "BaseUrl": "https://api.recomind.site/api/users",
    "VirtualPathUrl": "/UserProfileImage",
    "InvitationServiceUrl": "http://invitation-svc:5006",
    "TeamServiceUrl": "http://team-svc:5010",
    "CompanyServiceUrl": "http://company-svc:5007"
  },
  "AllowedHosts": "*"
}
```

### Environment Variables (from secrets or Kubernetes)

```bash
# JWT Settings
JWT_SECRET_KEY=your-256-bit-base64-encoded-key
JWT_ISSUER=https://api.recomind.site
JWT_AUDIENCE=recomind-app
JWT_DURATION_HOURS=2

# Database
CONNECTION_STRING=Server=sql-server;Database=AuthenticationDb;User Id=sa;Password=...

# Email
GMAIL_EMAIL=your-gmail@gmail.com
GMAIL_PASSWORD=your-app-password

# Service URLs
INVITATION_SERVICE_URL=http://invitation-svc:5006
TEAM_SERVICE_URL=http://team-svc:5010
COMPANY_SERVICE_URL=http://company-svc:5007

# Ports
HTTP_PORT=8000
GRPC_PORT=5000
```

---

## 9. Architecture Notes

### Clean Architecture Layers

- **Core** — Domain entities, DTOs, interfaces, business logic (no infrastructure dependencies)
- **Infrastructure** — EF Core DbContext, repositories, database migrations, external service clients
- **Presentation** — Controllers, gRPC services, DI wiring, middleware

### Key Design Patterns

#### 1. **Repository Pattern**
`GenericRepository<T>` abstracts database access. Clients depend on `IGenericRepository<T>` interface, enabling easy mocking in tests.

#### 2. **Unit of Work Pattern**
`IUnitOfWork<T>` groups repositories and provides a single SaveChanges operation, ensuring transactional consistency.

#### 3. **Dependency Injection**
All services are registered in DI container. Service constructors declare dependencies explicitly; the framework provides them.

#### 4. **DTO Pattern**
DTOs isolate the API contract from domain models. Changes to the database schema don't affect client code.

#### 5. **gRPC Client Pattern**
Internal service clients (`GrpcTeamService`, `GrpcCompanyService`, `GrpcInvitationService`) are registered as singletons and retry-aware.

### Role-Based Access Control (RBAC)

- **Admin** — Direct login, full system access. No invitation validation.
- **Manager/TeamLeader/Employee** — Login requires valid invitation from `InvitationService`. Auto-provisioned with secure random password.

### Refresh Token Rotation

Every time a refresh token is used:
1. The old token is marked as revoked (`RevokeOn = UtcNow`)
2. A new refresh token is generated and added to the user's token collection
3. The new token is sent in cookies

This pattern prevents token reuse: if an old token is stolen and replayed, the service will detect it (revoked) and reject it.

### Email Verification Code Workflow

1. User calls `/forget-password` ? code is generated and emailed
2. User calls `/verify` ? validates the code (checks `IsActive` and timestamp)
3. User calls `/update-password` ? hashes and stores new password

This ensures password reset requests are initiated by the actual email owner.

### Two-Service Coordination

For non-Admin login:
1. Service calls `InvitationService.LoginAttempt(email)` ? confirms user was invited
2. If valid, calls `TeamService.GetTeamByUserId(userId)` ? retrieves team ID
3. Embeds `CompanyId` claim in JWT ? enables downstream services to validate authorization

---

## 10. Running the Service

### Prerequisites

- .NET 8 SDK
- SQL Server 2019+ or use Docker
- Docker (for containerized deployment)

### Local Development

```bash
# Clone and navigate
cd Microservices/AuthenticationService

# Restore dependencies
dotnet restore

# Apply migrations to database
dotnet ef database update -p Authentication.Infrastructure

# Run the service
dotnet run --project RecoMindAuthenticationAPI

# Swagger UI available at: https://localhost:7001/swagger/index.html
```

### Docker Deployment

```bash
# Build image
docker build -t authentication-service:latest -f RecoMindAuthenticationAPI/Dockerfile .

# Run container
docker run -e CONNECTION_STRING="Server=sql-server;Database=AuthenticationDb;..." \
           -e JWT_SECRET_KEY="..." \
           -e GMAIL_EMAIL="..." \
           -e GMAIL_PASSWORD="..." \
           -p 8000:8080 \
           -p 5000:5000 \
           authentication-service:latest
```

### Kubernetes Deployment

A `deployment.yaml` should define:
- Pod with the authentication service image
- ConfigMap for non-sensitive settings (JWT issuer, audience, service URLs)
- Secret for sensitive data (JWT secret, database connection string, email credentials)
- Service exposing port 8000 (REST) and 5000 (gRPC)
- HorizontalPodAutoscaler for auto-scaling

---

## 11. Testing

### Unit Tests (xUnit)

Located in `Authentication.Tests/`:

```bash
dotnet test Authentication.Tests
```

Test coverage includes:
- `AuthenticationServiceTests` — register, login, token generation
- `AccountServiceTests` — profile management
- `VerificationServiceTests` — email code validation

### Integration Tests (Planned)

- Database seeding and migrations
- gRPC service-to-service communication
- SMTP email delivery

### Manual Testing

Use Swagger UI at `/swagger/index.html` to test all endpoints interactively.

---

## 12. Troubleshooting

| Issue | Cause | Solution |
|---|---|---|
| `InvalidOperationException: No database provider configured` | Missing `appsettings.json` or connection string | Ensure `CONNECTION_STRING` env var or connection string in `appsettings.json` |
| `401 Unauthorized` on protected endpoints | Missing or expired JWT token | Call `/refresh-token` to refresh, or login again |
| `Email not received` | SMTP configuration incorrect | Verify `GMAIL_EMAIL`, `GMAIL_PASSWORD`, and firewall rules for port 587 |
| `User already registered` | Email already exists | Try a different email or use `/login` |
| `Cannot validate refresh token` | Token expired or revoked | User must login again |
| `Invalid role` on register | Role name misspelled | Use: `admin`, `manager`, `teamleader`, or `employee` |

---

## 13. Future Enhancements

- [ ] Multi-factor authentication (MFA) via SMS or authenticator app
- [ ] OAuth 2.0 / OpenID Connect for federated login
- [ ] Social login (Google, Microsoft, GitHub)
- [ ] Audit logging for security events
- [ ] Session management (track active sessions, force logout)
- [ ] Rate limiting on login attempts to prevent brute force
- [ ] Custom password policies (strength requirements)
- [ ] User deactivation without deletion
- [ ] LDAP/Active Directory integration for enterprise

---

*RecoMind Project — AuthenticationService*
*Last Updated: January 2025*
