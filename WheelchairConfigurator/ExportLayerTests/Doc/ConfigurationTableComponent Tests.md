# ConfigurationTableComponent Tests

The `ConfigurationTableComponent` is the most complex UI element in the export, responsible for the dynamic list of wheelchair components and financial totals.

## Test Coverage Summary

### 1. Data-Driven Table Generation

* **Row Logic**: Tests verify that the table always includes a header row and matches the number of items in the configuration model (Empty, Single, or Multiple items).
* **Column Logic**: Ensures the table maintains exactly 5 columns (Category, Name, Code, Quantity, Price).
* **Header Content**: Verifies correct localized titles for all columns.

### 2. Visual Styling & Shading

* **Zebra Striping**: Specifically tests the shading logic where even data rows remain transparent and odd data rows use `PdfDocumentColors.GreyLight` for better readability.

### 3. Business Logic: Text Truncation

* **Category Truncation**: Uses `[Theory]` to validate that category names are capped at 25 characters followed by an ellipsis (`…`).
* **Component Name Truncation**: Validates that component names are capped at 40 characters to prevent table layout breakage on long descriptions.

### 4. Grand Total Rendering

* **Placement**: Ensures the total price is rendered as a standalone paragraph immediately after the table.
* **Formatting**: Checks that the total price reflects the decimal value from the model and is strictly right-aligned for professional document standards.

---

**Note for developers:** These tests are culture-aware regarding currency formatting. If the system locale changes, the price assertion (`Assert.Contains`) is designed to remain robust by checking for the numeric value.
