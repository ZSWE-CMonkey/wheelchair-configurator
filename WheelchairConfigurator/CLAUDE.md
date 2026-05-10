# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build for Windows (run from WheelchairConfigurator/ directory)
dotnet build WheelchairConfigurator.sln -f net8.0-windows10.0.19041.0

# Run on Windows
dotnet run --project WheelchairConfigurator/WheelchairConfigurator.csproj -f net8.0-windows10.0.19041.0

# Build for Android
dotnet build WheelchairConfigurator.sln -f net8.0-android

# Run tests (all projects)
dotnet test WheelchairConfigurator.sln

# Run a single test project
dotnet test WheelchairConfigurator.DataLayer.Tests/WheelchairConfigurator.DataLayer.Tests.csproj
dotnet test WheelchairConfigurator.ServiceLayer.Tests/WheelchairConfigurator.ServiceLayer.Tests.csproj
dotnet test ExportLayerTests/ExportLayer.Tests.csproj
```

## Solution Structure

```
WheelchairConfigurator.sln
├── WheelchairConfigurator/      # MAUI UI app — Pages, Components, MauiProgram.cs
├── DataLayer/                   # SQLite persistence (sqlite-net-pcl)
├── ServiceLayer/                # Business orchestration — IAppService, AppService
├── ConfigurationLogic/          # Engine + C# Vulkan bridge (Bazilišek class)
├── ExportLayer/                 # PDF generation via PDFsharp-MigraDoc
├── WheelchairConfigurator.DataLayer.Tests/
├── WheelchairConfigurator.ServiceLayer.Tests/
├── ExportLayerTests/
└── DATA/                        # SRS document (reference only)
```

## Current Integration Status (v0.14.0)

**The UI is using hardcoded mock data.** The backend layers (DataLayer, ServiceLayer, ExportLayer) are implemented and tested, but the pages have not been wired to `IAppService`. This is the primary task remaining before release.

### Pages and their mock status

| Page | File | Mock classes | Status |
|---|---|---|---|
| `NewPatientPage` | `Pages/NewPatientPage.xaml.cs` | `UserInput` | Collects real input, but data is never passed forward |
| `PatientSelectPage` | `Pages/PatientSelectPage.xaml.cs` | `PatientMock`, `WheelchairMock` | Fully mocked |
| `WheelchairConfiguratorPage` | `Pages/WheelchairConfiguratorPage.xaml.cs` | `PatientData`, `ComponentMock` | Fully mocked |
| `SummaryPage` | `Pages/SummaryPage.xaml.cs` | `PatientData`, `ComponentMock` | Fully mocked |
| `ComponentManagerPage` | `Pages/ComponentManagerPage.xaml.cs` | `CategoryMock`, `ComponentItemMock` | Fully mocked, save/remove are TODO stubs |

Mock class definitions live in `Mocks.cs` (app-level classes: `PatientData`, `ComponentMock`, `ComponentCategories`) and inline at the top of each page file.

### What `IAppService` already provides

```csharp
Task<List<CategoryModel>>     GetCategoriesAsync();
Task<List<ComponentModel>>    GetComponentsAsync(int categoryId, PatientProfileModel? patient = null);
Task<ConfigurationResult>     ValidateConfigurationAsync(ConfigurationRequest request);
Task<ConfigurationResult>     SaveConfigurationAsync(ConfigurationRequest request);
Task<List<ConfigurationModel>> GetConfigurationsBySpecialistAsync(int specialistId);
Task<byte[]>                  ExportConfigurationAsync(int configurationId);
```

### What MauiProgram.cs is missing

- `DbInitializer` is not registered and `Initialize()` is never called at startup — the database is never seeded
- `IExportFileBuilder` (PdfBuilder) is not registered — `ExportConfigurationAsync` will crash
- Font and logo loading for the PDF builder (see `ExportLayer/EXPORT_LAYER_README.md`)
- Repositories are registered as concrete types instead of interfaces — `AppService` constructor requires the interface types

## Architecture: Data Flow Between Layers

```
NewPatientPage (UserInput filled by user)
    │  → navigates to WheelchairConfiguratorPage
    │    UserInput must be passed as QueryProperty or via a shared state object
    │
WheelchairConfiguratorPage
    │  calls: IAppService.GetCategoriesAsync()
    │  calls: IAppService.GetComponentsAsync(categoryId, patientProfile)
    │  user selects one component per category
    │  → navigates to SummaryPage passing selected component IDs
    │
