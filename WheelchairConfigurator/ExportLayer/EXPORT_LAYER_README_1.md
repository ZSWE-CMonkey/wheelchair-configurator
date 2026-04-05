# Export Layer — Wheelchair Configurator

**Version 1.1** | Author: Peta8 | C# / MAUI / QuestPDF

---

## Overview

The export layer is responsible for generating PDF documents from wheelchair configurations.
It is fully decoupled from the database and UI — it receives a fully populated model and produces a file.

**ExportService and IExportService have been moved to ServiceLayer.**
ExportLayer contains only the file builder and PDF components — no orchestration logic.

---

## Project Structure

```
ExportLayer/
    IExportFileBuilder.cs       — contract for the file builder

    ExportModel/
        ConfigurationExportModel.cs  — model passed from ServiceLayer to PdfBuilder
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
AppService.ExportConfigurationAsync()    ← called from ServiceLayer
    │
    ├── ExportMapper.MapAsync()          ← assembles ConfigurationExportModel
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

ExportLayer is called exclusively through `AppService` in ServiceLayer.
UI never calls ExportLayer directly.

### 1. Register QuestPDF license — once at startup

In `MauiProgram.cs`:

```csharp
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
```

### 2. Register PdfBuilder in DI

```csharp
builder.Services.AddSingleton<IExportFileBuilder, PdfBuilder>();
```

### 3. Call export from UI via AppService

```csharp
string pdfPath = await _appService.ExportConfigurationAsync(configurationId);
```

AppService handles all data loading and model assembly internally — UI only provides the configuration ID.

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

## Known Limitations (v1.1)

- PDF save path uses `Directory.GetCurrentDirectory()` — for MAUI on Android replace with `FileSystem.CacheDirectory`.

---

## Developed by Claude Sonnet 4.6 <3

## Consulted with Gemini 3.1 Pro <3

## Managed by Peta 8-)
