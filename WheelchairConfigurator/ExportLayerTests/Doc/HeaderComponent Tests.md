# HeaderComponent Tests

This document provides a quick overview of the unit tests written for the `HeaderComponent` in the PDF export module. Our goal is to ensure the document header is rendered correctly, including structural layout, text formatting, and dynamic logo injection.

## Test Coverage Summary

The `HeaderComponentTest` class verifies the following behaviors:

### 1. Layout Structure

* **`Render_AddsTableWithTwoColumns`** & **`Render_TableHasExactlyOneRow`**
  Ensures the header layout is built using a strict 1-row, 2-column MigraDoc table (left cell for text, right cell for the logo).
* **`Render_TableBordersAreNotVisible`**
  Verifies that structural table borders are correctly hidden from the final PDF output.

### 2. Left Cell (Text Content)

* **`Render_LeftCell_ContainsTitleText`** & **`Render_LeftCell_ContainsSubtitleText`**
  Checks that the main title ("Wheelchair Configuration") and subtitle ("Vygenerováno systémem") are correctly placed in the left column.
* **`Render_TitleParagraph_HasBoldFont`** & **`Render_TitleParagraph_HasExpectedFontSize`**
  Ensures the main title formatting is strictly maintained (Bold, 18-point font size).

### 3. Right Cell (Logo Handling)

* **`Render_WithNullLogo_RightCellHasNoParagraphWithImage`** & **`Render_WithEmptyLogoArray_RightCellHasNoParagraphWithImage`**
  Confirms the component handles missing or invalid logos gracefully without throwing errors or adding empty image elements.
* **`Render_WithValidLogo_RightCellContainsImage`**
  Ensures that when a valid byte array is provided, an image element is successfully added to the right column.
* **`Render_WithValidLogo_ImageSourceContainsBase64Prefix`**
  Verifies that the image is properly converted into a base64 string format compatible with MigraDoc (`base64:...`).
* **`Render_WithValidLogo_ImageHasLockedAspectRatio`** & **`Render_WithValidLogo_ImageWidthIs2Point5Cm`**
  Validates that the injected logo retains its aspect ratio and is strictly scaled to a width of exactly 2.5 centimeters.

---

**Note for developers:** Logo tests use a minimal valid 1x1 pixel transparent PNG byte array to ensure test execution remains lightning-fast and doesn't rely on external files. Text tests utilize the shared `MigraDocExtensions.GetRawText()` method.
