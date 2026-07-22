# Hotel Manager (Modernized)

Modern rewrite of the legacy VB.NET/WinForms/Access Hotel Management System.
The legacy sources remain untouched at the repository root; this folder contains
the new application.

## Stack

- ASP.NET Core 10 + Blazor Server (interactive server rendering)
- MudBlazor 8 UI
- EF Core 10 with a configurable provider: SQLite (default) or PostgreSQL
- Cookie authentication with ASP.NET Core Identity password hashing
  (replaces the legacy Base64 `ModFunc.Encrypt/Decrypt`)
- xUnit + Coverlet

The .NET 10 SDK is pinned via `global.json`.

## Solution layout

| Project | Purpose |
| --- | --- |
| `src/HotelManager.Domain` | POCO entities mirroring every table in `HMS_DBDataSet.xsd` (names/relationships preserved) |
| `src/HotelManager.Infrastructure` | `HotelDbContext`, EF configurations, `InitialCreate` migration, `DbSeeder`, `AuthService` |
| `src/HotelManager.Application` | **Frozen contracts** (`IAuthService` … `IReportService`), DTOs, and the shared `BillingCalculator` reproducing the legacy billing/payroll math exactly |
| `src/HotelManager.Web` | Blazor Server host: login, role gating (Admin/User), dashboard, nav mirroring the legacy main menu groups |
| `tests/HotelManager.UnitTests` | BillingCalculator tests |
| `tests/HotelManager.IntegrationTests` | SQLite in-memory fixture + AuthService tests |

## Run locally

```bash
cd modern
dotnet run --project src/HotelManager.Web
```

On first start the app migrates and seeds the SQLite database at
`%LOCALAPPDATA%/HotelManager/hotelmanager.db` (override with the
`HotelManager:DbPath` configuration key).

Default login: **admin / admin@123** (UserType `Admin`).

### Database provider

The provider is selected with the `HotelManager:DbProvider` configuration key
(`Sqlite`, the default, or `Postgres`). For PostgreSQL, also supply a connection
string named `Postgres`:

```bash
export HotelManager__DbProvider=Postgres
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=hotelmanager;Username=hotel;Password=hotel"
dotnet run --project src/HotelManager.Web
```

Migrations are applied automatically on startup (`DbSeeder.SeedAsync` runs
`Database.Migrate`), and each provider has its own migration set
(`Migrations/Sqlite` and `Migrations/Postgres`). A custom
`ProviderFilteredMigrationsAssembly` ensures only the active provider's
migrations are applied. To add a migration for a specific provider:

```bash
# SQLite (default)
dotnet ef migrations add <Name> --project src/HotelManager.Infrastructure \
  --output-dir Migrations/Sqlite --namespace HotelManager.Infrastructure.Migrations.Sqlite

# PostgreSQL
HotelManager_DbProvider=Postgres dotnet ef migrations add <Name> \
  --project src/HotelManager.Infrastructure \
  --output-dir Migrations/Postgres --namespace HotelManager.Infrastructure.Migrations.Postgres
```

## Run with Docker

```bash
cd modern
docker compose up --build
```

The app is served at http://localhost:8080 and is backed by a PostgreSQL
container (`HotelManager__DbProvider=Postgres`). PostgreSQL data is persisted in
the `hotelmanager-postgres` volume, and the `web` service waits for the database
healthcheck before starting. The schema is migrated and seeded automatically on
first start, so the same default login (**admin / admin@123**) works out of the
box.

## Test

```bash
cd modern
dotnet test --collect:"XPlat Code Coverage"
```

## CI

`.github/workflows/modern-ci.yml` runs on every PR and push to `main` touching
`modern/`: it builds the solution, runs `dotnet test --collect:"XPlat Code Coverage"`,
publishes a ReportGenerator HTML report as the `coverage-report` artifact, and fails
if combined line coverage (excluding generated EF migrations and `.razor` UI markup,
which is exercised manually/e2e rather than by unit tests) is below 80%.

## Legacy fidelity notes

- `BillingCalculator` reproduces `frmCheckIn.Compute/Compute1/Compute2` and
  `frmCheckOut.Calculate` including VB `CInt` banker's rounding, 2-decimal tax
  rounding, and the education/higher-education cess derivations.
- Payroll reproduces `frmEmployeePayment`: `Salary = BasicSalary × PresentDays / 30`,
  overtime from `TotalMinutes × rate / 60`, advance = Σamount − Σdeduction, and the
  guards (deduction ≤ advance, net pay ≥ 0).
- The legacy `AdvanceEntry` table stores both advances (`Amount`) and deductions
  (`Deduction` column) — there is no separate `DeductionEntry` table in the schema,
  so deduction entries are `AdvanceEntry` rows with `Deduction > 0`.
- The hardware-locked activation/splash (`frmSplash`/`frmActivation`) is intentionally
  not carried over.

## Coordination rules (parallel workstreams)

- Do **not** modify `HotelManager.Domain`, `HotelManager.Infrastructure`, or the
  interfaces/`BillingCalculator` in `HotelManager.Application`; request changes
  from the lead.
- Register DI only in your own `src/HotelManager.Web/Extensions/*ServiceRegistration.cs`.
- Add nav links only in your own `src/HotelManager.Web/Components/Layout/Nav*.razor`.
