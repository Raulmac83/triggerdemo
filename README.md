# triggerdemo

A side-by-side comparison of four ways to react to EF Core entity changes:

1. **`SaveChangesInterceptor`** — `triggers.events.interceptor`
2. **`EntityFrameworkCore.Triggered`** — `triggers.events.efcoretriggered`
3. **Domain events** (interceptor + dispatcher) — `triggers.events.domain`
4. **Audit.NET** data provider — `triggers.events.audit`

All four are wired into the same `AppDbContext`. A small ASP.NET Core 8 API (`triggers.api`) and a Vite + React + MUI admin UI (`triggers.web`) let you flip between methods at runtime and watch the resulting notifications fire.

---

## Prerequisites

Install these first:

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 8.0+ | `dotnet --version` |
| Node.js | 20+ | `node --version` (ships with npm) |
| Docker Desktop | recent | Used to run SQL Server + Flyway migrations |
| SQL Server | 2019+ | Run as a container or local install — see below |
| `gh` (optional) | 2.x | Only if you want `gh repo clone` |

---

## 1. Clone the repo

```bash
git clone https://github.com/Raulmac83/triggerdemo.git
cd triggerdemo
```

## 2. Start SQL Server

The connection string in [`triggers.api/appsettings.json`](triggers.api/appsettings.json) expects SQL Server at `localhost,1433` with `sa` / `Admin@1234!`. The simplest way to get that running is the official Microsoft container:

```bash
docker run -d \
  --name triggers-mssql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Admin@1234!" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

> **Apple Silicon (M1/M2/M3/M4):** use `mcr.microsoft.com/azure-sql-edge:latest` instead — the regular SQL Server image isn't `arm64`.

If you already have SQL Server running locally with different credentials, update [`triggers.api/appsettings.json`](triggers.api/appsettings.json) → `ConnectionStrings:Default` to match.

## 3. Create the database and apply migrations

The repo ships a `docker-compose.yml` that creates the `triggerdb` database and runs all Flyway migrations against the SQL Server you started in step 2.

```bash
# Optional: override defaults via .env (copy the example)
cp .env.example .env

# Bootstrap the DB and apply migrations
docker compose run --rm flyway-bootstrap
docker compose run --rm flyway
```

What this does:

- `flyway-bootstrap` waits for SQL Server, then `CREATE DATABASE triggerdb` if missing.
- `flyway` applies every migration under [`triggers.db/container/migrations/versioned/`](triggers.db/container/migrations/versioned/) in order:
  - `V1__init_triggers.sql` — `Triggers` table
  - `V2__create_auth_tables.sql` — `Users`, `Roles`, `UserRoles`, `RefreshTokens`
  - `V3__seed_admin_user.sql` — seeds **`admin` / `Password123!`**
  - `V4__create_notifications.sql` — `Notifications` table
  - `V5__notifications_add_trigger_method.sql` — adds method column

> On Linux, you may need to point Flyway at the host: `MSSQL_HOST=172.17.0.1 docker compose run --rm flyway-bootstrap` (etc.). On macOS/Windows the default `host.docker.internal` works.

## 4. Run the API

```bash
dotnet restore
dotnet run --project triggers.api
```

The API listens on **http://localhost:5208** (HTTPS on 7054). Swagger UI is at http://localhost:5208/swagger.

## 5. Run the web UI

In a second terminal:

```bash
cd triggers.web
npm install
npm run dev
```

Open **http://localhost:5173** and log in as **`admin` / `Password123!`**.

The frontend reads its API base URL from [`triggers.web/.env.local`](triggers.web/.env.local). Create it if you don't have it:

```bash
echo "VITE_API_URL=http://localhost:5208" > triggers.web/.env.local
```

---

## Trying the four trigger methods

1. Log in to the web UI.
2. Open **Trigger Methods** in the sidebar and switch the active method (Interceptor, EFCoreTriggered, DomainEvents, AuditNet).
3. Open **Triggers** and create / edit / delete a row.
4. Open **Notifications** to see the row each pipeline produced. Only the active method writes; the others stay quiet.

The exact handler for each method lives in `triggers.repo/Notifications/`:

- `InterceptorHandler.cs`
- `EFCoreTriggeredHandler.cs`
- `DomainEventsHandler.cs`
- `AuditHandler.cs`

---

## Project layout

```
triggers.api/                  ASP.NET Core 8 API + JWT auth + Swagger + CORS
triggers.web/                  Vite + React + MUI admin UI
triggers.db/                   AppDbContext, EF Core entities, Flyway migrations
triggers.repo/                 Repositories + per-method notification handlers
triggers.events.interceptor/   SaveChangesInterceptor pipeline
triggers.events.efcoretriggered/ EntityFrameworkCore.Triggered wrapper
triggers.events.domain/        Domain events via interceptor + dispatcher
triggers.events.audit/         Audit.NET data provider
docker-compose.yml             SQL Server bootstrap + Flyway migrate
```

---

## Troubleshooting

- **`Cannot connect to localhost,1433`** — the SQL Server container isn't running, or port 1433 is taken. Check `lsof -i :1433`.
- **`Login failed for user 'sa'`** — password mismatch between the container env var and `appsettings.json` / `.env`.
- **`Failed to create trigger (500)`** in the UI — confirm the API console; common causes are a stale build (restart the API after pulling) or the DB not being migrated.
- **Web shows `Failed to fetch`** — the API isn't running on `:5208`, or `VITE_API_URL` isn't pointing at it.

---

## Credentials & secrets

The values committed in `appsettings.json` and `.env.example` (`Admin@1234!`, the JWT signing key, `admin / Password123!`) are **demo-only**. Replace them in any real deployment.
