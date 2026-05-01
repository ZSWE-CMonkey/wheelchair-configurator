# PdfFontResolver Tests

Since PDFSharp/MigraDoc on .NET 8 requires a custom font resolver to handle private font files (TTF/OTF), the `PdfFontResolver` is a critical infrastructure component.

## Test Coverage Summary

### 1. Singleton Integrity

* **`Instance_IsSingleton`**
  Ensures that only one instance of the resolver exists in memory to prevent duplicate font registration and memory leaks.

### 2. Font Registration Logic

* **Storage & Retrieval**: Confirms that registered byte arrays (font files) are stored correctly and can be retrieved by name.
* **Case Insensitivity**: Verifies that font names are treated as case-insensitive (e.g., "Roboto" vs "roboto"), which is crucial for cross-platform compatibility.
* **Overwrite Safety**: Ensures that registering a font with an existing name correctly updates the internal registry.

### 3. Error Handling

* **Missing Fonts**: Validates that trying to access a font that hasn't been registered throws a clear `InvalidOperationException` with a helpful message explaining how to fix it (hinting at `RegisterFont`).

### 4. Typeface Resolution (MigraDoc Integration)

* **Fallback Strategy**: Verifies that our resolver uses a "strict branding" strategy—no matter what font name is requested in the document code (Arial, Helvetica, etc.), it always resolves to our branded **Roboto** font.
* **Robustness**: Uses `[Theory]` to ensure that various font family requests never return a null result, which would cause the PDF rendering to crash.
