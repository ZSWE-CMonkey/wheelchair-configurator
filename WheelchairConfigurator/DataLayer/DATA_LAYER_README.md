# Data Layer — Wheelchair Configurator
**Version 1.4** | Author: Peta8 | C# / MAUI / SQLite-net-pcl

---

## Overview

This document describes the data layer of the Wheelchair Configurator application.
The data layer is responsible for:
- Database connection and schema initialization
- Seeding initial data from JSON files
- Providing repositories for reading and writing data

The layer is designed to be clean, scalable, and easy to extend.
All colleagues working on the engine or UI layer interact with this layer exclusively through repositories and `DbInitializer`.

---

## Project Structure

```
Data/
    DbService.cs            — SQLite connection, table creation, reset
    DbInitializer.cs        — entry point, checks DB and triggers seed
    JsonDataLoader.cs       — deserializes JSON files into DTOs
    
    DTOs/                   — data transfer objects (mirrors JSON structure)
        SeedDataDto.cs
        CategoryDto.cs
        ComponentDto.cs
        ComponentSpecsDto.cs
        Model3DDto.cs
        CompatibilityRuleDto.cs

    Providers/
        ILocalFileProvider.cs — contract for resolving seed file paths
        LocalFileProvider.cs — returns paths to JSON seed files

    Repositories/           — data access layer
        IRepository.cs                  — generic repository contract
        ICategoryRepository.cs          — category-specific contract
        IComponentRepository.cs         — component-specific contract
        IConfigurationRepository.cs     — configuration-specific contract
        IConfigurationItemRepository.cs — configuration item-specific contract
        ISpecialistRepository.cs        — specialist-specific contract
        GenericRepository.cs            — base CRUD implementation
        CategoryRepository.cs
        ComponentRepository.cs
        ComponentSpecsRepository.cs
        CompatibilityRuleRepository.cs
        SpecialistRepository.cs
        ConfigurationRepository.cs
        ConfigurationItemRepository.cs
        Model3DRepository.cs

    Seeding/                — responsible for populating the database
        DbSeeder.cs             — orchestrator, runs all seeders in order
        Seeders/
            CategorySeeder.cs
            ComponentSeeder.cs
            ComponentSpecsSeeder.cs
            Model3DSeeder.cs
            CompatibilityRuleSeeder.cs

Domain/
    Models/                 — database entities
        Category.cs
        Component.cs
        ComponentSpecs.cs
        Model3D.cs
        CompatibilityRule.cs
        Specialist.cs
        Configuration.cs
        ConfigurationItem.cs

Service/
    DataService.cs          — orchestrates JSON loading pipeline

Resources/
    seed_data.json          — initial seed data
```

---

## Dependency Flow

```
DbInitializer.Initialize()
    │
    ├── DbService               — opens connection, creates tables
    │
    └── DataService.ProcessData()
            ├── LocalFileProvider   — resolves file paths
            └── JsonDataLoader      — reads JSON → SeedDataDto
                    │
                    └── DbSeeder.Seed()
                            ├── CategorySeeder
                            ├── ComponentSeeder        (needs categoryMap)
                            ├── ComponentSpecsSeeder   (needs componentMap)
                            ├── Model3DSeeder          (needs componentMap)
                            └── CompatibilityRuleSeeder (needs componentMap)
```

---

## How to Use — Entry Point

Call this **once** at application startup (e.g. in `MauiProgram.cs`):

```csharp
var initializer = new DbInitializer(
    new DbService("konfigurator.db"),
    new DataService(new LocalFileProvider(), new JsonDataLoader())
);

// Production
initializer.Initialize();

// Development — drops and recreates all tables on every start
initializer.Initialize(resetOnStart: true);
```

`DbInitializer` checks whether the database already contains data.
If empty, it runs the full seeding pipeline automatically.

---

## Repository Pattern

All repositories follow the same pattern — generic CRUD is inherited,
entity-specific queries are added on top.

### Contract — `IRepository<T>`

