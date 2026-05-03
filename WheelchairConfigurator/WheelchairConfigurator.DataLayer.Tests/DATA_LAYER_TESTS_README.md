# Data Layer Tests — Wheelchair Configurator
**Version 1.0** | Author: Peta8 | xUnit / Moq / SQLite-net-pcl

---

## Overview

This document describes the test coverage for the **Data Layer** of the Wheelchair Configurator application.
Tests are split into two categories:

- **Unit tests** — no database, no file system. Dependencies mocked via Moq or real temp files.
- **Integration tests** — real SQLite database (`:memory:` or temp file). No mocks for the DB itself.

---

## Project Structure

```
DataLayer.Tests/
    JsonDataLoaderTest.cs       — unit
    DataServiceTest.cs          — unit
    DbServiceTest.cs            — integration
    DbInitializerTest.cs        — integration
    Seeding/
        CategorySeederTest.cs
        ComponentSeederTest.cs
        ComponentSpecsSeederTest.cs
        Model3DSeederTest.cs
        CompatibilityRuleSeederTest.cs
        DbSeederTest.cs
```

---

## Unit Tests

### `JsonDataLoaderTest`

Tests `JsonDataLoader.LoadData(string filePath) → SeedDataDto?`.
Uses real temporary JSON files — no mocks needed. Files are deleted after each test.

| Group | What it verifies |
|---|---|
| **File not found** | Non-existent path and empty string return null without throwing |
| **Valid JSON** | Categories, components, compatibility rules and empty collections are parsed correctly |
| **JSON options** | Case-insensitive property names, `//` comments, and trailing commas all parse without error — these directly reflect the `JsonSerializerOptions` configured in the class |
| **Invalid JSON** | Malformed JSON, empty file, and JSON array at root all return null instead of throwing |
| **Missing fields** | Collections absent from the JSON file default to empty lists; optional `CatalogUrl` defaults to null |

---

### `DataServiceTest`

Tests `DataService.ProcessData() → List<SeedDataDto>`.
`ILocalFileProvider` is mocked via Moq — controls which file paths are returned per test.
`JsonDataLoader` uses real temp files.

| Group | What it verifies |
|---|---|
| **No files** | Empty path array returns empty list |
| **Single valid file** | Returns one DTO with correct category count |
| **Multiple valid files** | Returns one DTO per file, each with correct data |
| **Invalid / missing files** | Invalid JSON and non-existent paths are skipped silently — no exception thrown |
| **Mixed files** | Only successfully parsed files appear in the result; failed ones are dropped |
| **Provider call count** | `GetSeedFilePaths()` is called exactly once — verified via `Verify(..., Times.Once)` |

---

## Integration Tests

All integration tests use **SQLite in-memory** (`:memory:`) unless noted.
Each test creates its own isolated database — no shared state between tests.

---

### `CategorySeederTest`

Tests `CategorySeeder.Seed(db, dtos) → Dictionary<string, int>`.

| Group | What it verifies |
|---|---|
| **Insert behavior** | Single, multiple and empty lists insert the correct number of rows |
| **RoleKey normalization** | Input is trimmed and converted to lowercase; null, empty or whitespace falls back to `"unknown"` |
| **Return map** | Returned dictionary contains all inserted names with non-zero, unique IDs matching the actual database IDs; empty input returns empty map |

---

### `ComponentSeederTest`

Tests `ComponentSeeder.Seed(db, dtos, categoryMap) → Dictionary<string, int>`.

| Group | What it verifies |
|---|---|
| **Insert behavior** | Single, multiple and empty lists insert correctly |
| **Field mapping** | `Name`, `Price`, `CatalogUrl` (including null) and resolved `CategoryId` are persisted correctly |
| **Skip behavior** | Components whose `CategoryName` is not in the map are silently skipped; skipped components do not appear in the returned map |
| **Return map** | Count, keys and IDs match inserted rows; skipped entries are absent |

---

### `ComponentSpecsSeederTest`

Tests `ComponentSpecsSeeder.Seed(db, dtos, componentMap)`.

