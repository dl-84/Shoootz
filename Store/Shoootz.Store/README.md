# Shoootz.Store — Migrations

## SQLite

```bash
dotnet ef migrations add <MigrationName> \
  --project Store/Shoootz.Store/Adapter/Shoootz.Store.Adapter.SQLite \
  --startup-project Store/Shoootz.Store/Adapter/Shoootz.Store.Adapter.SQLite \
  --output-dir Migrations
```

## PostgreSQL

```bash
dotnet ef migrations add <MigrationName> \
  --project Store/Shoootz.Store/Adapter/Shoootz.Store.Adapter.PostgreSQL \
  --startup-project Store/Shoootz.Store/Adapter/Shoootz.Store.Adapter.PostgreSQL \
  --output-dir Migrations
```

> Befehle vom Solution-Root (`Shoootz/`) ausführen.
