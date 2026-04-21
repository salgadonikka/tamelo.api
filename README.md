# Tamelo.Api

The ASP.NET Core backend for **Tamelo**, *The Procrastinator's To Do List*. Built on Clean Architecture with MediatR, EF Core, and PostgreSQL. Exposes a REST API consumed by the [Tamelo frontend](https://github.com/salgadonikka/tamelo).

## Tech Stack

| Concern | Library / Tool |
|---|---|
| Framework | ASP.NET Core 10 (Minimal APIs) |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / Web) |
| CQRS | MediatR 14 |
| Validation | FluentValidation 12 |
| ORM | Entity Framework Core 10 + Npgsql |
| Database | PostgreSQL |
| Auth | Supabase JWT (RS256/ES256 via OIDC JWKS — no shared secret) |
| API docs | Scalar (OpenAPI) at `/api` in development |
| Testing | NUnit + Shouldly + Testcontainers.PostgreSql |

## Prerequisites

- **.NET SDK 10.0.103** (pinned via `global.json`)
- A **PostgreSQL** database (any provider — self-hosted, Neon, Render, etc.)
- A **Supabase project** for authentication — only the project URL and JWKS endpoint are needed (no JWT secret; the database is not on Supabase)

## Getting Started

### 1. Configure `appsettings.json`

Edit `src/Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Tamelo.ApiDb":    "Host=<host>;Port=5432;Database=tamelo;Username=<user>;Password=<password>",
    "Tamelo.Direct":   "Host=<host>;Port=5432;Database=tamelo;Username=<user>;Password=<password>"
  },
  "Supabase": {
    "Url": "https://<ref>.supabase.co"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:8080", "https://localhost:8080"]
  }
}
```

| Setting | Purpose |
|---|---|
| `Tamelo.ApiDb` | Used by the running API (can point to a pooler if needed) |
| `Tamelo.Direct` | Direct connection — used by EF Core CLI only (`dotnet ef migrations add`, `dotnet ef database update`) |
| `Supabase:Url` | Used to construct the JWKS/OIDC authority URL for token validation |
| `Cors:AllowedOrigins` | Origins allowed to call the API (add your frontend URL) |

> Both connection strings can point to the same host. `Tamelo.Direct` exists so that EF Core CLI tooling always bypasses any connection pooler.

### 2. Apply database migrations

EF Core migrations are **not applied on startup**. Run them via the EF Core CLI using the `Tamelo.Direct` connection string:

```bash
# From src/Infrastructure/
dotnet ef database update --startup-project ../Web
```

To generate a SQL script instead (e.g. for a managed database that restricts CLI access):

```bash
dotnet ef migrations script --idempotent --output migration.sql \
  --project . --startup-project ../Web
```

### 3. Run the API

```bash
cd src/Web
dotnet watch run
```

The API runs on `https://localhost:5001`. Interactive API docs (Scalar) are available at `https://localhost:5001/api`.

## Commands Reference

```bash
# Build
dotnet build -tl

# Run with hot reload (from src/Web/)
dotnet watch run

# Run all tests (excluding Playwright acceptance tests)
dotnet test --filter "FullyQualifiedName!~AcceptanceTests"

# Run acceptance tests (start the API first, then in a second terminal)
cd src/Web && dotnet test

# Add a new EF Core migration (from src/Infrastructure/)
dotnet ef migrations add <MigrationName> --startup-project ../Web

# Generate idempotent migration SQL
dotnet ef migrations script --idempotent --output migration.sql --startup-project ../Web
```

## Project Structure

```
src/
├── Domain/             # Entities, enums, domain exceptions, base types
├── Application/        # CQRS commands/queries (MediatR), validators, interfaces
├── Infrastructure/     # EF Core DbContext, Npgsql, design-time factory
└── Web/                # ASP.NET Core host, endpoint groups, DI wiring, Program.cs
tests/
├── Domain.UnitTests/
├── Application.UnitTests/
├── Infrastructure.IntegrationTests/   # Testcontainers.PostgreSql
├── Application.FunctionalTests/
└── Web.AcceptanceTests/               # Playwright end-to-end
```

> **Build note:** Build from `src/Web/` directly (`cd src/Web && dotnet build`). The `.slnx` solution file references Aspire projects (`AppHost`, `ServiceDefaults`) that are not yet present and will fail at solution level.

