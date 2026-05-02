# AppService Tests

`AppService` is the main orchestrator of the application — it connects the UI, repositories, engine and export layer.
It contains no business logic of its own; all tests verify that it correctly delegates, sequences and transforms data between layers.

All dependencies are mocked via **Moq** using specific repository interfaces (`ICategoryRepository`, `IComponentRepository`, etc.).

---

## Test Coverage Summary

### 1. GetCategoriesAsync

* **`GetCategoriesAsync_ReturnsAllMappedCategories`** — all categories returned by the repository are mapped and returned to the caller.
* **`GetCategoriesAsync_MapsIdAndNameCorrectly`** — `Id` and `Name` survive the mapping intact.
* **`GetCategoriesAsync_EmptyRepo_ReturnsEmptyList`** — empty repository returns an empty list without throwing.

### 2. GetComponentsAsync — Without Patient Profile

* **`GetComponentsAsync_WithoutPatient_ReturnsMappedComponents`** — components are loaded and mapped when no patient is provided.
* **`GetComponentsAsync_WithoutPatient_DoesNotCallEngine`** — the engine is **never** called when patient is null. Verified with `Times.Never` to prevent unnecessary processing.
* **`GetComponentsAsync_WithoutPatient_IsRecommendedIsFalse`** — engine flags stay at their default `false` values when the engine is not involved.

### 3. GetComponentsAsync — With Patient Profile

* **`GetComponentsAsync_WithPatient_CallsEngine`** — engine is called exactly once when a patient profile is present.
* **`GetComponentsAsync_WithPatient_RecommendedComponent_IsRecommendedIsTrue`** — components returned by the engine have `IsRecommended = true`.
* **`GetComponentsAsync_WithPatient_NonRecommendedComponent_IsIncompatibleIsTrue`** — components absent from the engine result have `IsIncompatible = true`.
* **`GetComponentsAsync_WithPatient_RecommendedComponent_IsIncompatibleIsFalse`** — recommended components are not simultaneously marked incompatible.

### 4. ValidateConfigurationAsync

* **`ValidateConfigurationAsync_DelegatesToEngine`** — the result is taken directly from the engine; engine is called exactly once.
* **`ValidateConfigurationAsync_PassesMappedComponentsToEngine`** — the engine receives the correctly mapped component list, verified by inspecting the argument via `It.Is<>`.

### 5. SaveConfigurationAsync

* **`SaveConfigurationAsync_WhenValidationFails_ReturnsFailureWithoutSaving`** — a failed validation result stops the pipeline immediately. `InsertAsync` on the configuration repository is **never** called. Prevents partial writes.
* **`SaveConfigurationAsync_WhenValidationSucceeds_InsertsConfiguration`** — on success, exactly one configuration record is inserted.
* **`SaveConfigurationAsync_WhenValidationSucceeds_InsertsOneItemPerComponent`** — one `ConfigurationItem` is inserted per selected component, verified with `Times.Exactly(n)`.
* **`SaveConfigurationAsync_WhenValidationSucceeds_ReturnsSuccessResult`** — `IsSuccess = true` and the success message are present in the result.
* **`SaveConfigurationAsync_InsertedItems_HaveQuantityOfOne`** — each inserted item carries `Quantity = 1`, verified via argument matching.

### 6. ExportConfigurationAsync

* **`ExportConfigurationAsync_CallsFileBuilderAndReturnsPdfBytes`** — the file builder is called once and its byte array is returned unchanged to the caller.
* **`ExportConfigurationAsync_PassesCorrectConfigurationIdToItemRepo`** — the correct configuration ID is passed when loading items, preventing cross-configuration data leaks.

### 7. GetConfigurationsBySpecialistAsync

* **`GetConfigurationsBySpecialistAsync_ReturnsMappedConfigurations`** — all configurations for the specialist are returned and mapped.
* **`GetConfigurationsBySpecialistAsync_EmptyRepo_ReturnsEmptyList`** — empty result is handled gracefully.
* **`GetConfigurationsBySpecialistAsync_MapsSpecialistIdCorrectly`** — `SpecialistId` is preserved through the mapping.
