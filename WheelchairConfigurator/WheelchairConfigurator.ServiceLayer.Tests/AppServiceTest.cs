using Moq;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Export;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;
using WheelchairConfigurator.Data.Repositories;
using Xunit;

namespace WheelchairConfigurator.ServiceLayer.Tests;

/// <summary>
/// Unit tests for AppService using Moq and Interfaces.
/// </summary>
public class AppServiceTest
{
    // -------------------------------------------------------------------------
    // SUT factory — keeps individual tests clean
    // -------------------------------------------------------------------------

    private record Sut(
        AppService Service,
        Mock<ICategoryRepository> CategoryRepo,
        Mock<IComponentRepository> ComponentRepo,
        Mock<IConfigurationRepository> ConfigurationRepo,
        Mock<IConfigurationItemRepository> ConfigurationItemRepo,
        Mock<ISpecialistRepository> SpecialistRepo,
        Mock<IConfigurationEngine> Engine,
        Mock<IExportFileBuilder> FileBuilder);

    private static Sut Create()
    {
        var categoryRepo = new Mock<ICategoryRepository>();
        var componentRepo = new Mock<IComponentRepository>();
        var configurationRepo = new Mock<IConfigurationRepository>();
        var configurationItemRepo = new Mock<IConfigurationItemRepository>();
        var specialistRepo = new Mock<ISpecialistRepository>();
        var engine = new Mock<IConfigurationEngine>();
        var fileBuilder = new Mock<IExportFileBuilder>();

        var service = new AppService(
            categoryRepo.Object,
            componentRepo.Object,
            configurationRepo.Object,
            configurationItemRepo.Object,
            specialistRepo.Object,
            engine.Object,
            fileBuilder.Object);

        return new Sut(service, categoryRepo, componentRepo,
            configurationRepo, configurationItemRepo, specialistRepo, engine, fileBuilder);
    }

    // -------------------------------------------------------------------------
    // Shared domain builders
    // -------------------------------------------------------------------------

    private static Category MakeCategory(int id = 1, string name = "Frames") =>
        new() { Id = id, Name = name };

    private static Component MakeComponent(int id = 1, string name = "Frame", decimal price = 100m) =>
        new() { Id = id, Name = name, Price = price, CatalogUrl = null };

    private static Configuration MakeConfiguration(int id = 1, int specialistId = 5) =>
        new() { Id = id, SpecialistId = specialistId, CreatedAt = DateTime.Now };

    private static Specialist MakeSpecialist(int id = 5) =>
        new() { Id = id, FirstName = "Jana", LastName = "Nováková" };

    private static ConfigurationItem MakeItem(int configId, int componentId) =>
        new() { ConfigurationId = configId, ComponentId = componentId, Quantity = 1 };

    // =========================================================================
    // GetCategoriesAsync
    // =========================================================================

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllMappedCategories()
    {
        var sut = Create();
        sut.CategoryRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category>
            {
                MakeCategory(1, "Frames"),
                MakeCategory(2, "Wheels")
            });

        var result = await sut.Service.GetCategoriesAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCategoriesAsync_MapsIdAndNameCorrectly()
    {
        var sut = Create();
        sut.CategoryRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category> { MakeCategory(42, "Joysticks") });

        var result = await sut.Service.GetCategoriesAsync();