## Architecture

### Layers

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Domain/` | Entities, value objects, enums, domain exceptions. No dependencies. |
| Application | `Application/` | CQRS commands and queries via MediatR. FluentValidation validators. Interfaces for infrastructure. |
| Infrastructure | `Infrastructure/` | EF Core `ApplicationDbContext`. Npgsql/PostgreSQL. `IDesignTimeDbContextFactory`. |
| Web | `Web/` | ASP.NET Core minimal API endpoints, `Program.cs`, JWT auth, CORS, OpenAPI. |

### Endpoints

All endpoints are auto-discovered from subclasses of `EndpointGroupBase`. The class name becomes the route prefix under `/api/`.

| Endpoint group | Routes |
|---|---|
| `Tasks` | `GET/POST /api/Tasks`, `GET/PUT/DELETE /api/Tasks/{id}`, `PATCH /api/Tasks/{id}/archive`, `PATCH /api/Tasks/{id}/reorder`, `GET/POST /api/Tasks/{id}/history` |
| `Projects` | `GET/POST /api/Projects`, `GET/PUT/DELETE /api/Projects/{id}` |
| `DayMarkers` | `PUT/DELETE /api/DayMarkers/{taskItemId}/{date}` |
| `TaskNotes` | `GET /api/TaskNotes/{taskItemId}`, `POST /api/TaskNotes`, `PUT/DELETE /api/TaskNotes/{id}` |
| `UserProfiles` | `GET/PUT /api/UserProfiles/me` |

Full interactive documentation is available at `/api` when running in development.

### Authentication

The API validates **Supabase-issued JWTs** via OIDC discovery. No shared secret is stored — ASP.NET Core fetches the JWKS keys automatically from the Supabase authority URL.

```
ValidIssuer:   https://<ref>.supabase.co/auth/v1
ValidAudience: authenticated
JWKS endpoint: https://<ref>.supabase.co/auth/v1/.well-known/jwks.json
```

Every command and query is decorated with `[Authorize]`. The `AuthorizationBehaviour` MediatR pipeline behaviour enforces that `IUser.Id` is non-null before any handler runs. `IUser.Id` resolves to the Supabase user UUID from the `sub` JWT claim.

### User Data Isolation

All entities have a `string UserId` property (the Supabase user UUID). Every query handler filters `WHERE UserId = @userId`. There are no shared rows between users.

### MediatR Pipeline

Requests flow through the following pipeline behaviours before reaching a handler:

1. `UnhandledExceptionBehaviour` — logs unhandled exceptions
2. `AuthorizationBehaviour` — checks `[Authorize]` + `IUser.Id != null`
3. `ValidationBehaviour` — runs FluentValidation; returns 400 on failure
4. `PerformanceBehaviour` — logs slow queries (> 500 ms)

### Domain Entities

Located in `src/Domain/Entities/`:

| Entity | Description |
|---|---|
| `TaskItem` | A task (title, notes, sort order, archived flag) |
| `Project` | Groups tasks; has a colour and optional description |
| `DayMarker` | A date + `CircleState` (Empty/Planned/Started/Completed) on a task |
| `TaskNote` | A free-text note attached to a task |
| `TaskHistory` | An immutable audit log entry for a task event |
| `UserProfile` | Per-user display name and avatar |

### EF Core Design-Time Factory

`ApplicationDbContextFactory` (`src/Infrastructure/Data/`) implements `IDesignTimeDbContextFactory<ApplicationDbContext>`. The EF Core CLI picks it up automatically when adding or scripting migrations. It reads `Tamelo.Migrations` from `../Web/appsettings.json`, bypassing the full DI container.

## Code Scaffolding

New commands and queries can be scaffolded using the Clean Architecture template:

```bash
# Install the template (once)
dotnet new install Clean.Architecture.Solution.Template::10.3.0

# New command (from src/Application/)
dotnet new ca-usecase --name CreateProject --feature-name Projects --usecase-type command --return-type int

# New query
dotnet new ca-usecase --name GetProject --feature-name Projects --usecase-type query --return-type ProjectDto
```

## Package Management

NuGet package versions are managed centrally in `Directory.Packages.props`. Add new packages there — do not specify versions in individual `.csproj` files.