```csharp
Task<List<T>> GetAllAsync();
Task<T?> GetByIdAsync(int id);
Task<int> InsertAsync(T entity);
Task<int> UpdateAsync(T entity);
Task<int> DeleteAsync(T entity);
```

### Available Repositories & Specific Methods

| Repository | Interface | Specific methods |
|---|---|---|
| `CategoryRepository` | `ICategoryRepository` | `GetByNameAsync(name)`, `GetByIdsAsync(ids)` |
| `ComponentRepository` | `IComponentRepository` | `GetByCategoryIdAsync(categoryId)`, `GetByNameAsync(name)`, `GetByIdsAsync(ids)` |
| `ComponentSpecsRepository` | — | `GetByComponentIdAsync(componentId)` |
| `CompatibilityRuleRepository` | — | `GetRulesForComponentAsync(componentId)`, `GetRuleAsync(compAId, compBId)`, `AreCompatibleAsync(compAId, compBId)` |
| `SpecialistRepository` | `ISpecialistRepository` | `GetByEmailAsync(email)`, `GetByClinicAsync(clinic)` |
| `ConfigurationRepository` | `IConfigurationRepository` | `GetBySpecialistIdAsync(specialistId)` |
| `ConfigurationItemRepository` | `IConfigurationItemRepository` | `GetByConfigurationIdAsync(configurationId)` |
| `Model3DRepository` | — | `GetByComponentIdAsync(componentId)` |

> Note: `ComponentSpecsRepository`, `CompatibilityRuleRepository` and `Model3DRepository` do not have
> specific interfaces yet as they are not injected into `AppService`. Add interfaces when needed.

### Usage Example

```csharp
// Get all components in a category
var components = await _componentRepo.GetByCategoryIdAsync(categoryId);

// Check compatibility between two components
bool? compatible = await _compatibilityRepo.AreCompatibleAsync(compAId, compBId);
// true  = compatible
// false = incompatible
// null  = no rule defined

// Save a new configuration
await _configurationRepo.InsertAsync(new Configuration
{
    SpecialistId = specialistId,
    CreatedAt = DateTime.Now
});

// Save configuration items
await _configurationItemRepo.InsertAsync(new ConfigurationItem
{
    ConfigurationId = configurationId,
    ComponentId = componentId,
    Quantity = 1
});
```

---

## Adding a New Entity

Follow these steps when extending the data model:

**1. Domain model** — `Domain/Models/NewEntity.cs`
```csharp
[Table("NewEntity")]
public class NewEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

**2. DTO** — `Data/DTOs/NewEntityDto.cs`
```csharp
public class NewEntityDto
{
    public string Name { get; set; } = string.Empty;
}
```

**3. `SeedDataDto`** — add one line
```csharp
public List<NewEntityDto> NewEntities { get; set; } = new();
```

**4. `DbService`** — add one line to `InitializeDatabase()` and `ResetDatabase()`
```csharp
_db.CreateTable<NewEntity>();
```

**5. Seeder** — `Data/Seeding/Seeders/NewEntitySeeder.cs`
```csharp
public class NewEntitySeeder
{
    public void Seed(SQLiteConnection db, List<NewEntityDto> dtos)
    {
        foreach (var dto in dtos)
            db.Insert(new NewEntity { Name = dto.Name });
    }
}
```

**6. `DbSeeder`** — add one line
```csharp
_newEntitySeeder.Seed(db, data.NewEntities);
```

**7. Repository** — `Data/Repositories/NewEntityRepository.cs`
```csharp
public class NewEntityRepository : GenericRepository<NewEntity>
{
    public NewEntityRepository(SQLiteAsyncConnection db) : base(db) { }