        Assert.Equal(42, result[0].Id);
        Assert.Equal("Joysticks", result[0].Name);
    }

    [Fact]
    public async Task GetCategoriesAsync_EmptyRepo_ReturnsEmptyList()
    {
        var sut = Create();
        sut.CategoryRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Category>());

        var result = await sut.Service.GetCategoriesAsync();

        Assert.Empty(result);
    }

    // =========================================================================
    // GetComponentsAsync — without patient profile
    // =========================================================================

    [Fact]
    public async Task GetComponentsAsync_WithoutPatient_ReturnsMappedComponents()
    {
        var sut = Create();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(1))
            .ReturnsAsync(new List<Component>
            {
                MakeComponent(1, "Frame A"),
                MakeComponent(2, "Frame B")
            });

        var result = await sut.Service.GetComponentsAsync(categoryId: 1, patient: null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetComponentsAsync_WithoutPatient_DoesNotCallEngine()
    {
        var sut = Create();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Component> { MakeComponent() });

        await sut.Service.GetComponentsAsync(categoryId: 1, patient: null);

        sut.Engine.Verify(e => e.GetRecommendedComponentIdsAsync(It.IsAny<PatientProfileModel>(), It.IsAny<List<ComponentModel>>()), Times.Never);
    }

    [Fact]
    public async Task GetComponentsAsync_WithoutPatient_IsRecommendedIsFalse()
    {
        var sut = Create();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Component> { MakeComponent(1) });

        var result = await sut.Service.GetComponentsAsync(1);

        Assert.False(result[0].IsRecommended);
    }

    // =========================================================================
    // GetComponentsAsync — with patient profile
    // =========================================================================

    [Fact]
    public async Task GetComponentsAsync_WithPatient_CallsEngine()
    {
        var sut = Create();
        var patient = new PatientProfileModel();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Component> { MakeComponent(1) });

        sut.Engine.Setup(e => e.GetRecommendedComponentIdsAsync(It.IsAny<PatientProfileModel>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new List<int> { 1 });

        await sut.Service.GetComponentsAsync(1, patient);

        sut.Engine.Verify(e => e.GetRecommendedComponentIdsAsync(patient, It.IsAny<List<ComponentModel>>()), Times.Once);
    }

    [Fact]
    public async Task GetComponentsAsync_WithPatient_RecommendedComponent_IsRecommendedIsTrue()
    {
        var sut = Create();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Component> { MakeComponent(id: 1) });

        sut.Engine.Setup(e => e.GetRecommendedComponentIdsAsync(It.IsAny<PatientProfileModel>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new List<int> { 1 });

        var result = await sut.Service.GetComponentsAsync(1, new PatientProfileModel());

        Assert.True(result[0].IsRecommended);
    }

    [Fact]
    public async Task GetComponentsAsync_WithPatient_NonRecommendedComponent_IsIncompatibleIsTrue()
    {
        var sut = Create();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Component> { MakeComponent(id: 99) });

        sut.Engine.Setup(e => e.GetRecommendedComponentIdsAsync(It.IsAny<PatientProfileModel>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new List<int>());

        var result = await sut.Service.GetComponentsAsync(1, new PatientProfileModel());

        Assert.True(result[0].IsIncompatible);
    }

    [Fact]
    public async Task GetComponentsAsync_WithPatient_RecommendedComponent_IsIncompatibleIsFalse()
    {
        var sut = Create();
        sut.ComponentRepo.Setup(r => r.GetByCategoryIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Component> { MakeComponent(id: 5) });

        sut.Engine.Setup(e => e.GetRecommendedComponentIdsAsync(It.IsAny<PatientProfileModel>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new List<int> { 5 });

        var result = await sut.Service.GetComponentsAsync(1, new PatientProfileModel());

        Assert.False(result[0].IsIncompatible);
    }

    // =========================================================================
    // ValidateConfigurationAsync
    // =========================================================================

    [Fact]
    public async Task ValidateConfigurationAsync_DelegatesToEngine()
    {
        var sut = Create();
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [1, 2] };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component> { MakeComponent(1), MakeComponent(2) });

        // Uprav název metody z ValidateAsync na to co máš reálně v IConfigurationEngine (zde předpokládám ValidateAsync dle tvého kódu)
        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new ConfigurationResult { IsSuccess = true });

        var result = await sut.Service.ValidateConfigurationAsync(request);

        Assert.True(result.IsSuccess);
        sut.Engine.Verify(e => e.ValidateAsync(request, It.IsAny<List<ComponentModel>>()), Times.Once);
    }

    [Fact]
    public async Task ValidateConfigurationAsync_PassesMappedComponentsToEngine()
    {
        var sut = Create();
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [7] };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component> { MakeComponent(7, "Joystick", 89m) });

        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new ConfigurationResult { IsSuccess = true });

        await sut.Service.ValidateConfigurationAsync(request);

        sut.Engine.Verify(e => e.ValidateAsync(
            It.IsAny<ConfigurationRequest>(),
            It.Is<List<ComponentModel>>(list => list.Count == 1 && list[0].Id == 7)), Times.Once);
    }

    // =========================================================================
    // SaveConfigurationAsync
    // =========================================================================

    [Fact]
    public async Task SaveConfigurationAsync_WhenValidationFails_ReturnsFailureWithoutSaving()
    {
        var sut = Create();
        var failResult = new ConfigurationResult { IsSuccess = false, Message = "Invalid" };
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [1] };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component> { MakeComponent(1) });

        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(failResult);

        var result = await sut.Service.SaveConfigurationAsync(request);

        Assert.False(result.IsSuccess);
        sut.ConfigurationRepo.Verify(r => r.InsertAsync(It.IsAny<Configuration>()), Times.Never);
    }

    [Fact]
    public async Task SaveConfigurationAsync_WhenValidationSucceeds_InsertsConfiguration()
    {
        var sut = Create();
        var request = new ConfigurationRequest { SpecialistId = 3, SelectedComponentIds = [1] };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component> { MakeComponent(1) });

        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new ConfigurationResult { IsSuccess = true });

        await sut.Service.SaveConfigurationAsync(request);

        sut.ConfigurationRepo.Verify(r => r.InsertAsync(It.IsAny<Configuration>()), Times.Once);
    }

    [Fact]
    public async Task SaveConfigurationAsync_WhenValidationSucceeds_InsertsOneItemPerComponent()
    {
        var sut = Create();
        var request = new ConfigurationRequest
        {
            SpecialistId = 1,
            SelectedComponentIds = [10, 20, 30]
        };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component>
            {
                MakeComponent(10), MakeComponent(20), MakeComponent(30)
            });

        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new ConfigurationResult { IsSuccess = true });

        await sut.Service.SaveConfigurationAsync(request);

        sut.ConfigurationItemRepo.Verify(r => r.InsertAsync(It.IsAny<ConfigurationItem>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SaveConfigurationAsync_WhenValidationSucceeds_ReturnsSuccessResult()
    {
        var sut = Create();
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [1] };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component> { MakeComponent(1) });

        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new ConfigurationResult { IsSuccess = true });

        var result = await sut.Service.SaveConfigurationAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("Configuration saved successfully.", result.Message);
    }

    [Fact]
    public async Task SaveConfigurationAsync_InsertedItems_HaveQuantityOfOne()
    {
        var sut = Create();
        var request = new ConfigurationRequest { SpecialistId = 1, SelectedComponentIds = [5] };

        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component> { MakeComponent(5) });

        sut.Engine.Setup(e => e.ValidateAsync(It.IsAny<ConfigurationRequest>(), It.IsAny<List<ComponentModel>>()))
            .ReturnsAsync(new ConfigurationResult { IsSuccess = true });

        await sut.Service.SaveConfigurationAsync(request);

        sut.ConfigurationItemRepo.Verify(r => r.InsertAsync(
            It.Is<ConfigurationItem>(item => item.Quantity == 1)), Times.Once);
    }

    // =========================================================================
    // ExportConfigurationAsync
    // =========================================================================

    [Fact]
    public async Task ExportConfigurationAsync_CallsFileBuilderAndReturnsPdfBytes()
    {
        var sut = Create();
        var config = MakeConfiguration(id: 1, specialistId: 5);
        var specialist = MakeSpecialist(id: 5);
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        sut.ConfigurationRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(config);
        sut.ConfigurationItemRepo.Setup(r => r.GetByConfigurationIdAsync(1)).ReturnsAsync(new List<ConfigurationItem>());
        sut.SpecialistRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(specialist);
        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Component>());
        sut.CategoryRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Category>());

        sut.FileBuilder.Setup(f => f.Build(It.IsAny<ConfigurationExportModel>())).Returns(pdfBytes);

        var result = await sut.Service.ExportConfigurationAsync(configurationId: 1);

        Assert.Equal(pdfBytes, result);
        sut.FileBuilder.Verify(f => f.Build(It.IsAny<ConfigurationExportModel>()), Times.Once);
    }

    [Fact]
    public async Task ExportConfigurationAsync_PassesCorrectConfigurationIdToItemRepo()
    {
        var sut = Create();
        sut.ConfigurationRepo.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync(MakeConfiguration(id: 99, specialistId: 5));
        sut.ConfigurationItemRepo.Setup(r => r.GetByConfigurationIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<ConfigurationItem>());
        sut.SpecialistRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(MakeSpecialist());
        sut.ComponentRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Component>());
        sut.CategoryRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Category>());

        sut.FileBuilder.Setup(f => f.Build(It.IsAny<ConfigurationExportModel>())).Returns([]);

        await sut.Service.ExportConfigurationAsync(configurationId: 99);

        sut.ConfigurationItemRepo.Verify(r => r.GetByConfigurationIdAsync(99), Times.Once);
    }

    // =========================================================================
    // GetConfigurationsBySpecialistAsync
    // =========================================================================

    [Fact]
    public async Task GetConfigurationsBySpecialistAsync_ReturnsMappedConfigurations()
    {
        var sut = Create();
        sut.ConfigurationRepo.Setup(r => r.GetBySpecialistIdAsync(3))
            .ReturnsAsync(new List<Configuration>
            {
                MakeConfiguration(id: 1, specialistId: 3),
                MakeConfiguration(id: 2, specialistId: 3)
            });

        var result = await sut.Service.GetConfigurationsBySpecialistAsync(specialistId: 3);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetConfigurationsBySpecialistAsync_EmptyRepo_ReturnsEmptyList()
    {
        var sut = Create();
        sut.ConfigurationRepo.Setup(r => r.GetBySpecialistIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Configuration>());

        var result = await sut.Service.GetConfigurationsBySpecialistAsync(specialistId: 1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetConfigurationsBySpecialistAsync_MapsSpecialistIdCorrectly()
    {
        var sut = Create();
        sut.ConfigurationRepo.Setup(r => r.GetBySpecialistIdAsync(7))
            .ReturnsAsync(new List<Configuration>
            {
                MakeConfiguration(id: 10, specialistId: 7)
            });

        var result = await sut.Service.GetConfigurationsBySpecialistAsync(specialistId: 7);

        Assert.Equal(7, result[0].SpecialistId);
    }
}