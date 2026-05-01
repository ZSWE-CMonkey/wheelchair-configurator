using System;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using WheelchairConfigurator.Export.ExportModel;
using WheelchairConfigurator.Export.Pdf.Components;
using WheelchairConfigurator.Export.Tests;
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf.Components;

public class MetadataComponentTest
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Section CreateSection()
    {
        var document = new Document();
        return document.AddSection();
    }

    private static ConfigurationExportModel CreateModel(
        string configName = "Test Config",
        string specialistName = "Jane Doe",
        DateTime? createdAt = null)
    {
        return new ConfigurationExportModel
        {
            ConfigurationName = configName,
            SpecialistName = specialistName,
            CreatedAt = createdAt ?? new DateTime(2024, 6, 15, 10, 30, 0)
        };
    }

    // -------------------------------------------------------------------------
    // Paragraph count
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_AddsExactlyThreeParagraphs()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        Assert.Equal(3, section.Elements.OfType<Paragraph>().Count());
    }

    // -------------------------------------------------------------------------
    // Content: ConfigurationName
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_FirstParagraph_ContainsKonfiguraceLabel()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        var first = section.Elements.OfType<Paragraph>().ElementAt(0);
        Assert.Contains("Konfigurace:", first.GetRawText());
    }

    [Fact]
    public void Render_FirstParagraph_ContainsConfigurationNameValue()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel(configName: "My Custom Config")).Render(section);

        var first = section.Elements.OfType<Paragraph>().ElementAt(0);
        Assert.Contains("My Custom Config", first.GetRawText());
    }

    // -------------------------------------------------------------------------
    // Content: SpecialistName
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_SecondParagraph_ContainsSpecialistaLabel()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        var second = section.Elements.OfType<Paragraph>().ElementAt(1);
        Assert.Contains("Specialista:", second.GetRawText());
    }

    [Fact]
    public void Render_SecondParagraph_ContainsSpecialistNameValue()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel(specialistName: "Dr. Smith")).Render(section);

        var second = section.Elements.OfType<Paragraph>().ElementAt(1);
        Assert.Contains("Dr. Smith", second.GetRawText());
    }

    // -------------------------------------------------------------------------
    // Content: CreatedAt
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_ThirdParagraph_ContainsVytvorenolabel()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        var third = section.Elements.OfType<Paragraph>().ElementAt(2);
        Assert.Contains("Vytvořeno:", third.GetRawText());
    }

    [Fact]
    public void Render_ThirdParagraph_ContainsFormattedDate()
    {
        var date = new DateTime(2024, 6, 15, 10, 30, 0);
        var section = CreateSection();
        new MetadataComponent(CreateModel(createdAt: date)).Render(section);

        var third = section.Elements.OfType<Paragraph>().ElementAt(2);
        // We expect the "g" format (short date/time) as used in the component
        Assert.Contains(date.ToString("g"), third.GetRawText());
    }

    // -------------------------------------------------------------------------
    // Spacing
    // -------------------------------------------------------------------------

    [Fact]
    public void Render_FirstParagraph_HasPositiveSpaceBefore()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        var first = section.Elements.OfType<Paragraph>().ElementAt(0);
        Assert.True(first.Format.SpaceBefore.Point > 0);
    }

    [Fact]
    public void Render_ThirdParagraph_HasPositiveSpaceAfter()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        var third = section.Elements.OfType<Paragraph>().ElementAt(2);
        Assert.True(third.Format.SpaceAfter.Point > 0);
    }

    [Fact]
    public void Render_SecondParagraph_HasNoExtraSpaceBefore()
    {
        var section = CreateSection();
        new MetadataComponent(CreateModel()).Render(section);

        // The middle row has default spaceBefore = 0
        var second = section.Elements.OfType<Paragraph>().ElementAt(1);
        Assert.Equal(0, second.Format.SpaceBefore.Point);
    }
}