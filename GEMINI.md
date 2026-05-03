# Bellhop Authentication Server

This project is an authentication and identity server built with .NET 10 and PostgreSQL.

## Architecture
The project follows a **Vertical Slices / Feature Folders** architectural pattern. Domain logic, endpoints, and data models are grouped by feature under the `src/Features/` directory.

## Core Components
- **Identity**: User management and password hashing (BCrypt).
- **Session Auth**: Cookie-based authentication stored in the PostgreSQL `Sessions` table.
  - Namespace: `/api/v1/auth/session/`
- **Token Auth**: JWT-based authentication with database-backed refresh tokens.
  - Namespace: `/api/v1/auth/token/`

## Infrastructure
- **Database**: PostgreSQL (defined in `docker-compose.yml`).
- **ORM**: Entity Framework Core (Code-First).
- **Session Store**: Server-side sessions are persisted in the PostgreSQL database for infrastructure simplicity.

## Development Guidelines

### Database Migrations
**Important:** Do not directly edit files in `Infrastructure/Data/Migrations`. Whenever database schema changes are required, update the source Models and `AppDbContext.cs` and run another migration using the following command:

```bash
dotnet ef migrations add <MigrationName> --project src/Bellhop.csproj --output-dir Infrastructure/Data/Migrations
```

### Running Locally
The project is designed to run via Docker Compose:
```bash
docker-compose up --build
```
Migrations are automatically applied on application startup.
