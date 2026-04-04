# Export Layer — Wheelchair Configurator

**Version 1.0** | Author: Peta8 | C# / MAUI / QuestPDF

---

## Overview

The export layer is responsible for generating PDF documents from wheelchair configurations.
It is fully decoupled from the database and UI — it receives a populated model and produces a file.

---

## Project Structure

```
Export/
    IExportService.cs           — contract for the export orchestrator
    ExportService.cs            — orchestrator, loads data and routes to builder
    IExportFileBuilder.cs       — contract for the file builder

    ExportModel/
        ConfigurationExportModel.cs  — internal model passed between service and builder
        ExportFormat.cs              — enum of supported formats

    Pdf/
        PdfBuilder.cs               — builds the PDF document
        Components/
            HeaderComponent.cs      — page header (title + logo)
            MetadataComponent.cs    — configuration metadata (name, specialist, date)
            ConfigurationTableComponent.cs  — table of selected components
            SignatureComponent.cs   — signature line at the bottom
            FooterComponent.cs      — page number + generation date

    Assets/
        logo.webp                   — logo used in the PDF header
```

---

## Dependency Flow

```
ExportAsync(configurationId, format)
    │
    ├── GetMockData()              ← TODO: replace with repository calls
    │       └── ConfigurationExportModel
    │
    └── IExportFileBuilder.Build(model)
            └── PdfBuilder
                    ├── HeaderComponent
                    ├── MetadataComponent
                    ├── ConfigurationTableComponent
                    ├── SignatureComponent
                    └── FooterComponent
```

---

## How to Use

### 1. Register QuestPDF license — once at startup

In `MauiProgram.cs` or `Program.cs`:

```csharp
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
```

### 2. Initialize ExportService

```csharp
var dbService = new DbService("konfigurator.db");
var exportService = new ExportService(new PdfBuilder(), dbService);
```

### 3. Call ExportAsync

```csharp
string pdfPath = await exportService.ExportAsync(configurationId, ExportFormat.Pdf);
Console.WriteLine($"PDF saved to: {pdfPath}");
```

---

## Connecting to the Database (TODO for engine team)

`ExportService` currently uses mock data. Once the engine is ready, replace `GetMockData()` with real repository calls:

```csharp
// Replace this:
private ConfigurationExportModel GetMockData(int id) { ... }

// With this:
private async Task<ConfigurationExportModel> LoadFromDbAsync(int configurationId)
{
    var config     = await _configurationRepo.GetByIdAsync(configurationId);
    var items      = await _configurationItemRepo.GetByConfigurationIdAsync(configurationId);
    var specialist = await _specialistRepo.GetByIdAsync(config.SpecialistId);

    var exportItems = new List<ConfigurationExportItem>();
    foreach (var item in items)
    {
        var component = await _componentRepo.GetByIdAsync(item.ComponentId);
        var category  = await _categoryRepo.GetByIdAsync(component.CategoryId);

        exportItems.Add(new ConfigurationExportItem
        {
            CategoryName  = category.Name,
            ComponentName = component.Name,
            ItemCode      = component.CatalogUrl ?? "-",
            Price         = component.Price,
            Quantity      = item.Quantity
        });
    }

    return new ConfigurationExportModel
    {
        ConfigurationName = $"Configuration #{config.Id}",
        SpecialistName    = $"{specialist.FirstName} {specialist.LastName}",
        CreatedAt         = config.CreatedAt,
        TotalPrice        = exportItems.Sum(i => i.Price * i.Quantity),
        Items             = exportItems
    };
}
```

Also inject the repositories via constructor:

```csharp
public ExportService(
    IExportFileBuilder fileBuilder,
    DbService dbService,
    ConfigurationRepository configurationRepo,
    ConfigurationItemRepository configurationItemRepo,
    ComponentRepository componentRepo,
    CategoryRepository categoryRepo,
    SpecialistRepository specialistRepo)
```

---

## PDF Structure

```
┌─────────────────────────────────────────┐
│  Wheelchair Configuration    [logo]      │  ← HeaderComponent
│  Premium Mobility Solutions              │
├─────────────────────────────────────────┤
│  Konfigurace: Test Configuration #1     │  ← MetadataComponent
│  Specialista: Dr. House                 │
│  Vytvořeno:   03.04.2026               │
├──────────────┬──────────────┬───────────┤
│  Kategorie   │  Komponenta  │  Cena     │  ← ConfigurationTableComponent
│  Frame       │  Alum. Frame │  $500.00  │
│  Wheels      │  Off-road    │  $800.50  │
│  Seat        │  Cushion     │  $200.00  │
├──────────────┴──────────────┴───────────┤
│                      Celková cena: $... │
├─────────────────────────────────────────┤
│  V .............. dne ..............    │  ← SignatureComponent
│               Podpis odpovědné osoby   │
├─────────────────────────────────────────┤
│  Generated on 03.04.2026 | Page 1 of 1 │  ← FooterComponent
└─────────────────────────────────────────┘
```

---

## Logo / Assets

Logo is loaded via relative path — works on any machine:

```csharp
.Image(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo.webp"))
```

Assets are copied to output automatically via `.csproj`:

```xml
<ItemGroup>
    <None Update="Assets\**\*.*">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

---

## NuGet Dependencies

```xml
<PackageReference Include="QuestPDF" Version="2024.3.4" />
```

---

## Known Limitations (v1.0)

- `ExportService` uses mock data — replace `GetMockData()` with real repository calls once engine is ready.
- PDF save path uses `Directory.GetCurrentDirectory()` — for MAUI on Android replace with `FileSystem.CacheDirectory`.

Developed by Claude Sonnet 4.6 <3
Consulted with Gemini 3.1 Pro <3
Managed by Peta 8-)
