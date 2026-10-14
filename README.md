# RaceDay Event Management System

## System Description

RaceDay is an event management system designed to manage race events, organisers, participants, event categories, enrolments, and race results.

The system allows organisers to create and manage race events and categories, while participants can register for events and categories. The database stores user information, event details, enrolments, and race results in a structured relational database.

## System Roles

### 1. Organiser

The Organiser manages race events and their categories. The Organiser can create and update event information, manage event categories, and monitor participant enrolments.

### 2. Participant

The Participant can view available race events and categories, enrol in events, provide their required information, and view their enrolment and race results.

## Database

The RaceDay database was created using Microsoft SQL Server and Entity Framework Core (Code-First approach). It contains the following entities:

* Users
* Organisers
* Participants
* Events
* Categories
* Enrolments
* Results

The database uses primary keys and foreign keys to maintain relationships and data integrity between the entities.

## Authentication and Role-Based Access

The API uses session-based authentication. On login, the server stores the user's ID and role in the session. Every protected endpoint checks whether the user is logged in (returns **401 Unauthorized** if not) and whether the user has the correct role (returns **403 Forbidden** if not).

Passwords are hashed using BCrypt and are never stored in plain text.

## API Endpoints

The API exposes the following endpoint groups:

* **Auth** — register, login, logout
* **Events** — list, view, create, update, delete
* **Categories** — list by event, view, create, update, delete
* **Enrolments** — enrol, view own, view by event, delete
* **Results** — view own, view by event, capture, update, delete

All endpoints are visible and testable through the Swagger UI.

## Setup and Run

1. Clone the repository.
2. Ensure SQL Server LocalDB is running: `sqllocaldb start MSSQLLocalDB`
3. Update the connection string in `appsettings.json` if needed.
4. Apply the database migrations: `dotnet ef database update`
5. Run the API: `dotnet run --project RaceDay.Api`
6. Open Swagger in your browser at `https://localhost:<port>/swagger`.

## Testing

Unit tests are located in the `RaceDay.Api.Tests` project. Run them with: `dotnet test`

The test suite covers registration, login, password hashing, role-based access, event management, and event enrolments. All tests pass locally.

## CI/CD

GitHub Actions runs the test suite automatically on every push to the repository. The workflow file is located at `.github/workflows/ci.yml`.

## References

[1] Microsoft, “SQL Server documentation,” Microsoft Learn, 2026. [Online]. Available: https://learn.microsoft.com/en-us/sql/sql-server/. [Accessed: Oct. 14, 2026].

[2] Microsoft, “ASP.NET Core web API documentation,” Microsoft Learn, 2026. [Online]. Available: https://learn.microsoft.com/en-us/aspnet/core/web-api/. [Accessed: Oct. 14, 2026].

[3] Microsoft, “Entity Framework Core documentation,” Microsoft Learn, 2026. [Online]. Available: https://learn.microsoft.com/en-us/ef/core/. [Accessed: Oct. 14, 2026].

[4] Microsoft, “Session and state management in ASP.NET Core,” Microsoft Learn, 2026. [Online]. Available: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state. [Accessed: Oct. 14, 2026].

[5] B. Provost, “BCrypt.Net-Next,” GitHub, 2026. [Online]. Available: https://github.com/BcryptNet/bcrypt.net. [Accessed: Oct. 14, 2026].

[6] xUnit.net, “Getting started with xUnit.net,” xUnit.net, 2026. [Online]. Available: https://xunit.net/docs/getting-started/netcore/cmdline. [Accessed: Oct. 14, 2026].
