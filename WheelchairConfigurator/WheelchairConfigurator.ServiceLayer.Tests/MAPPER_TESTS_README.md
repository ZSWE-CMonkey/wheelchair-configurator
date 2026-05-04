# Mapper Tests — Service Layer

Mappers are pure static methods with no external dependencies — no mocking required.
Each test is isolated, instantaneous, and deterministic.

---

## CategoryMapperTest

Validates `CategoryMapper.Map(Category entity) → CategoryModel`.

### 1. Field Mapping
* **`Map_MapsIdCorrectly`** / **`Map_MapsNameCorrectly`** — confirms `Id` and `Name` are transferred from entity to model without mutation.
* **`Map_ReturnsCategoryModel`** — verifies the return type is `CategoryModel`.

### 2. Edge Cases
* **`Map_EmptyName_MapsCorrectly`** — empty string name is preserved, not replaced with null or default.
* **`Map_ZeroId_MapsCorrectly`** — zero ID is a valid value and must not be filtered out.

### 3. Parametrized & Isolation
* **`Map_VariousEntities_MapsIdAndNameCorrectly`** — `[Theory]` covering several id/name combinations.
* **`Map_TwoDistinctEntities_ReturnTwoDistinctModels`** — two separate inputs produce two separate outputs with no shared state.

---

## ComponentMapperTest

Validates `ComponentMapper.Map(Component entity) → ComponentModel`.

### 1. Field Mapping
* **`Map_MapsIdCorrectly`** / **`Map_MapsNameCorrectly`** / **`Map_MapsPriceCorrectly`** / **`Map_MapsCatalogUrlCorrectly`** — each field is verified independently.
* **`Map_NullCatalogUrl_MapsAsNull`** — nullable `CatalogUrl` must survive the mapping as null, not as an empty string.
* **`Map_ReturnsComponentModel`** — return type check.

### 2. Engine Flag Isolation
* **`Map_IsRecommended_DefaultsFalse`** / **`Map_IsIncompatible_DefaultsFalse`** — critical contract: the mapper must **not** pre-set engine flags. Only `AppService` may set these after calling `IConfigurationEngine`. Pre-setting them here would silently break the recommendation flow.

### 3. Edge Cases & Parametrized
* **`Map_ZeroPrice_MapsCorrectly`** — zero is a valid price, must not be treated as missing.
* **`Map_VariousEntities_MapsAllFieldsCorrectly`** — `[Theory]` covering several id/name/price combinations.

---

## ConfigurationMapperTest

Validates both directions of `ConfigurationMapper`:
- `Map(Configuration entity) → ConfigurationModel`
- `Map(ConfigurationRequest request) → Configuration`

### 1. Entity → Model
* **`Map_Entity_MapsIdCorrectly`** / **`Map_Entity_MapsSpecialistIdCorrectly`** / **`Map_Entity_MapsCreatedAtCorrectly`** — all three fields verified independently.
* **`Map_Entity_ReturnsConfigurationModel`** — return type check.
* **`Map_Entity_VariousIds_MapsCorrectly`** — `[Theory]` with edge values including `int.MaxValue`.

### 2. Request → Entity
* **`Map_Request_MapsSpecialistIdCorrectly`** — `SpecialistId` is carried over from the UI request.
* **`Map_Request_CreatedAt_IsSetToCurrentTime`** — `CreatedAt` must be set by the mapper to `DateTime.Now`, not left at default. Verified with an `InRange` check to avoid flakiness.
* **`Map_Request_IdIsNotSetByMapper`** — `Id` must remain `0`. The database assigns the real ID on insert; pre-setting it here would cause a silent primary key conflict.
* **`Map_Request_ReturnsConfigurationEntity`** — return type check.
* **`Map_Request_VariousSpecialistIds_MapsCorrectly`** — `[Theory]` parametrized.

---

## ExportMapperTest

Validates `ExportMapper.MapAsync(...)` which assembles the full `ConfigurationExportModel` from raw database entities.
Uses **Moq** to mock `IComponentRepository` and `ICategoryRepository`.

### 1. Header Fields
* **`MapAsync_ConfigurationName_IncludesConfigId`** — the configuration name must embed the database ID for traceability.
* **`MapAsync_SpecialistName_CombinesFirstAndLastName`** — first and last name are joined with a space.
* **`MapAsync_WhenSpecialistIsNull_UsesUnknownSpecialistFallback`** — null specialist must not throw; falls back to `"Unknown Specialist"`.
* **`MapAsync_CreatedAt_MatchesConfigCreatedAt`** — timestamp is taken from the configuration entity, not from `DateTime.Now`.

### 2. Item Collection
* **`MapAsync_Items_CountMatchesConfigurationItemCount`** — one export item is produced per `ConfigurationItem`.
* **`MapAsync_EmptyItems_ReturnsEmptyItemsList`** — empty input produces an empty list, not null.
* **`MapAsync_ExportItem_ComponentNameIsMappedCorrectly`** / **`MapAsync_ExportItem_CategoryNameIsMappedCorrectly`** / **`MapAsync_ExportItem_PriceIsMappedCorrectly`** / **`MapAsync_ExportItem_QuantityIsMappedCorrectly`** — each field on the export item is verified independently.
* **`MapAsync_ExportItem_CatalogUrlIsUsedAsItemCode`** — `CatalogUrl` doubles as the printable item code.
* **`MapAsync_ExportItem_NullCatalogUrl_ItemCodeIsDash`** — null `CatalogUrl` must render as `"-"`, not as null or empty string, to keep the PDF table readable.

### 3. TotalPrice
* **`MapAsync_TotalPrice_IsSumOfPriceTimesQuantity`** — total is `Σ (Price × Quantity)` across all items, including multi-quantity lines.
* **`MapAsync_EmptyItems_TotalPriceIsZero`** — zero items produce zero total.

### 4. N+1 Prevention
* **`MapAsync_LoadsAllComponentsInSingleRepositoryCall`** / **`MapAsync_LoadsAllCategoriesInSingleRepositoryCall`** — verifies via `Verify(..., Times.Once)` that both repositories are called **exactly once** regardless of item count. Regression guard against accidental N+1 reintroduction.
