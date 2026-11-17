# CuraLink doctor module

## Scope

The doctor module is the authenticated `Doctor` area of CuraLink. A user with the `Doctor` role can complete a profile, manage weekly schedules and exceptions, review appointment requests, change appointment statuses, browse assigned patients, write and finalize consultations, issue prescriptions, and upload/download private medical documents. Access is restricted by role and every query is scoped to the current doctor. Doctor registration starts with `Pending` verification; only `Approved`, active doctors with an active specialty can be publicly listed. Patients register with the `Patient` role and do not get access to the doctor area.

## Endpoints

| Route | Purpose |
| --- | --- |
| `/health` | Liveness check (`ok`) |
| `/Account/Login`, `/Account/Register`, `/Account/Logout` | Identity entry points |
| `/Doctor/Dashboard` | Today's appointments, pending requests, upcoming count, patient count |
| `/Doctor/Profile` (`/Edit`) | Doctor profile view and update |
| `/Doctor/Schedule` | Schedules, exceptions, available slot calculation |
| `/Doctor/Appointments` (`/Details`) | Appointment list/details, status changes, rescheduling |
| `/Doctor/Patients` (`/Details`) | Patient list and clinical detail view |
| `/Doctor/Consultations` (`/Create`, `/Edit`, `/Details`) | Consultation authoring, finalize, share |
| `/Doctor/Prescriptions` (`/Create`, `/Details`) | Issue and cancel prescriptions |
| `/Doctor/Documents` (`/Upload`, `/Download`) | Private document upload and authorized download |

Unauthenticated requests to `/Doctor/*` redirect to `/Account/Login`. Doctors whose verification status is not `Approved` are redirected to `/Doctor/Profile` and cannot reach clinical pages.

## Run with .NET 6

```bash
dotnet restore CuraLink.sln
dotnet test CuraLink.sln
dotnet run --project src/CuraLink.Web
```

A reachable SQL Server instance is required; override `ConnectionStrings__DefaultConnection` if the default localDB string is not usable on your machine. Seeding is off by default in `appsettings.json`; set `SeedDatabase=true` on a fresh database to create roles, specialties, and fictional demo users. At startup `MigrateAsync` applies all EF Core migrations before seeding runs. Outside the Development environment, `Curalink__DemoPassword` must be configured or seeding throws a clear error.

## Run with Docker

```bash
cp .env.example .env
docker compose up --build
```

Edit `.env` before starting. Compose requires both `SQL_SA_PASSWORD` and `DEMO_PASSWORD` and fails fast if either is unset. It starts SQL Server 2022, waits for its `sqlcmd` health check, then starts the web app with `SeedDatabase=true` and `Curalink__DemoPassword` taken from `.env`. The web app is available at `http://localhost:8080`. Private uploads persist in the `private-uploads` named volume mounted at `/app/App_Data/private-uploads`. Use `docker compose down -v` to discard the database and upload volumes.

## Configuration

| Variable | Default | Notes |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | localDB string in `appsettings.json` | Compose points to `Server=db;Database=CuraLinkDb;...;TrustServerCertificate=True` |
| `SQL_SA_PASSWORD` | none; required in `.env` | Compose-only; must satisfy SQL Server password policy |
| `SeedDatabase` | `false` in `appsettings.json` | Compose sets `true` to opt in to demo seeding |
| `ASPNETCORE_ENVIRONMENT` | `Production` in Docker | Enables the production error handler |
| `ASPNETCORE_URLS` | `http://+:8080` | Matches the exposed container port |
| `Curalink__DemoPassword` | `Demo@12345` in Development only | Required outside Development; Compose sets it from `DEMO_PASSWORD` |
| `UseHttps` | `false` | HTTPS redirection is opt-in |
| `App__TimeZone` | `Africa/Tunis` | Time zone used to display stored UTC timestamps |
| `FileStorage__PrivateRoot` | `App_Data/private-uploads` | Private upload root on the web host; Compose persists it in the `private-uploads` volume |

## Demo password warning

Seeded users are fictional. The `Demo@12345` fallback applies only in the Development environment; outside Development, `Curalink__DemoPassword` is required and seeding throws `InvalidOperationException` when it is missing. Compose always supplies it from `DEMO_PASSWORD` in `.env`. Change both `DEMO_PASSWORD` and `SQL_SA_PASSWORD` to real, unique values for any real deployment, and never reuse the `.env.example` placeholders outside local development.

## Assumptions

- All timestamps are stored in UTC and converted for display using `App:TimeZone`.
- Clinical data is never written to logs or emails; the development email sender only logs a generic message.
- Uploaded files stay in local private storage behind the authorized download endpoint.
- Appointment overlap, future-start, status-transition, and prescription item rules are enforced by domain rules in the application services.
- Integration tests disable seeding and replace SQL Server with an EF Core in-memory context; they do not validate SQL Server-specific behavior.
