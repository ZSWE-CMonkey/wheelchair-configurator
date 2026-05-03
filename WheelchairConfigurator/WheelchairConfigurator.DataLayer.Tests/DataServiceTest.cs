using Moq;
using WheelchairConfigurator.Data;
using WheelchairConfigurator.Data.Providers;
using WheelchairConfigurator.Service;
using Xunit;

namespace WheelchairConfigurator.DataLayer.Tests;

/// <summary>
/// Unit tests for DataService.ProcessData() pipeline.
/// ILocalFileProvider is mocked via Moq — controls which paths are returned.
/// JsonDataLoader uses real temp files to avoid mocking file I/O twice.
/// All temp files are cleaned up after each test via IDisposable.
/// </summary>
public class DataServiceTest : IDisposable
{
    private readonly List<string> _tempFiles = [];

    public void Dispose()
    {
        foreach (var file in _tempFiles.Where(File.Exists))
            File.Delete(file);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private string WriteValidJson(int categoryCount = 1)
    {
        var categories = string.Join(",", Enumerable.Range(1, categoryCount)
            .Select(i => $"{{\"Name\": \"Category{i}\", \"RoleKey\": \"cat{i}\"}}"));

        var path = TempPath();
        File.WriteAllText(path, $$"""
            {
                "Categories": [{{categories}}],
                "Components": [],
                "Specs": [],
                "Models3D": [],
                "Rules": []
            }
            """);
        return path;
    }

    private string WriteInvalidJson()
    {
        var path = TempPath();
        File.WriteAllText(path, "{ this is not valid json !!!");
        return path;
    }

    private string TempPath()
    {
        var path = Path.Combine(Path.GetTempPath(), $"DataServiceTest_{Guid.NewGuid()}.json");
        _tempFiles.Add(path);
        return path;
    }

    private static DataService CreateSut(params string[] paths)
    {
        var mockProvider = new Mock<ILocalFileProvider>();
        mockProvider.Setup(p => p.GetSeedFilePaths()).Returns(paths);
        return new DataService(mockProvider.Object, new JsonDataLoader());
    }

    // -------------------------------------------------------------------------
    // No seed files
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessData_NoPaths_ReturnsEmptyList()
    {
        var sut = CreateSut();

        var result = sut.ProcessData();

        Assert.Empty(result);
    }

    // -------------------------------------------------------------------------
    // Single valid file
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessData_SingleValidFile_ReturnsOneDto()
    {
        var path = WriteValidJson();
        var sut = CreateSut(path);

        var result = sut.ProcessData();

        Assert.Single(result);
    }

    [Fact]
    public void ProcessData_SingleValidFile_DtoContainsCorrectCategoryCount()
    {
        var path = WriteValidJson(categoryCount: 3);
        var sut = CreateSut(path);

        var result = sut.ProcessData();

        Assert.Equal(3, result[0].Categories.Count);
    }

    // -------------------------------------------------------------------------
    // Multiple valid files
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessData_MultipleValidFiles_ReturnsOneDtoPerFile()
    {
        var path1 = WriteValidJson(categoryCount: 1);
        var path2 = WriteValidJson(categoryCount: 2);
        var sut = CreateSut(path1, path2);

        var result = sut.ProcessData();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ProcessData_MultipleValidFiles_EachDtoHasCorrectData()
    {
        var path1 = WriteValidJson(categoryCount: 1);
        var path2 = WriteValidJson(categoryCount: 3);
        var sut = CreateSut(path1, path2);

        var result = sut.ProcessData();

        Assert.Single(result[0].Categories);
        Assert.Equal(3, result[1].Categories.Count);
    }

    // -------------------------------------------------------------------------
    // Invalid / missing files — skipped, no exception
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessData_SingleInvalidFile_ReturnsEmptyList()
    {
        var path = WriteInvalidJson();
        var sut = CreateSut(path);

        var result = sut.ProcessData();

        Assert.Empty(result);
    }

    [Fact]
    public void ProcessData_NonExistentFile_ReturnsEmptyList()
    {
        var sut = CreateSut("/nonexistent/path/seed.json");

        var result = sut.ProcessData();

        Assert.Empty(result);
    }

    [Fact]
    public void ProcessData_InvalidFile_DoesNotThrow()
    {
        var path = WriteInvalidJson();
        var sut = CreateSut(path);

        var ex = Record.Exception(() => sut.ProcessData());

        Assert.Null(ex);
    }

    // -------------------------------------------------------------------------
    // Mixed valid and invalid files
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessData_MixedValidAndInvalid_ReturnsOnlySuccessful()
    {
        var validPath = WriteValidJson(categoryCount: 2);
        var invalidPath = WriteInvalidJson();
        var sut = CreateSut(validPath, invalidPath);

        var result = sut.ProcessData();

        Assert.Single(result);
        Assert.Equal(2, result[0].Categories.Count);
    }

    [Fact]
    public void ProcessData_AllInvalid_ReturnsEmptyList()
    {
        var invalid1 = WriteInvalidJson();
        var invalid2 = WriteInvalidJson();
        var sut = CreateSut(invalid1, invalid2);

        var result = sut.ProcessData();

        Assert.Empty(result);
    }

    [Fact]
    public void ProcessData_ValidThenNonExistent_ReturnsOnlyValid()
    {
        var validPath = WriteValidJson(categoryCount: 1);
        var sut = CreateSut(validPath, "/nonexistent/file.json");

        var result = sut.ProcessData();

        Assert.Single(result);
    }

    // -------------------------------------------------------------------------
    // Provider is called exactly once
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessData_CallsProviderExactlyOnce()
    {
        var mockProvider = new Mock<ILocalFileProvider>();
        mockProvider.Setup(p => p.GetSeedFilePaths()).Returns([]);
        var sut = new DataService(mockProvider.Object, new JsonDataLoader());

        sut.ProcessData();

        mockProvider.Verify(p => p.GetSeedFilePaths(), Times.Once);
    }
}