    // Add entity-specific queries here
}
```

**8. `seed_data.json`** — add data
```json
"NewEntities": [
    { "Name": "Example" }
]
```

---

## Adding a New Attribute to an Existing Entity

Even simpler — only 4 files change:

1. **Domain model** — add property
2. **DTO** — add property
3. **Seeder** — map new property
4. **`seed_data.json`** — add value

`DbService.CreateTable` handles schema changes automatically (`ALTER TABLE IF NOT EXISTS`).

---

## DI Registration (for MAUI colleague)

Register all dependencies in `MauiProgram.cs`.
Repositories are registered **under their interface** so they can be mocked in unit tests:

```csharp
var asyncDb = new DbService(dbPath).GetAsyncConnection();

builder.Services.AddSingleton<DbService>(sp => new DbService(dbPath));
builder.Services.AddSingleton<ILocalFileProvider, LocalFileProvider>();
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<DbInitializer>();

// Repositories — registered under interface for testability
builder.Services.AddSingleton<ICategoryRepository>(sp => new CategoryRepository(asyncDb));
builder.Services.AddSingleton<IComponentRepository>(sp => new ComponentRepository(asyncDb));
builder.Services.AddSingleton<IConfigurationRepository>(sp => new ConfigurationRepository(asyncDb));
builder.Services.AddSingleton<IConfigurationItemRepository>(sp => new ConfigurationItemRepository(asyncDb));
builder.Services.AddSingleton<ISpecialistRepository>(sp => new SpecialistRepository(asyncDb));

// Repositories without specific interfaces (not injected into AppService)
builder.Services.AddSingleton<ComponentSpecsRepository>(sp => new ComponentSpecsRepository(asyncDb));
builder.Services.AddSingleton<CompatibilityRuleRepository>(sp => new CompatibilityRuleRepository(asyncDb));
builder.Services.AddSingleton<Model3DRepository>(sp => new Model3DRepository(asyncDb));
```

Then call at startup:
```csharp
var initializer = app.Services.GetRequiredService<DbInitializer>();
initializer.Initialize();
```

---

## seed_data.json Structure

```json
{
  "Categories": [
    { "Name": "Wheels" }
  ],
  "Components": [
    {
      "Name": "SportWheel X1",
      "CategoryName": "Wheels",
      "CatalogUrl": "https://example.com",
      "Price": 299.99
    }
  ],
  "Specs": [
    {
      "ComponentName": "SportWheel X1",
      "WeightCapacityKg": 120,
      "SeatWidthCm": 45,
      "MaxSpeedKmh": 10
    }
  ],
  "Models3D": [
    {
      "ComponentName": "SportWheel X1",
      "FilePath": "models/sportwheel_x1.obj",
      "TextureId": "tex_001",
      "AnchorX": 0.0,
      "AnchorY": 0.0,
      "AnchorZ": 0.0
    }
  ],
  "Rules": [
    {
      "ComponentAName": "SportWheel X1",
      "ComponentBName": "BasicFrame A",
      "IsCompatible": true
    }
  ]
}
```

**Important:** Components reference categories by **name**, not by ID.
The seeder resolves names to database IDs automatically.

---

## Known Limitations (v1.4)

- `SpecialistSeeder` not implemented — specialists are expected to be created by the user in the app, not seeded from JSON. Can be added in v1.5 if needed.
- `ComponentSpecsRepository`, `CompatibilityRuleRepository` and `Model3DRepository` do not have specific interfaces yet — add when needed.
- No JSON validation — malformed entries are skipped with a console warning.
- No migration history — to reset data during development, use `resetOnStart: true` or delete `konfigurator.db` manually.

---

## Changelog

| Version | Change |
|---|---|
| 1.4 | Added `ILocalFileProvider` interface; `DbService.Close()` for explicit connection cleanup; `DataService` now depends on `ILocalFileProvider` instead of concrete class |
| 1.3 | Added specific repository interfaces (`ICategoryRepository`, `IComponentRepository`, `IConfigurationRepository`, `IConfigurationItemRepository`, `ISpecialistRepository`) for clean DI and unit testability |
| 1.2 | Initial release |



--------
Developed by Claude Sonnet 4.6 <3 |
Consulted with Gemini 3.1 Pro <3 |
Managed by Peta 8-)  