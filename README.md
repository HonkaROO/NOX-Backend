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
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
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

