# PdfBuilder Tests

The `PdfBuilder` acts as the master orchestrator of the export process. It configures the global document properties and assembles the structure before handing it off to the MigraDoc rendering engine. 

To ensure fast, deterministic unit testing without triggering OS-level font dependencies, these tests validate the internal document construction logic directly.

## Test Coverage Summary

### 1. Document Initialization
* **`CreateDocument_SetsCorrectFontAndTitle`**
  Validates the base structure of the MigraDoc `Document`. 
  * **Metadata:** Ensures the PDF metadata Title is correctly set to "Wheelchair Configuration".
  * **Global Styling:** Confirms that the global "Normal" style is strictly bound to our branded **Roboto** font at a size of **12pt**.

### 2. Page Setup & Dimensions
* **`ConfigureSection_SetsA4AndCorrectMargins`**
  Ensures the output is strictly formatted for physical printing.
  * **Page Size:** Locked strictly to **A4** format to prevent layout shifts.
  * **Margins:** Validates the precise, asymmetrical margin configuration required for professional printing: Left/Right at **2 cm**, Top at **3 cm**, and Bottom at **1 cm**.
  * **Header/Footer Distances:** Confirms exactly **0.5 cm** distance for header and footer placement to avoid overlapping with main content.

---
**Note for developers:** Due to MigraDoc's strict font rendering requirements in .NET Core environments (which throw exceptions if physical fonts are missing during the render phase), these tests deliberately bypass the final `Build()` byte-conversion. Instead, they use reflection to peek into the `private` configuration methods, verifying the structural integrity of the document model instantly and safely.