| Group | What it verifies |
|---|---|
| **Insert behavior** | Single, multiple and empty lists insert correctly |
| **Field mapping** | All 15 spec fields are persisted correctly in a single assertion block; `ComponentId` is resolved from the map |
| **Skip behavior** | Unknown component names are silently skipped |

---

### `Model3DSeederTest`

Tests `Model3DSeeder.Seed(db, dtos, componentMap)`.

| Group | What it verifies |
|---|---|
| **Insert behavior** | Single, multiple and empty lists insert correctly |
| **Field mapping** | `FilePath`, `TextureId`, `AnchorX/Y/Z` and resolved `ComponentId` are persisted correctly |
| **Skip behavior** | Unknown component names are silently skipped |

---

### `CompatibilityRuleSeederTest`

Tests `CompatibilityRuleSeeder.Seed(db, dtos, componentMap)`.

| Group | What it verifies |
|---|---|
| **Insert behavior** | Single, multiple and empty lists insert correctly |
| **Field mapping** | `ComponentAId`, `ComponentBId` are resolved from the map; `IsCompatible` true and false are both persisted correctly |
| **Skip behavior** | Rule is skipped if either component A or component B is missing from the map; mixed valid/invalid input inserts only the valid rules |

---

### `DbSeederTest`

Tests `DbSeeder.Seed(db, seedData)` — the orchestrator that runs all seeders in order inside a single transaction.

| Group | What it verifies |
|---|---|
| **Full pipeline** | All five entity types (categories, components, specs, models, rules) are inserted when full seed data is provided |
| **Empty data** | No rows are inserted when all collections are empty |
| **Dependency order** | Components correctly reference category IDs from the same seed run; specs correctly reference component IDs |
| **Transaction — rollback** | When an exception occurs mid-seed (simulated by dropping the `CompatibilityRule` table), the entire transaction is rolled back — categories and components inserted before the crash are also removed |
| **Exception propagation** | The original `SQLiteException` is rethrown after rollback |

---

### `DbServiceTest`

Tests `DbService` schema initialization, schema upgrades and reset.
Uses a **temporary file-based** SQLite database because `DbService` creates two internal connections
(sync + async) and each `:memory:` connection is isolated.
Temp files are deleted after each test via `IDisposable`. All `DbService` instances are explicitly
closed via `Close()` before deletion to release the file handle.

| Group | What it verifies |
|---|---|
| **Table creation** | All 8 domain tables exist after construction and are writable |
| **Schema upgrades** | `RoleKey` on `Category` and `SeatDepthCm` on `ComponentSpecs` exist after construction |
| **Idempotency** | Creating a second `DbService` on the same file does not throw and does not duplicate columns — `TryAddColumn` is safe to call repeatedly |
| **ResetDatabase** | Clears all existing data, tables still exist and are writable after reset |
| **Connections** | `GetConnection()` and `GetAsyncConnection()` return non-null; `GetConnection()` returns the same instance on each call |

---

### `DbInitializerTest`

Tests `DbInitializer.Initialize(bool resetOnStart)` — the top-level entry point.
Uses a temporary file-based SQLite database and a real `DataService` with `ILocalFileProvider` mocked via Moq.

| Group | What it verifies |
|---|---|
| **Empty database** | Seeding runs when the database is empty; empty seed file results in zero rows |
| **Already seeded** | Second call to `Initialize()` skips seeding — row count stays the same; no exception is thrown |
| **resetOnStart = true** | Existing data is cleared and re-seeded from the current seed file; tables still exist after reset; replacing the seed file with empty data results in an empty database after reset |
| **Default parameter** | Calling `Initialize()` without arguments does not reset — equivalent to `resetOnStart: false` |

---

## What Is NOT Tested Here

| Area | Reason |
|---|---|
| Repository SQL queries | No custom logic — pure SQLite wrappers. Covered indirectly by seeder and initializer tests. |
| `LocalFileProvider` | Single line returning a hardcoded path — no logic to test |
| Domain models / DTOs | Plain data classes with no logic |
| DI wiring in `MauiProgram.cs` | Verified at runtime |
