using WheelchairConfigurator.Data;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests;

/// <summary>
/// Unit tests for JsonDataLoader.
/// Tests use temporary files written to disk and cleaned up after each test.
/// No database or mocks required — JsonDataLoader only reads and deserializes JSON.
/// </summary>
public class JsonDataLoaderTest : IDisposable
{
    private readonly JsonDataLoader _loader = new();
    private readonly List<string> _tempFiles = [];

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes content to a temp file and registers it for cleanup.
    /// </summary>
    private string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"JsonDataLoaderTest_{Guid.NewGuid()}.json");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
            if (File.Exists(file))
                File.Delete(file);
    }

    // -------------------------------------------------------------------------
    // File not found
    // -------------------------------------------------------------------------

    [Fact]
    public void LoadData_FileDoesNotExist_ReturnsNull()
    {
        var result = _loader.LoadData("/nonexistent/path/seed_data.json");
        Assert.Null(result);
    }

    [Fact]
    public void LoadData_EmptyPath_ReturnsNull()
    {
        var result = _loader.LoadData(string.Empty);
        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // Valid JSON
    // -------------------------------------------------------------------------

    [Fact]
    public void LoadData_ValidJson_ReturnsNonNull()
    {
        var path = WriteTempFile("""
            {
                "Categories": [],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
    }

    [Fact]
    public void LoadData_ValidJson_CategoriesAreParsedCorrectly()
    {
        var path = WriteTempFile("""
            {
                "Categories": [
                    { "Name": "Wheels", "RoleKey": "wheels" },
                    { "Name": "Frames", "RoleKey": "frames" }
                ],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.Equal(2, result!.Categories.Count);
        Assert.Equal("Wheels", result.Categories[0].Name);
        Assert.Equal("Frames", result.Categories[1].Name);
    }

    [Fact]
    public void LoadData_ValidJson_ComponentsAreParsedCorrectly()
    {
        var path = WriteTempFile("""
            {
                "Categories": [],
                "Components": [
                    { "Name": "SportWheel X1", "CategoryName": "Wheels", "CatalogUrl": "https://example.com", "Price": 299.99 }
                ],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.Single(result!.Components);
        Assert.Equal("SportWheel X1", result.Components[0].Name);
        Assert.Equal("Wheels", result.Components[0].CategoryName);
        Assert.Equal(299.99m, result.Components[0].Price);
        Assert.Equal("https://example.com", result.Components[0].CatalogUrl);
    }

    [Fact]
    public void LoadData_ValidJson_CompatibilityRulesAreParsedCorrectly()
    {
        var path = WriteTempFile("""
            {
                "Categories": [],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": [
                    { "ComponentAName": "Wheel A", "ComponentBName": "Frame B", "IsCompatible": true },
                    { "ComponentAName": "Wheel A", "ComponentBName": "Frame C", "IsCompatible": false }
                ]
            }
            """);

        var result = _loader.LoadData(path);

        Assert.Equal(2, result!.Rules.Count);
        Assert.True(result.Rules[0].IsCompatible);
        Assert.False(result.Rules[1].IsCompatible);
    }

    [Fact]
    public void LoadData_ValidJson_EmptyLists_ReturnsEmptyCollections()
    {
        var path = WriteTempFile("""
            {
                "Categories": [],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
        Assert.Empty(result!.Categories);
        Assert.Empty(result.Components);
        Assert.Empty(result.Specs);
        Assert.Empty(result.Models3D);
        Assert.Empty(result.Rules);
    }

    // -------------------------------------------------------------------------
    // JSON options — case insensitivity, comments, trailing commas
    // -------------------------------------------------------------------------

    [Fact]
    public void LoadData_PropertyNamesAreCaseInsensitive()
    {
        // All lowercase property names — must still deserialize correctly
        var path = WriteTempFile("""
            {
                "categories": [
                    { "name": "Wheels", "rolekey": "wheels" }
                ],
                "components": [],
                "specs": [],
                "models3d": [],
                "rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
        Assert.Single(result!.Categories);
        Assert.Equal("Wheels", result.Categories[0].Name);
    }

    [Fact]
    public void LoadData_JsonWithComments_ParsedSuccessfully()
    {
        var path = WriteTempFile("""
            {
                // seed data
                "Categories": [
                    { "Name": "Wheels", "RoleKey": "wheels" } // main category
                ],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
        Assert.Single(result!.Categories);
    }

    [Fact]
    public void LoadData_JsonWithTrailingCommas_ParsedSuccessfully()
    {
        var path = WriteTempFile("""
            {
                "Categories": [
                    { "Name": "Wheels", "RoleKey": "wheels" },
                ],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": [],
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
        Assert.Single(result!.Categories);
    }

    // -------------------------------------------------------------------------
    // Invalid JSON
    // -------------------------------------------------------------------------

    [Fact]
    public void LoadData_MalformedJson_ReturnsNull()
    {
        var path = WriteTempFile("{ this is not valid json !!!");

        var result = _loader.LoadData(path);

        Assert.Null(result);
    }

    [Fact]
    public void LoadData_EmptyFile_ReturnsNull()
    {
        var path = WriteTempFile(string.Empty);

        var result = _loader.LoadData(path);

        Assert.Null(result);
    }

    [Fact]
    public void LoadData_JsonArray_ReturnsNull()
    {
        // Root is array, not object — cannot deserialize to SeedDataDto
        var path = WriteTempFile("[ { \"Name\": \"Wheels\" } ]");

        var result = _loader.LoadData(path);

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // Missing optional fields
    // -------------------------------------------------------------------------

    [Fact]
    public void LoadData_MissingCollections_DefaultsToEmptyLists()
    {
        // JSON contains only Categories — other collections are missing entirely
        var path = WriteTempFile("""
            {
                "Categories": [
                    { "Name": "Wheels", "RoleKey": "wheels" }
                ]
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
        Assert.Single(result!.Categories);
        Assert.Empty(result.Components);
        Assert.Empty(result.Specs);
        Assert.Empty(result.Models3D);
        Assert.Empty(result.Rules);
    }

    [Fact]
    public void LoadData_ComponentMissingOptionalCatalogUrl_ParsedAsNull()
    {
        var path = WriteTempFile("""
            {
                "Categories": [],
                "Components": [
                    { "Name": "BasicFrame", "CategoryName": "Frames", "Price": 500.0 }
                ],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);

        var result = _loader.LoadData(path);

        Assert.NotNull(result);
        Assert.Single(result!.Components);
        Assert.Null(result.Components[0].CatalogUrl);
    }
}
