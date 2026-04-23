# Export Layer — Wheelchair Configurator

**Version 1.2** | Author: Peta8 | C# / MAUI / PDFsharp + MigraDoc

---

## Overview

The export layer is responsible for generating PDF documents from wheelchair configurations.
It is fully decoupled from the database and UI — it receives a populated model and returns PDF as `byte[]`.

Fonts and logos are injected dynamically by the host application

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
        PdfFontResolver.cs          — custom font resolver mapping external byte arrays

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

ExportLayer is called exclusively through AppService in ServiceLayer. The PDF library is completely decoupled from the file system and relies on the host application (MAUI) to provide assets via byte arrays.

### 1. Register fonts and load logo at startup

In your host application (e.g., `MauiProgram.cs` or an initialization service), load your `.ttf` and `.jpg` files from MAUI's `Resources/Raw` folder and register them:



```csharp
using PdfSharp.Fonts;
using WheelchairConfigurator.Export.Pdf;

// A. Load and register the custom font
using (var fontStream = await FileSystem.OpenAppPackageFileAsync("Roboto-Regular.ttf"))
{
    using var ms = new MemoryStream();
    await fontStream.CopyToAsync(ms);

    // Register font bytes into our custom resolver
    PdfFontResolver.Instance.RegisterFont("Roboto", ms.ToArray());
    GlobalFontSettings.FontResolver = PdfFontResolver.Instance;
}

// B. Load the logo
byte[] logoData;
using (var logoStream = await FileSystem.OpenAppPackageFileAsync("vibrant-cheerful-blue-dolphin-leaping.jpg"))
{
    using var ms = new MemoryStream();
    await logoStream.CopyToAsync(ms);
    logoData = ms.ToArray();
}

// C. Register PdfBuilder in Dependency Injection
builder.Services.AddSingleton<IExportFileBuilder>(sp => new PdfBuilder(logoData));
```

### 2. Call export from UI via AppService

The ServiceLayer remains clean and unaware of how the PDF is generated.

C#

```csharp
byte[] pdfData = await _appService.ExportConfigurationAsync(configurationId);

// Save and share on Android/iOS via MAUI Share API:
string filePath = Path.Combine(FileSystem.CacheDirectory, "configuration.pdf");
await File.WriteAllBytesAsync(filePath, pdfData);

await Share.RequestAsync(new ShareFileRequest
{
    Title = "Wheelchair Configuration",
    File  = new ShareFile(filePath)
});
```

### External Assets Setup (No Embedded Resources)

The export library does **not** contain any embedded fonts or images. This ensures cross-platform compatibility and keeps the Class Library lightweight.

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
- Bold text requires a bold font variant registered dynamically via `PdfFontResolver` — currently only Roboto Regular is included

---

## Developed by Claude Sonnet 4.6 <3

## Consulted with Gemini 3.1 Pro <3

## Managed by Peta 8-)
