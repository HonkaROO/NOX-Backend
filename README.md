# NOX-Backend

NPAX-Onboarding eXpert Chatbot (Backend)

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
- `SA_PASSWORD=YourPassword123!` - Set system admin password (change this in production)
- `-p 1433:1433` - Map container port 1433 to host port 1433
- `--name sqlserver` - Name the container "sqlserver"
- `-d` - Run in detached mode (background)
- `mcr.microsoft.com/mssql/server:2019-latest` - Use SQL Server 2019 image

**Note:** The password in `appsettings.Development.json` must match the `SA_PASSWORD` set here.

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

# Run the application
dotnet run --launch-profile https
```

The API will be available at:

- HTTPS: `https://localhost:7238`
- Swagger UI: `http://localhost:5164/swagger/index.html` (HTTP, development only)

### Stopping the SQL Server Container

```bash
# Stop the container
docker stop sqlserver

# Remove the container (if you want to start fresh)
docker rm sqlserver
```

## Development

See [CLAUDE.md](./CLAUDE.md) for detailed architecture information and development workflow.

### Common Commands

- **Build:** `dotnet build`
- **Run:** `dotnet run --launch-profile https`
- **Database migrations:** `dotnet ef migrations add MigrationName`
- **Apply migrations:** `dotnet ef database update`
- **Run with watch mode:** `dotnet watch run`

## API Documentation

Complete documentation for each API controller is available in the `/docs` folder:

- **[Authentication Controller](./docs/AUTHENTICATION_CONTROLLER.md)** - User login, logout, and profile management
  - `POST /api/authentication/login` - Login with email/password
  - `GET /api/authentication/me` - Get current user profile
  - `PUT /api/authentication/me` - Update current user profile
  - `POST /api/authentication/logout` - Logout current user

- **[User Management Controller](./docs/USER_MANAGEMENT_CONTROLLER.md)** - User account creation and administration
  - `GET /api/usermanagement` - Get all users (filtered by role)
  - `GET /api/usermanagement/{userId}` - Get specific user
  - `POST /api/usermanagement` - Create new user
  - `PUT /api/usermanagement/{userId}` - Update user information
  - `DELETE /api/usermanagement/{userId}` - Deactivate user
  - `POST /api/usermanagement/{userId}/reset-password` - Reset user password

- **[Role Management Controller](./docs/ROLE_MANAGEMENT_CONTROLLER.md)** - Role and authorization management
  - `GET /api/rolemanagement` - Get all available roles
  - `GET /api/rolemanagement/user/{userId}` - Get user's roles
  - `POST /api/rolemanagement/user/{userId}/assign` - Assign role to user
  - `DELETE /api/rolemanagement/user/{userId}/remove/{roleName}` - Remove role from user
  - `GET /api/rolemanagement/{roleName}/users` - Get users in role

- **[Department Controller](./docs/DEPARTMENT_CONTROLLER.md)** - Department management
  - `GET /api/departments` - Get all departments
  - `GET /api/departments/{id}` - Get specific department
  - `POST /api/departments` - Create new department
  - `PUT /api/departments/{id}` - Update department
  - `PUT /api/departments/{id}/manager` - Assign manager to department
  - `DELETE /api/departments/{id}` - Delete (soft-delete) department
