# FooterComponent Tests

This document provides a quick overview of the unit tests written for the `FooterComponent` in the PDF export module. Our goal is to ensure the footer is rendered correctly, with the right formatting, content, and layout.

## Test Coverage Summary

The `FooterComponentTest` class verifies the following behaviors:

* **`Render_AddsExactlyOneParagraph`**
  Ensures that calling the render method adds exactly one paragraph to the document's footer section, preventing duplicate outputs or empty footers.

* **`Render_Paragraph_IsCenterAligned`**
  Verifies that the text inside the footer is center-aligned.

* **`Render_Paragraph_HasFontSizeNine`**
  Checks the formatting to guarantee the footer text size is strictly set to 9 points.

* **`Render_Paragraph_ContainsGeneratedOnText`**
  Reads the text content and confirms the presence of the "Generated on" timestamp label.

* **`Render_Paragraph_ContainsPageFieldAndNumPagesField`**
  Ensures that the footer utilizes dynamic MigraDoc fields (`PageField` and `NumPagesField`) to correctly display page numbers (e.g., "Page 1 of 5").

* **`Render_Paragraph_HasPositiveSpaceBefore`**
  Validates the visual layout by checking that there is a positive top margin (SpaceBefore) so the footer isn't glued to the page content above it.

* **`Render_Paragraph_FontColorMatchesGreyMedium`**
  Checks that the footer text color strictly matches our predefined standard (`PdfDocumentColors.GreyMedium`).

---

**Note for developers:** Tests are written using the Arrange-Act-Assert pattern. A helper method `CreateFooter()` is used to isolate each test by providing a fresh, empty MigraDoc document context.
