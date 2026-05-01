# SignatureComponent Tests

This document provides a quick overview of the unit tests written for the `SignatureComponent` in the PDF export module. Our goal is to ensure the signature block is rendered correctly, with the proper layout, placeholders, and formatting.

## Test Coverage Summary

The `SignatureComponentTest` class verifies the following behaviors, structured by feature areas:

### 1. Title Configuration (Constructors)

* **`Ctor_DefaultTitle_IsPodpis`**
  Ensures that if no title is provided, the component falls back to the default label "Podpis".
* **`Ctor_CustomTitle_IsRenderedInLabelRow`**
  Verifies that a custom title (e.g., "Podpis odpovědné osoby") passed via the constructor is correctly rendered in the component.

### 2. Layout Structure

* **`Render_AddsSpacerParagraphBeforeTable`** & **`Render_SpacerParagraph_HasPositiveSpaceBefore`**
  Checks that the component begins with a dedicated spacer paragraph with a positive top margin to separate the signature block from preceding content.
* **`Render_AddsTableAfterSpacer`**
  Confirms that the core of the signature block is built using a MigraDoc Table, placed immediately after the spacer.
* **`Render_Table_HasTwoColumns`** & **`Render_Table_HasTwoRows`**
  Verifies the table grid matches the exact 2x2 specification required for the layout.
* **`Render_TableBordersAreNotVisible`**
  Ensures the structural table borders are strictly hidden from the final PDF output.

### 3. Content: Row 1 (Placeholders & Signature Line)

* **`Render_FirstRow_LeftCell_ContainsDotPlaceholders`**
  Checks the left column for the correct date and location placeholders (e.g., "V ..... dne .....").
* **`Render_FirstRow_RightCell_ContainsSignatureDots`** & **`Render_FirstRow_RightCell_IsCenterAligned`**
  Verifies that the right column contains the dotted line for the actual physical signature ("........") and is center-aligned.

### 4. Content: Row 2 (Labels & Styling)

* **`Render_SecondRow_LabelCell_IsCenterAligned`**
  Ensures the text label directly under the signature line is center-aligned.
* **`Render_SecondRow_LabelCell_FontSizeIsNine`**
  Checks that the label's formatting enforces a strict 9-point font size.
* **`Render_SecondRow_LabelCell_FontColorMatchesGreyDark`**
  Confirms the label text color specifically matches the predefined standard (`PdfDocumentColors.GreyDark`).
* **`Render_SecondRow_HasPositiveTopPadding`**
  Validates that there is a slight visual gap (top padding) between the dotted signature line and the label text beneath it.

---

**Note for developers:** Tests rely on a shared `MigraDocExtensions.GetRawText()` extension method that correctly parses both standard `Text` and `FormattedText` elements from MigraDoc paragraphs.
