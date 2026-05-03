Goals of the project:

Gain proficiency with:

- .NET10 Web API project
- Authentication Protocols
- Docker + Docker Compose
- Postgres
- Agent Driven Development

Using Gemini for most of the development, manual edits only when necessary.
Update the GEMINI.md with directives as needed
Be able to explain + understand all system components - dont replace brain w AI

# First Pass

The server now supports:

- User Registration: Create users with secure BCrypt password hashing.
- Session Authentication: Cookie-based login/logout/me endpoints (/api/v1/auth/session/).
- Token Authentication: JWT-based login/refresh/me endpoints (/api/v1/auth/token/).
- Infrastructure: Fully containerized with PostgreSQL and automatic EF Core migrations on startup.

Getting Started
To start the server and its database, run:
1 docker-compose up --build

Endpoints

- Register: POST /api/v1/users/register
- Session Login: POST /api/v1/auth/session/login
- Token Login: POST /api/v1/auth/token/login
- Token Refresh: POST /api/v1/auth/token/refresh
