# MetadataComponent Tests

This document summarizes the test suite for `MetadataComponent`, which is responsible for rendering the technical details of the configuration (Name, Specialist, and Creation Date).

## Test Coverage Summary

The `MetadataComponentTest` class verifies the following:

### 1. Structure

* **`Render_AddsExactlyThreeParagraphs`**
  Ensures the component renders exactly three lines (paragraphs) of information.

### 2. Data Integrity (Labels & Values)

* **Configuration Name**: Verifies that the first paragraph contains the "Konfigurace:" label and correctly displays the provided configuration name.
* **Specialist Name**: Confirms the second paragraph contains the "Specialista:" label and the correct name of the responsible person.
* **Creation Date**: Checks that the third paragraph contains the "Vytvořeno:" label and that the date is formatted correctly using the standard short date/time format (`"g"`).

### 3. Layout & Spacing

* **`Render_FirstParagraph_HasPositiveSpaceBefore`**
  Verifies there is a top margin before the metadata block starts.
* **`Render_ThirdParagraph_HasPositiveSpaceAfter`**
  Ensures a bottom margin exists after the last metadata line to prevent content overlap.
* **`Render_SecondParagraph_HasNoExtraSpaceBefore`**
  Checks that the lines within the metadata block are tightly packed (no extra internal spacing).

---

**Note for developers:** These tests use a local `CreateModel` helper to easily inject custom data (names, dates) for specific test scenarios.
