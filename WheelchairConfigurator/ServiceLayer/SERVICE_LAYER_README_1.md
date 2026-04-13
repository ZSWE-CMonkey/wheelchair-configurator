# Service Layer — Wheelchair Configurator

**Version 1.0** | Author: Peta8 | C# / MAUI

---

## Overview

The service layer is the central orchestrator of the application.
It connects the UI, DataLayer, ExportLayer and ConfigurationEngine.
UI communicates exclusively through `IAppService` — it never touches repositories,
domain entities or PDF builders directly.

---

## Project Structure

```
ServiceLayer/
    AppService.cs               — main orchestrator

    Interfaces/
        IAppService.cs          — contract for UI
        IConfigurationEngine.cs — contract for engine (ConfigurationLogic colleague)

    Models/
        CategoryModel.cs        — category for UI
        ComponentModel.cs       — component for UI (includes IsRecommended, IsIncompatible)
        ConfigurationModel.cs   — saved configuration for UI
        ConfigurationRequest.cs — data sent from UI when saving a configuration
        ConfigurationResult.cs  — result returned to UI after any operation
        PatientProfileModel.cs  — patient anthropometric and clinical data
        TrunkStabilityLevel.cs  — enum for trunk stability (Good/Fair/Poor)

    Mapping/
        CategoryMapper.cs       — Category entity → CategoryModel
        ComponentMapper.cs      — Component entity → ComponentModel
        ConfigurationMapper.cs  — Configuration entity ↔ ConfigurationRequest/Model
        (Featured) ExportMapper.cs         — DB entities → ConfigurationExportModel for PDF
```

---

## Dependency Flow

```
UI
 └── IAppService (AppService)
         │
         ├── CategoryRepository          → GetCategoriesAsync()
         ├── ComponentRepository         → GetComponentsAsync()
         ├── ConfigurationRepository     → SaveConfigurationAsync()
         ├── ConfigurationItemRepository → SaveConfigurationAsync()
         ├── SpecialistRepository        → ExportConfigurationAsync()
         │
         ├── IConfigurationEngine        → GetRecommendedComponentIdsAsync()
         │                                 ValidateAsync()
         │
         ├── (Featured) ExportMapper                → assembles ConfigurationExportModel
         │
         └── (Featured) IExportFileBuilder          → Build(model) → PDF file
```

---

## Models Overview

### `ConfigurationRequest` — from UI to service

```csharp
{
    SpecialistId:          int                // who is creating the configuration
    SelectedComponentIds:  List<int>          // what components were selected
    Patient:               PatientProfileModel? // optional patient data for engine
}
```

### `ConfigurationResult` — from service to UI

```csharp
{
    IsSuccess:       bool     // did the operation succeed?
    Message:         string   // human-readable result or error description
    ConfigurationId: int?     // set on successful save
}
```

### `ComponentModel` — component displayed in UI

```csharp
{
    Id:             int
    Name:           string
    Price:          decimal
    CatalogUrl:     string?
    IsRecommended:  bool    // flagged by engine based on patient profile
    IsIncompatible: bool    // flagged by engine based on current selection
}
```

### `PatientProfileModel` — clinical parameters

```csharp
{
    PelvisWidthCm:        int   // determines seat width
    ThighLengthCm:        int   // determines seat depth
    LowerLegLengthCm:     int   // determines footrest length
    WeightKg:             int   // determines chassis weight capacity
    TrunkStability:       TrunkStabilityLevel (Good = 1 / Fair = 2 / Poor = 3)
    HasPressureSoresRisk: bool  // determines cushion type
}
```

---

## How to Use (for UI colleague)

### 1. Register in `MauiProgram.cs`

```csharp
// QuestPDF license
// QuestPDF.Settings.License = LicenseType.Community;

// DataLayer
var dbService = new DbService(dbPath);
var asyncDb   = dbService.GetAsyncConnection();

builder.Services.AddSingleton(sp => dbService);
builder.Services.AddSingleton(sp => new DbInitializer(dbService, new DataService(new LocalFileProvider(), new JsonDataLoader())));
builder.Services.AddSingleton(sp => new CategoryRepository(asyncDb));
builder.Services.AddSingleton(sp => new ComponentRepository(asyncDb));
builder.Services.AddSingleton(sp => new ConfigurationRepository(asyncDb));
builder.Services.AddSingleton(sp => new ConfigurationItemRepository(asyncDb));
builder.Services.AddSingleton(sp => new SpecialistRepository(asyncDb));

// ExportLayer
// builder.Services.AddSingleton<IExportFileBuilder, PdfBuilder>();

// Engine (ConfigurationLogic colleague)
builder.Services.AddSingleton<IConfigurationEngine, YourEngineImplementation>();

// ServiceLayer
builder.Services.AddSingleton<IAppService, AppService>();
```

