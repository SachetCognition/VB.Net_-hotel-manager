# Hotel Manager (Modernized)

Modern rewrite of the legacy VB.NET/WinForms/Access Hotel Management System.
The legacy sources remain untouched at the repository root; this folder contains
the new application.

## Stack

- ASP.NET Core 8 (LTS) + Blazor Server (interactive server rendering)
- MudBlazor UI
- EF Core 8 + SQLite
- Cookie authentication with ASP.NET Core Identity password hashing
  (replaces the legacy Base64 `ModFunc.Encrypt/Decrypt`)
- xUnit + Coverlet

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

## Test

```bash
cd modern
dotnet test --collect:"XPlat Code Coverage"
```

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
