# SchoolApp

A school management system for handling students, professors, groups, modules, exams and absences — with role-based access (Admin / Professeur / Etudiant), TOTP two-factor authentication, and forced password rotation on first login.

## Tech stack

- **ASP.NET Core 8 MVC** (C#, Razor Views)
- **Entity Framework Core 8** with the **Pomelo MySQL** provider
- **ASP.NET Core Identity** for auth (roles, lockout, 2FA)
- **MySQL** as the database
- Repository / Unit-of-Work pattern (`Repositories/`) on top of EF Core

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running MySQL server (8.x recommended)
- (Optional) `dotnet-ef` CLI tool for migrations: `dotnet tool install --global dotnet-ef`

## Setup

### 1. Clone

```bash
git clone https://github.com/ranimejemal/school-app-final.git
cd school-app-final
```

### 2. Configure the database connection

`appsettings.json` currently ships with a hardcoded local connection string (`ConnectionStrings:DefaultConnection`). **Don't run with that value as-is** — set your own via `dotnet user-secrets` instead of editing the committed file:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=SchoolDb;user=root;password=<your-password>;"
```

This overrides `appsettings.json` locally without touching the repo. (If you've already pushed a real password in `appsettings.json`, rotate that MySQL credential — it's public in the repo history.)

### 3. Create the database and apply migrations

```bash
dotnet restore
dotnet ef database update
```

This creates the `SchoolDb` schema from `Migrations/`. On first run, the app also seeds:

- Roles: `Admin`, `Professeur`, `Etudiant`
- A default admin account — **username:** `admin`, **password:** `Admin@1234`
- Sample niveaux, specialites, groupes, modules, professeurs and etudiants

Change the seeded admin password immediately after first login (or before deploying anywhere public).

### 4. Run

```bash
dotnet run
```

By default the app listens on the URL(s) printed in the console (typically `https://localhost:5001` / `http://localhost:5000` for `dotnet run`, or whatever's configured in `Properties/launchSettings.json` if present). Open that URL and log in with the seeded admin account.

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
Migrations/     EF Core migrations
Views/          Razor views, one folder per controller
wwwroot/        Static assets (css/site.css, js/site.js)
```

## Deploying (Render)

This is a stateful ASP.NET Core app (Identity, sessions, cookie auth) with a MySQL backend, so it needs a real long-running host — it will **not** run on Vercel (no .NET runtime there, and Vercel's serverless functions are stateless, which breaks the in-memory session store this app uses).

Render works because it runs the container as a persistent process. Steps:

1. **Get a MySQL database.** Render doesn't host MySQL directly — use an external provider (PlanetScale, Railway, Aiven, or your own server) and grab its connection string in the form:
   `server=<host>;port=3306;database=<db>;user=<user>;password=<password>;`
2. **Create the service.** In the Render dashboard: New → Blueprint → point it at this repo. It picks up `render.yaml` and `Dockerfile` automatically. (Or: New → Web Service → Runtime: Docker, if you'd rather configure it by hand.)
3. **Set the connection string.** `render.yaml` declares `ConnectionStrings__DefaultConnection` as a secret (`sync: false`) — Render will prompt you for its value during setup. Paste the connection string from step 1 there; don't put it in `appsettings.json`.
4. **Deploy.** Render builds the Dockerfile and starts the container on the `$PORT` it assigns. On first boot, `DbInitializer.SeedAsync` creates the schema and seeds roles + the default admin account (see [Setup](#3-create-the-database-and-apply-migrations) above) — no separate migration step needed.
5. Log in as `admin` / `Admin@1234` and change the password immediately (the forced-password-change flow will prompt you anyway).

### Running the container locally

```bash
docker build -t schoolapp .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="server=host.docker.internal;port=3306;database=SchoolDb;user=root;password=<your-password>;" \
  schoolapp
```

## Security notes

- Cookies are `HttpOnly`, `SecurePolicy = SameAsRequest`, 8-hour sliding expiration.
- Security headers set on every response: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `X-XSS-Protection`.
- `app.UseHsts()` + `UseHttpsRedirection()` in non-development environments.
- Forced password change middleware blocks all routes except login/logout/change-password until `MustChangePassword` is cleared.