Then call at startup:

```csharp
var initializer = app.Services.GetRequiredService<DbInitializer>();
initializer.Initialize();
```

### 2. Use from ViewModel

```csharp
public class MainViewModel
{
    private readonly IAppService _appService;

    public MainViewModel(IAppService appService)
    {
        _appService = appService;
    }
}
```

---

## Use Cases

### Load categories

```csharp
var categories = await _appService.GetCategoriesAsync();
// returns List<CategoryModel>
```

### Load components — without patient

```csharp
var components = await _appService.GetComponentsAsync(categoryId);
// returns List<ComponentModel> — IsRecommended and IsIncompatible are false
```

### Load components — with patient profile

```csharp
var patient = new PatientProfileModel
{
    WeightKg             = 85,
    PelvisWidthCm        = 38,
    ThighLengthCm        = 42,
    LowerLegLengthCm     = 40,
    TrunkStability       = TrunkStabilityLevel.Fair,
    HasPressureSoresRisk = true
};

var components = await _appService.GetComponentsAsync(categoryId, patient);
// engine flags IsRecommended and IsIncompatible on each component
```

### Validate selection

```csharp
var result = await _appService.ValidateConfigurationAsync(new ConfigurationRequest
{
    SpecialistId         = 1,
    SelectedComponentIds = new List<int> { 1, 3, 5 },
    Patient              = patient
});

if (!result.IsSuccess)
    ShowError(result.Message);
```

### Save configuration

```csharp
// Validation runs automatically before saving
var result = await _appService.SaveConfigurationAsync(new ConfigurationRequest
{
    SpecialistId         = 1,
    SelectedComponentIds = new List<int> { 1, 3, 5 },
    Patient              = patient
});

if (result.IsSuccess)
    Console.WriteLine($"Saved! ID: {result.ConfigurationId}");
else
    ShowError(result.Message);
```

### Export to PDF

```csharp
// string pdfPath = await _appService.ExportConfigurationAsync(result.ConfigurationId!.Value);
// open or share the PDF file
```

### Load past configurations

```csharp
var history = await _appService.GetConfigurationsBySpecialistAsync(specialistId);
// returns List<ConfigurationModel>
```

---

## For Engine Colleague

Implement `IConfigurationEngine` in your `ConfigurationLogic` project:

```csharp
public class YourEngine : IConfigurationEngine
{
    /// <summary>
    /// Returns IDs of components that are safe and recommended for this patient.
    /// </summary>
    public async Task<List<int>> GetRecommendedComponentIdsAsync(
        PatientProfileModel patient,
        List<ComponentModel> availableComponents)
    {
        // Example: filter by weight capacity
        return availableComponents
            .Where(c => c.WeightCapacityKg >= patient.WeightKg)
            .Select(c => c.Id)
            .ToList();
    }

    /// <summary>
    /// Validates selected components against clinical rules.
    /// </summary>
    public async Task<ConfigurationResult> ValidateAsync(
        ConfigurationRequest request,
        List<ComponentModel> selectedComponents)
    {
        // your validation logic here
        return new ConfigurationResult { IsSuccess = true, Message = "OK" };
    }
}
```

Then register in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IConfigurationEngine, YourEngine>();
```

A `MockEngine` is available in `ConfigurationLogic/MockEngine.cs` for testing
until the real engine is implemented.

---

## Integration Test — verified pipeline

The following flow was tested end-to-end and confirmed working:

```
✅ DbInitializer.Initialize()          — seeds DB from JSON
✅ AppService.GetCategoriesAsync()     — reads categories from DB
✅ AppService.GetComponentsAsync()     — reads components from DB
✅ AppService.SaveConfigurationAsync() — validates and saves configuration
(Featured)✅ AppService.ExportConfigurationAsync() — exports PDF via ExportLayer
```

---

## Performance Notes

- `ValidateConfigurationAsync` loads all selected components in a **single query** — no N+1.
- (Featured) `ExportMapper` loads all components and categories in **bulk** — no N+1.

---

## Project References

```xml
<ItemGroup>
    <ProjectReference Include="..\DataLayer\DataLayer.csproj" />
    (Featured) <ProjectReference Include="..\ExportLayer\ExportLayer.csproj" />
</ItemGroup>
```

---

## Known Limitations (v1.0)

- Specialist management not implemented — depends on how the doctor logs in (TBD).
- `ConfigurationItem.Quantity` is always 1 — multiple quantities not yet supported.
- `MockEngine` always returns all components as recommended and always validates as valid.
  Replace with real engine implementation when ready.

---

## Developed by Claude Sonnet 4.6 <3

## Consulted with Gemini 3.1 Pro <3

## Managed by Peta 8-)
