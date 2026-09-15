# SchoolApp

A school management system for handling students, professors, groups, modules, exams and absences — with role-based access (Admin / Professeur / Etudiant), TOTP two-factor authentication, and forced password rotation on first login.

## Tech stack

- **ASP.NET Core 8 MVC** (C#, Razor Views)
- **Entity Framework Core 8** with the **Npgsql** provider
- **ASP.NET Core Identity** for auth (roles, lockout, 2FA)
- **PostgreSQL** as the database
- Repository / Unit-of-Work pattern (`Repositories/`) on top of EF Core

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running PostgreSQL server (Aiven, Render, Supabase, Railway, or local)

## Setup

### 1. Clone

```bash
git clone https://github.com/ranimejemal/school-app-final.git
cd school-app-final
```

### 2. Configure the database connection

`appsettings.json` ships with a placeholder connection string (`ConnectionStrings:DefaultConnection`). **Don't run with that value as-is** — set your own via `dotnet user-secrets` instead of editing the committed file:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<your-host>;Port=5432;Database=SchoolDb;Username=<your-user>;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true"
```

Drop the `SSL Mode`/`Trust Server Certificate` parts if you're pointing at a local Postgres instance without TLS. This overrides `appsettings.json` locally without touching the repo. (Note: a real MySQL password was previously committed to this file's history before the app was migrated to Postgres — if you're reusing any part of that old credential elsewhere, rotate it.)

### 3. Create the database

```bash
dotnet restore
dotnet run
```

There's no separate migration step — `DbInitializer.SeedAsync` (called on every startup in `Program.cs`) runs `Database.EnsureCreatedAsync()` against whatever connection string you've configured, creating the schema directly from the EF Core model. On first run it also seeds:

- Roles: `Admin`, `Professeur`, `Etudiant`
- A default admin account — **username:** `admin`, **password:** `Admin@1234`
- Sample niveaux, specialites, groupes, modules, professeurs and etudiants

Change the seeded admin password immediately after first login (or before deploying anywhere public).

> This app uses `EnsureCreated()`, not EF Core Migrations — there's no `Migrations/` folder. That's fine for this project's scope, but it means schema changes aren't tracked/versioned; if you extend the model significantly, consider adding migrations back with `dotnet ef migrations add <Name>` (requires the `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`) and switching `Program.cs` to call `Database.Migrate()` instead.

The app listens on the URL(s) printed in the console (typically `https://localhost:5001` / `http://localhost:5000`, or whatever's configured in `Properties/launchSettings.json` if present). Open that URL and log in with the seeded admin account.

## Login flow

1. Sign in with username/password.
2. Account lockout: **5 failed attempts** locks the account for **5 minutes**.
3. If 2FA is enabled on the account, you're prompted for a 6-digit authenticator code (`Account/LoginWith2fa`).
4. First login after seeding (or any account with `MustChangePassword = true`) is redirected to `Account/ForceChangePassword` before it can access anything else.

### Enabling 2FA

Once logged in, go to `Account/TwoFactorSetup` — it generates a shared key + QR code (Google Authenticator / any TOTP app) and asks you to confirm a generated code before turning 2FA on.

## Roles

| Role | Access |
|---|---|
| `Admin` | Full CRUD on niveaux, specialites, groupes, modules, professeurs, affectations, users |
| `Professeur` | Their assigned modules, student absences, exam entry |
| `Etudiant` | Their own profile, grades, absences |

## Project structure

```
Controllers/    MVC controllers (Account, Admin entities, Absence, Examen, Statistics, ...)
Models/         EF Core entities + ViewModels/
Data/           ApplicationDbContext + DbInitializer (seeding)
Repositories/   Generic repository + Unit of Work
Services/       Business logic (AbsenceService, StatisticsService)
Views/          Razor views, one folder per controller
wwwroot/        Static assets (css/site.css, js/site.js)
```

## Deploying

This is a stateful ASP.NET Core app (Identity, sessions, cookie auth) with a Postgres backend, so it needs a real long-running host — it will **not** run on Vercel (no .NET runtime there, and Vercel's serverless functions are stateless, which breaks the in-memory session store this app uses).

Both configs below build the same `Dockerfile`, which already reads the platform's assigned `$PORT` at runtime — no per-host Dockerfile changes needed.

### Railway (current target)

1. **Get a PostgreSQL database.** Railway can provision one for you (New → Database → PostgreSQL in the same project), or use an external provider (Aiven, Supabase). Either way you need the connection string in this format:
   `Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true`
2. **Create the service.** Railway dashboard → New → Deploy from GitHub repo → select this repo. `railway.json` pins the builder to `DOCKERFILE` explicitly, so it won't auto-detect the wrong language.
3. **Set the connection string.** In the service's Variables tab, add `ConnectionStrings__DefaultConnection` with the value from step 1. Don't put it in `appsettings.json`.
4. **Deploy.** On first boot, `DbInitializer.SeedAsync` creates the schema (via `EnsureCreatedAsync`) and seeds roles + the default admin account (see [Setup](#3-create-the-database) above) — no separate migration step needed.
5. Log in as `admin` / `Admin@1234` and change the password immediately (the forced-password-change flow will prompt you anyway).

### Render (also configured, `render.yaml`)

Same idea as Railway: New → Blueprint → point at this repo, it picks up `render.yaml` and `Dockerfile` automatically, prompts for `ConnectionStrings__DefaultConnection` as a secret. Kept in the repo in case Railway doesn't work out — the Dockerfile is shared between both.

### Running the container locally

```bash
docker build -t schoolapp .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=SchoolDb;Username=postgres;Password=<your-password>;SSL Mode=Require;Trust Server Certificate=true" \
  schoolapp
```

## Security notes

- Cookies are `HttpOnly`, `SecurePolicy = SameAsRequest`, 8-hour sliding expiration.
- Security headers set on every response: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `X-XSS-Protection`.
- `app.UseHsts()` + `UseHttpsRedirection()` in non-development environments.
- Forced password change middleware blocks all routes except login/logout/change-password until `MustChangePassword` is cleared.
