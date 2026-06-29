# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**UTN FRH – Programación III 2026 – TP Integrador: "Progra3card"**

A financial card statement system built across two platforms sharing one MySQL database (`mi_banco_db`):
- **Phase 1 – Admin Console (C#):** Bank employees issue cards and upload monthly statements.
- **Phase 2 – Web Portal (PHP):** Cardholders activate their account and view statements.

## Database Setup

Import the provided schema and seed data before running either platform:

```bash
mysql -u root -p < mi_banco_db.sql
```

The connection string in `Progra3card.cs` is hardcoded: `Server=localhost;Database=mi_banco_db;Uid=root;Pwd=;`

## C# Console Application

**Compile** (requires [MySql.Data NuGet package](https://www.nuget.org/packages/MySql.Data) / MySQL Connector for .NET):

```bash
# With .NET CLI (add a .csproj first, or open in Visual Studio)
dotnet run

# Direct compile with csc (if MySql.Data.dll is available locally)
csc Progra3card.cs -r:MySql.Data.dll
```

**Run:** `dotnet run` or execute the compiled binary. MySQL must be running first.

## PHP Web Application

Serve from the project root:

```bash
php -S localhost:8000
```

Then open `http://localhost:8000/registro.html` (activation) or `http://localhost:8000/ingreso.html` (login).

## What Needs to Be Implemented

### C# (`Progra3card.cs`)
Three stub methods to complete (lines 120–136):
- `ObtenerYMostrarTarjetas()` — `SELECT` on `tarjetas`, print rows.
- `MostrarDetalleCompleto(int cuenta)` — `SELECT … JOIN usuarios ON tarjetas` filtered by `num_cuenta`.
- `DarDeBajaTarjeta(int cuenta)` — `DELETE FROM tarjetas WHERE num_cuenta = @cuenta`; cascade handles `liquidaciones`.

`MenuEmitirTarjeta()` and `MenuEmitirLiquidacion()` are called from the menu switch but not yet defined — they need to be added.

### PHP (files to create from scratch)
- `altas.php` — receives DNI from `registro.html`; verifies a `tarjetas` row exists for that document; if so, `UPDATE usuarios SET usuario=?, password=?` (plain text per assignment spec).
- `ingreso.php` — validates credentials against `usuarios`, starts a PHP session, redirects to `resumen.php`.
- `resumen.php` — session-gated page; `JOIN tarjetas + liquidaciones + usuarios` to display the latest statement and statement history.

## Database Schema Key Points

| Table | PK | Notable constraints |
|---|---|---|
| `usuarios` | `documento` | `usuario`/`password` are NULL until web activation |
| `tarjetas` | `num_cuenta` | `dni_titular` FK → `usuarios.documento` (UNIQUE, CASCADE DELETE) |
| `liquidaciones` | `id_liquidacion` | `num_cuenta` FK → `tarjetas` (CASCADE DELETE) |

- `tarjetas.banco_emisor` is an ENUM: `'Banco Nación','Galicia','Santander','BBVA','Macro','Provincia'`
- `tarjetas.estado` is an ENUM: `'Activa','Bloqueada'`
- `liquidaciones.periodo` format: `YYYY-MM`
- Passwords are stored **plain text** by assignment design — no hashing required.

## Architecture

```
mi_banco_db (MySQL)
    ├── usuarios
    ├── tarjetas  (1:1 with usuarios via dni_titular UNIQUE FK)
    └── liquidaciones  (many:1 with tarjetas)

Progra3card.cs  ──── direct MySQL connection ────▶  mi_banco_db
                     (admin: CRUD on all tables)

registro.html  ──▶  altas.php   ─── UPDATE usuarios ──▶  mi_banco_db
ingreso.html   ──▶  ingreso.php ─── SELECT + session ──▶  mi_banco_db
                    resumen.php ─── SELECT + JOINs ──▶    mi_banco_db
```
