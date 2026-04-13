# Export Layer — Wheelchair Configurator
**Version 1.2** | Author: Peta8 | C# / MAUI / PDFsharp + MigraDoc

---

## Overview

The export layer is responsible for generating PDF documents from wheelchair configurations.
It is fully decoupled from the database and UI — it receives a populated model and returns PDF as `byte[]`.

Font and logo are embedded as assembly resources — no file path issues on any platform including Android.

---

## Project Structure

```
ExportLayer/
    IExportFileBuilder.cs       — contract for the file builder (returns byte[])

    ExportModel/
        ConfigurationExportModel.cs  — model passed from ServiceLayer to PdfBuilder
        ExportFormat.cs              — enum of supported formats

    Pdf/
        PdfBuilder.cs               — orchestrates PDF document assembly
        PdfDocumentColors.cs        — shared color palette
        PdfFontResolver.cs          — embedded font resolver for cross-platform support

        Components/
            HeaderComponent.cs      — page header (title + logo)
            MetadataComponent.cs    — configuration metadata (name, specialist, date)
            ConfigurationTableComponent.cs  — table of selected components with zebra striping
            SignatureComponent.cs   — signature line at the bottom
            FooterComponent.cs      — page number + generation date

        Interfaces/
            IPdfComponent.cs        — contract for section components
            IPdfHeaderComponent.cs  — contract for header components
            IPdfFooterComponent.cs  — contract for footer components

    Assets/
        vibrant-cheerful-blue-dolphin-leaping.jpg  — logo (embedded resource)

    Fonts/
        Roboto-Regular.ttf          — custom font (embedded resource)
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
                            └── byte[] (PDF data)
```

---

## How to Use

ExportLayer is called exclusively through `AppService` in ServiceLayer.
UI never calls ExportLayer directly.

### 1. Register font resolver — once at startup

In `MauiProgram.cs` or `Program.cs`:

```csharp
using PdfSharp.Fonts;
using WheelchairConfigurator.Export.Pdf;

GlobalFontSettings.FontResolver = PdfFontResolver.Instance;
```

### 2. Load logo and register PdfBuilder in DI

```csharp
// Load embedded logo
byte[]? logoData = PdfFontResolver.GetResourceBytes("vibrant-cheerful-blue-dolphin-leaping.jpg");

// Register in DI
builder.Services.AddSingleton<IExportFileBuilder>(sp => new PdfBuilder(logoData));
```

### 3. Call export from UI via AppService

```csharp
byte[] pdfData = await _appService.ExportConfigurationAsync(configurationId);

// Save and share on Android via MAUI Share API:
string filePath = Path.Combine(FileSystem.CacheDirectory, "configuration.pdf");
await File.WriteAllBytesAsync(filePath, pdfData);
await Share.RequestAsync(new ShareFileRequest
{
    Title = "Wheelchair Configuration",
    File  = new ShareFile(filePath)
});
```

---

## Embedded Resources

Font and logo are embedded into the assembly — no file path issues on Android or iOS.

`.csproj` setup:
```xml
<ItemGroup>
    <EmbeddedResource Include="Assets\vibrant-cheerful-blue-dolphin-leaping.jpg" />
    <EmbeddedResource Include="Fonts\Roboto-Regular.ttf" />
</ItemGroup>
```

Access via `PdfFontResolver.GetResourceBytes(fileName)`.

---

## PDF Structure

```
┌─────────────────────────────────────────┐
│  Wheelchair Configuration    [logo]      │  ← HeaderComponent
│  Vygenerováno systémem                  │
├─────────────────────────────────────────┤
│  Konfigurace: Test Configuration #1     │  ← MetadataComponent
│  Specialista: Dr. House                 │
│  Vytvořeno:   03.04.2026               │
├──────────────┬──────────────┬───────────┤
│  Kategorie   │  Komponenta  │  Cena     │  ← ConfigurationTableComponent
│  Frame       │  Alum. Frame │  $500.00  │     (zebra striping)
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

## NuGet Dependencies

```xml
<PackageReference Include="PDFsharp-MigraDoc" Version="6.2.4" />
```

---

## Known Limitations (v1.2)

- PDF save path on Android must use `FileSystem.CacheDirectory` — see usage example above.
- MigraDoc supports JPEG images only — PNG/WEBP logos must be converted to JPG.
- Bold text requires a bold font variant embedded as resource — currently only Roboto Regular is included.

---

## Developed by Claude Sonnet 4.6 <3
## Consulted with Gemini 3.1 Pro <3
## Managed by Peta 8-)
