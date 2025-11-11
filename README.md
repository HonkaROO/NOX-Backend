# NOX-Backend

NPAX-Onboarding eXpert Chatbot (Backend) - ASP.NET Core 9.0 RESTful Web API

This is the backend API for the NPAX-Onboarding eXpert Chatbot, providing user authentication, identity management, role-based access control, and department organization. Built with ASP.NET Core 9.0, Entity Framework Core, and SQL Server.

**Technology Stack:** ASP.NET Core 9.0 • Entity Framework Core 9.0 • SQL Server 2019 • ASP.NET Core Identity • NSwag/Swagger

---

## Table of Contents

- [Quick Start](#quick-start)
- [Setup Instructions](#setup-instructions)
- [Development](#development)
- [API Documentation](#api-documentation)
- [Authentication & Authorization](#authentication--authorization)
- [Departments Management](#departments-management)
- [Common Scenarios](#common-scenarios)
- [Troubleshooting](#troubleshooting)

---

## Quick Start

### Prerequisites

- .NET 9.0 SDK or later
- Docker (for SQL Server)
- VS Code with REST Client extension (recommended for API testing)

### 1. Start SQL Server Container

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Chang123!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```

### 2. Build and Run

```bash
# Restore dependencies and build
dotnet build

# Apply database migrations (creates tables and seeds defaults)
dotnet ef database update

# Run the application (HTTP port 5164)
dotnet run --launch-profile http
```

The API will be available at **`http://localhost:5164`**

Access Swagger UI at: **`http://localhost:5164/swagger/index.html`**

### 3. Login with Default Credentials

**SuperAdmin Account (created automatically):**
- Email: `superadmin@nox.local`
- Password: `SuperAdmin@2024!Nox`

---

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK or later
- Docker (for SQL Server)
- Docker Desktop or Docker Engine

### 1. Start SQL Server Container

Run the following command to start a SQL Server 2019 container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Chang123!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```

**Parameters:**

- `ACCEPT_EULA=Y` - Accept SQL Server license agreement
- `SA_PASSWORD=Change123!` - Set system admin password (matches `appsettings.Development.json`)
- `-p 1433:1433` - Map container port 1433 to host port 1433
- `--name sqlserver` - Name the container "sqlserver"
- `-d` - Run in detached mode (background)
- `mcr.microsoft.com/mssql/server:2019-latest` - Use SQL Server 2019 image

**⚠️ Important:** The password must match the connection string in `appsettings.Development.json`.

### 2. Verify SQL Server is Running

```bash
docker ps
```

You should see the `sqlserver` container in the list.

### 3. Build and Run the Application

```bash
# Restore dependencies and build
dotnet build

# Apply database migrations (creates tables)
dotnet ef database update

# Run the application (HTTPS profile - port 7238)
dotnet run --launch-profile https

# OR run with HTTP only (development - port 5164)
dotnet run --launch-profile http
```

The API will be available at:

- **HTTP (Development):** `http://localhost:5164`
- **HTTPS (Production):** `https://localhost:7238`
- **Swagger UI:** `http://localhost:5164/swagger/index.html` (HTTP, development only)

### 4. Stopping the SQL Server Container

```bash
# Stop the container
docker stop sqlserver

# Remove the container (if you want to start fresh)
docker rm sqlserver

# Restart stopped container
docker start sqlserver
```

---

## Development

### Common Commands

```bash
# Build the project
dotnet build

# Run with HTTP (development - recommended for testing)
dotnet run --launch-profile http

# Run with HTTPS (production-like)
dotnet run --launch-profile https

# Run in watch mode (auto-rebuild on file changes)
dotnet watch run

# Database migrations
dotnet ef migrations add MigrationName    # Create new migration
dotnet ef database update                  # Apply pending migrations
dotnet ef migrations list                  # View migration history
dotnet ef migrations remove                # Remove last migration
```

### Testing API Endpoints

**Option 1: REST Client (Recommended)**
- Install "REST Client" extension in VS Code (by Huachao Mao)
- Open `NOX-Backend.http` file in project root
- Click "Send Request" links to execute requests
- Cookies are automatically handled (like a browser)

**Option 2: Swagger UI**
- Navigate to `http://localhost:5164/swagger/index.html`
- Login first via `/api/authentication/login` endpoint
- Test subsequent endpoints with automatic cookie inclusion

**Option 3: curl**
```bash
# Login and save cookies
curl -X POST http://localhost:5164/api/authentication/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{"email":"superadmin@nox.local","password":"SuperAdmin@2024!Nox"}'

# Use cookies in subsequent requests
curl -X GET http://localhost:5164/api/authentication/me \
  -b cookies.txt
```

---

## API Documentation

Complete endpoint documentation is available in the `/Documentation` folder. Here's a quick reference:

### [Authentication Controller](./Documentation/AUTHENTICATION_CONTROLLER.md)

**Route:** `/api/authentication`

User authentication, logout, and profile management using cookie-based authentication.

| Endpoint | Method | Auth Required | Description |
|----------|--------|:-------------:|-------------|
| `/login` | POST | ✗ | Login with email/password; sets authentication cookie |
| `/me` | GET | ✓ | Get current user profile |
| `/me` | PUT | ✓ | Update current user profile (name, phone, address) |
| `/logout` | POST | ✓ | Logout and clear authentication cookie |
| `/access-denied` | GET | ✗ | Returns 403 Forbidden for access denied scenarios |

**Authentication:** Cookie-based (HttpOnly, 7-day expiration with sliding window)

**Example Login:**
```bash
curl -X POST http://localhost:5164/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"email":"superadmin@nox.local","password":"SuperAdmin@2024!Nox"}'
```

---

### [User Management Controller](./Documentation/USER_MANAGEMENT_CONTROLLER.md)

**Route:** `/api/usermanagement`

**Authorization:** Requires `SuperAdmin` or `Admin` role

User account creation and administration. SuperAdmin can manage all users; Admin can only manage User-role accounts.

| Endpoint | Method | Role | Description |
|----------|--------|:----:|-------------|
| `/` | GET | SA/A | Get all users (filtered by role) |
| `/dashboard/statistics` | GET | SA/A | Get total employee and department counts |
| `/{userId}` | GET | SA/A | Get specific user |
| `/` | POST | SA/A | Create new user |
| `/{userId}` | PUT | SA/A | Update user information |
| `/{userId}` | DELETE | SA/A | Deactivate user |
| `/{userId}/reset-password` | POST | SA/A | Reset user password |

**Required Fields for Create User:**
- `userName`, `email`, `password`, `firstName`, `lastName`, `departmentId`

**Password Requirements:**
- Minimum 8 characters
- Must include uppercase, lowercase, digit, and special character

**Example Create User:**
```bash
curl -X POST http://localhost:5164/api/usermanagement \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe",
    "departmentId": 2
  }'
```

---

### [Role Management Controller](./Documentation/ROLE_MANAGEMENT_CONTROLLER.md)

**Route:** `/api/rolemanagement`

**Authorization:** Requires `SuperAdmin` role

Role management and user role assignments.

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Get all available roles |
| `/user/{userId}` | GET | Get user's assigned roles |
| `/user/{userId}/assign` | POST | Assign role to user |
| `/user/{userId}/remove/{roleName}` | DELETE | Remove role from user |
| `/{roleName}/users` | GET | Get all users with specified role |

**Three-Tier Role Hierarchy:**
- **SuperAdmin** - Full system access (manage all users and roles)
- **Admin** - Moderate system access (manage only User-role accounts)
- **User** - Standard user (view/update own profile only)

**Example Assign Role:**
```bash
curl -X POST http://localhost:5164/api/rolemanagement/user/user-id-123/assign \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"roleName":"Admin"}'
```

---

### [Department Controller](./Documentation/DEPARTMENT_CONTROLLER.md)

**Route:** `/api/departments`

**Authorization:** All endpoints require authentication; create/update/delete require `SuperAdmin` or `Admin` role (delete SuperAdmin only)

Department management and organization of users.

| Endpoint | Method | Role | Description |
|----------|--------|:----:|-------------|
| `/` | GET | Any | Get all departments |
| `/{id}` | GET | Any | Get specific department |
| `/` | POST | SA/A | Create new department |
| `/{id}` | PUT | SA/A | Update department |
| `/{id}/manager` | PUT | SA/A | Assign manager to department |
| `/{id}` | DELETE | SA | Soft-delete department |

**Default Departments** (auto-created):
1. Unassigned
2. Engineering
3. Human Resources
4. Sales
5. Support

**Example Create Department:**
```bash
curl -X POST http://localhost:5164/api/departments \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Marketing",
    "description": "Marketing and communications team"
  }'
```

---

### [CORS Setup Guide](./Documentation/CORS_SETUP.md)

**Configuration:** CORS for React/Vite Frontend Development

CORS (Cross-Origin Resource Sharing) configuration for enabling seamless integration with React and Vite development servers. Includes allowed origins, methods, headers, credentials handling, and troubleshooting for common CORS issues.

**Key Features:**
- Development-only CORS policy
- Cookie-based authentication support
- Allowed origins for Vite (port 5173/5174) and Create-React-App (port 3000/3001)
- Preflight caching with 1-hour max age
- Complete troubleshooting guide

---

## Authentication & Authorization

### Cookie-Based Authentication

This API uses **HTTP-only cookie-based authentication** (not Bearer tokens). Cookies are:
- **HttpOnly** - JavaScript cannot access (prevents XSS)
- **Secure** - HTTPS only in production
- **SameSite=Strict** - CSRF protection
- **7-day expiration** with sliding window (resets on each request)

### Authentication Flow

1. User calls `POST /api/authentication/login` with email/password
2. Password validated via ASP.NET Core Identity
3. Account active status checked (inactive accounts rejected)
4. Authentication cookie created with signed user claims
5. `Set-Cookie` header returned in response
6. Browser automatically includes cookie in subsequent requests
7. Cookie middleware validates signature and populates `User.Claims`
8. Protected endpoints access user claims without additional DB lookups

### Authorization Hierarchy

```
SuperAdmin (Full Control)
├── Can manage all users and roles
├── Can create/update/delete accounts
├── Can manage departments
└── Cannot remove SuperAdmin role from self (prevents lockout)

Admin (Limited Control)
├── Can manage only "User" role accounts
├── Cannot manage other Admins or SuperAdmins
├── Can manage departments
└── Cannot assign Admin or SuperAdmin roles

User (Standard Access)
├── Can view/update own profile only
├── Cannot manage other users
└── Cannot manage roles or departments
```

### Protecting Endpoints

```csharp
// Require authentication
[Authorize]
public async Task<IActionResult> MyAction() { }

// Require specific role
[Authorize(Roles = "SuperAdmin")]
public async Task<IActionResult> AdminOnly() { }

// Require any of multiple roles
[Authorize(Roles = "SuperAdmin,Admin")]
public async Task<IActionResult> AdminOrSuperAdmin() { }
```

### Default SuperAdmin User

Automatically created on first application startup:

| Field | Value |
|-------|-------|
| **Email/Username** | `superadmin@nox.local` |
| **Password** | `SuperAdmin@2024!Nox` |
| **Role** | SuperAdmin |
| **Department** | System Administration |

⚠️ **Change these credentials immediately in production!**

---

## Departments Management

### Overview

The Department system organizes users into organizational units:
- Each user must belong to exactly one department
- Departments can have an optional manager (must belong to that department)
- Each department can manage multiple users
- Soft deletion support (IsActive flag)

### Database Schema

**Departments Table:**
- `Id` (int) - Primary key
- `Name` (nvarchar, unique) - Department name
- `Description` (nvarchar, nullable) - Department description
- `ManagerId` (FK, nullable) - User ID of department manager
- `IsActive` (bit) - Soft deletion flag
- `CreatedAt`, `UpdatedAt` - Timestamps

**Relationships:**
- Department ↔ Users (one-to-many)
- Department → Manager (one-to-one, optional)

### Department-User Workflow

**Add User to Department:**
1. Create user via `UserManagementController.CreateUser()` with `departmentId`
2. User belongs to specified department
3. User appears in department's user count

**Reassign User to Different Department:**
1. Update user via `UserManagementController.UpdateUser()` with new `departmentId`
2. Old department user count decreases
3. New department user count increases

**Delete Department:**
- Only SuperAdmin can delete departments
- Cannot delete department with assigned users
- Use soft-delete (sets `IsActive = false`)
- Reassign all users to different department first

---

## Common Scenarios

### Scenario 1: Complete Login and Profile Update Flow

```http
### 1. Login
POST http://localhost:5164/api/authentication/login
Content-Type: application/json

{
  "email": "superadmin@nox.local",
  "password": "SuperAdmin@2024!Nox"
}

### 2. Get current user profile
GET http://localhost:5164/api/authentication/me

### 3. Update user profile
PUT http://localhost:5164/api/authentication/me
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "phone": "+1-555-0456",
  "address": "456 User Lane, Tech City"
}

### 4. Logout
POST http://localhost:5164/api/authentication/logout
```

### Scenario 2: Create and Promote User to Admin

```bash
# 1. Login as SuperAdmin
curl -X POST http://localhost:5164/api/authentication/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{"email":"superadmin@nox.local","password":"SuperAdmin@2024!Nox"}'

# 2. Create new user with User role
curl -X POST http://localhost:5164/api/usermanagement \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe",
    "departmentId": 2,
    "role": "User"
  }'

# 3. Get the new user's ID from response, then assign Admin role
curl -X POST http://localhost:5164/api/rolemanagement/user/{userId}/assign \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"roleName":"Admin"}'

# 4. Verify user now has Admin role
curl -X GET http://localhost:5164/api/rolemanagement/user/{userId} \
  -b cookies.txt
```

### Scenario 3: Department Management

```bash
# 1. Get all departments
curl -X GET http://localhost:5164/api/departments \
  -b cookies.txt

# 2. Create new department
curl -X POST http://localhost:5164/api/departments \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Innovation Lab",
    "description": "Research and innovation team"
  }'

# 3. Assign manager to department
curl -X PUT http://localhost:5164/api/departments/{departmentId}/manager \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"managerId": "user-id-123"}'

# 4. Update department info
curl -X PUT http://localhost:5164/api/departments/{departmentId} \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Innovation Lab",
    "description": "Updated description"
  }'
```

---

## Troubleshooting

### Port Already in Use (5164 or 7238)

```bash
# Find process using the port
lsof -i :5164

# Kill the process (replace PID with actual process ID)
kill -9 <PID>

# Or kill all dotnet processes
killall dotnet
```

### Database Connection Error

```
Cannot connect to server at localhost,1433
```

**Solution:**
1. Verify SQL Server container is running: `docker ps`
2. If not running, start it:
   ```bash
   docker start sqlserver
   ```
3. If never created, run startup command from Quick Start section
4. Verify connection string in `appsettings.Development.json`
5. Wait 10-15 seconds for SQL Server to fully initialize

### Migration Failures

```
The migrations assembly referenced by DbContext could not be loaded
```

**Solution:**
```bash
# Rebuild project
dotnet build

# If still failing, remove and recreate migration
dotnet ef migrations remove
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Entity Framework Core Tools Not Found

```
No executable found matching command "dotnet-ef"
```

**Solution:**
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update to latest version
dotnet tool update --global dotnet-ef
```

### SSL/Certificate Errors (HTTPS Profile)

**Solution:**
- Use HTTP profile for development: `dotnet run --launch-profile http`
- Or trust development certificate:
  ```bash
  dotnet dev-certs https --trust
  ```

### Multiple Application Instances Running

**Symptom:** "Port already in use" or unexpected behavior

**Solution:**
```bash
# Kill all dotnet processes
killall dotnet

# Start fresh
dotnet run --launch-profile http
```

### Default User Already Exists (RoleSeederService)

**Solution:**
```bash
# Reset database (⚠️ CAUTION: deletes all data)
dotnet ef database drop
dotnet ef database update
```

Default SuperAdmin will be recreated on next app startup.

---

## Additional Resources

For detailed endpoint documentation, see the `/Documentation` folder:

- **[Authentication Controller Documentation](./Documentation/AUTHENTICATION_CONTROLLER.md)** - Complete auth endpoint reference
- **[User Management Controller Documentation](./Documentation/USER_MANAGEMENT_CONTROLLER.md)** - User CRUD and admin endpoints
- **[Role Management Controller Documentation](./Documentation/ROLE_MANAGEMENT_CONTROLLER.md)** - Role assignment and management
- **[Department Controller Documentation](./Documentation/DEPARTMENT_CONTROLLER.md)** - Department management endpoints
- **[CORS Setup Guide](./Documentation/CORS_SETUP.md)** - CORS configuration for React/Vite frontend

For development guidance, see [CLAUDE.md](./CLAUDE.md) (project architecture and setup).

---

## Project Structure

```
NOX-Backend/
├── Models/                      # Data models (ApplicationUser, Department, etc.)
├── Controllers/                 # API endpoints
├── Services/                    # Business logic (RoleSeederService, DepartmentSeederService)
├── Data/                        # Database context (AppDbContext)
├── Migrations/                  # EF Core migrations
├── Properties/                  # Launch profiles (launchSettings.json)
├── Documentation/               # Detailed controller documentation
├── Program.cs                   # Dependency injection & middleware setup
├── appsettings.json             # Production configuration
├── appsettings.Development.json # Development configuration
└── NOX-Backend.csproj           # Project file
```

---

## License

This project is part of the NPAX platform. See LICENSE for details.