SummaryPage
    │  displays selected components
    │  calls: IAppService.SaveConfigurationAsync(request) on confirm
    │  calls: IAppService.ExportConfigurationAsync(configId) on export
    │
PatientSelectPage
    │  calls: IAppService.GetConfigurationsBySpecialistAsync(specialistId)
    │  shows specialists' past configurations
    │
ComponentManagerPage (Supplier Admin role)
    │  calls: IAppService.GetCategoriesAsync()
    │  calls: IAppService.GetComponentsAsync(categoryId)
    │  save/remove are currently TODO stubs — IAppService has no add/remove component methods yet
```

## DI Registration (correct pattern)

The current `MauiProgram.cs` registers repositories as concrete types. `AppService` requires interfaces. The correct registration pattern (from `DataLayer/DATA_LAYER_README.md`):

```csharp
var asyncDb = new DbService(dbPath).GetAsyncConnection();

builder.Services.AddSingleton<DbService>(sp => new DbService(dbPath));
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<DbInitializer>();

// Repositories under their interface
builder.Services.AddSingleton<ICategoryRepository>(sp => new CategoryRepository(asyncDb));
builder.Services.AddSingleton<IComponentRepository>(sp => new ComponentRepository(asyncDb));
builder.Services.AddSingleton<IConfigurationRepository>(sp => new ConfigurationRepository(asyncDb));
builder.Services.AddSingleton<IConfigurationItemRepository>(sp => new ConfigurationItemRepository(asyncDb));
builder.Services.AddSingleton<ISpecialistRepository>(sp => new SpecialistRepository(asyncDb));

// Export
builder.Services.AddSingleton<IExportFileBuilder>(sp => new PdfBuilder(logoBytes));

// Engine + Service
builder.Services.AddSingleton<IConfigurationEngine, ConfigurationEngineAdapter>();
builder.Services.AddSingleton<IAppService, AppService>();
```

Then in `App.xaml.cs` or just after `builder.Build()`:
```csharp
var initializer = app.Services.GetRequiredService<DbInitializer>();
initializer.Initialize(); // seeds DB from seed_data.json on first run
```

## Key Patterns

### Passing data between pages (MAUI Shell navigation)
Use `[QueryProperty]` on the target page and encode parameters in the route:
```csharp
await Shell.Current.GoToAsync($"wheelchairConfiguratorPage?wheelchairId={id}");
// On WheelchairConfiguratorPage:
[QueryProperty(nameof(WheelchairId), "wheelchairId")]
```
For complex objects (like `UserInput`), use a singleton service registered in DI to hold transient navigation state.

### ServiceLayer models vs domain entities
- UI only ever sees `CategoryModel`, `ComponentModel`, `ConfigurationModel`, `PatientProfileModel`, `ConfigurationRequest`, `ConfigurationResult` from `ServiceLayer/Models/`
- `ComponentModel.IsRecommended` / `IsIncompatible` are engine-set at call time, never stored in DB
- `ConfigurationResult.IsSuccess` is false if validation fails; always check before proceeding

### Configuration engine
`ConfigurationEngineAdapter` in `ConfigurationLogic/` wraps the real engine. `MockEngine` (same folder) always returns all components as recommended and always validates as valid — useful for testing the UI without clinical rules.

### Bazilišek (3D rendering)
`Helpers/Bazilišek.cs` wraps the Vulkan graphics library. Used in `WheelchairConfiguratorPage` and `SummaryPage` for the 3D render loop. Missing 3D assets are non-fatal — show a "Visual Incompleteness" warning but do not block the flow.

### seed_data.json
Located at `WheelchairConfigurator/Resources/Raw/seed_data.json`. On Android this file must be copied to `FileSystem.AppDataDirectory` before `DbInitializer.Initialize()` runs. The copy-on-first-run block exists in `MauiProgram.cs` but is commented out — uncomment it.

## Adding IAppService Methods Not Yet Present

`ComponentManagerPage` needs add/remove component operations that don't exist in `IAppService`. To add them:
1. Add method signatures to `ServiceLayer/Interfaces/IAppService.cs`
2. Implement in `ServiceLayer/AppService.cs` using `IComponentRepository`
3. Update `WheelchairConfigurator.ServiceLayer.Tests/AppServiceTest.cs`

## Test Commands

```bash
# Run specific test by name
dotnet test --filter "FullyQualifiedName~AppServiceTest"

# Run with verbose output
dotnet test -v normal
```
