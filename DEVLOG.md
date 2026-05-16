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

## Next Steps

Reviewing the behavior by hitting the endpoints and checking responses.
Can I add Swagger or use builtin OpenAPI to make that check easy via a web ui?
Yes adding Scalar which is latest and greatest version of swaggerui.

Now testing the user register + session login/logout/me endpoints.
failing on /me.

# Add test project

Summary of Changes:

1.  Test Project: Created tests/Bellhop.IntegrationTests with xUnit.
2.  Infrastructure:
    - Added BellhopWebApplicationFactory which automatically swaps PostgreSQL for
      a SQLite In-Memory database during tests.
    - Updated Program.cs to support this swap and to use Database.EnsureCreated()
      when running in the Testing environment.
3.  Test Coverage: Implemented a comprehensive flow in SessionAuthTests.cs that
    verifies:
    - User Registration: Creating a new user via /api/v1/users/register.
    - Login: Authenticating and establishing a server-side session via
      /api/v1/auth/session/login.
    - Identity: Verifying the current user state via /api/v1/auth/session/me.
    - Logout: Terminating the session and revoking the token via
      /api/v1/auth/session/logout.

How to run your tests:
You can run the tests anytime from your terminal:

1 dotnet test tests/Bellhop.IntegrationTests/Bellhop.IntegrationTests.csproj
