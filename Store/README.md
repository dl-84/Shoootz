# Shoootz.Store — Migrations

> **Wichtig:** Bei jeder Schema-Änderung müssen **beide** Befehle ausgeführt werden — SQLite und PostgreSQL haben jeweils eine eigene, unabhängige Migrations-Historie.

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
