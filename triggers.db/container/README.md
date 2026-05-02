# triggers.db / container

Flyway-based migration runner for the `triggerdb` MSSQL database. Runs from the repo root via `docker compose`.

## Layout

```
container/
├── conf/
│   └── flyway.conf              # Flyway configuration (locations, naming, safety)
└── migrations/
    ├── versioned/               # V<n>__<name>.sql  — applied in order, immutable
    ├── repeatable/              # R__<name>.sql     — re-run whenever checksum changes
    └── callbacks/               # beforeMigrate.sql, afterMigrate.sql, etc.
```

Flyway's `locations` in `flyway.conf` points at all three folders.

## Adding migrations

- **Versioned** — bump the version: `V2__add_trigger_audit.sql`. Once applied, the file's checksum is locked.
- **Repeatable** — for views/procs/functions you want re-applied on change: `R__vw_active_triggers.sql`.
- **Callbacks** — lifecycle hooks named exactly: `beforeMigrate.sql`, `afterMigrate.sql`, etc.

## Commands (from repo root)

```bash
docker compose up -d mssql                          # start SQL Server
docker compose run --rm flyway-bootstrap            # create triggerdb if missing
docker compose run --rm flyway migrate              # apply pending migrations
docker compose run --rm flyway info                 # show migration history
docker compose run --rm flyway validate             # verify checksums
```

Or `docker compose up` to do all three in order.
