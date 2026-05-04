using System;
using System.Reflection;
using MigraDoc.DocumentObjectModel;
using WheelchairConfigurator.Export.Pdf;
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf;

public class PdfBuilderTest
{
    [Fact]
    public void CreateDocument_SetsCorrectFontAndTitle()
    {
        var methodInfo = typeof(PdfBuilder).GetMethod("CreateDocument", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(methodInfo);

        var document = (Document)methodInfo!.Invoke(null, null)!;

        Assert.NotNull(document);
        Assert.Equal("Wheelchair Configuration", document.Info.Title);
        Assert.Equal("Roboto", document.Styles["Normal"]?.Font.Name);
        Assert.Equal(12, document.Styles["Normal"]?.Font.Size);
    }

    [Fact]
    public void ConfigureSection_SetsA4AndCorrectMargins()
    {
        var document = new Document();
        var section = document.AddSection();
        var methodInfo = typeof(PdfBuilder).GetMethod("ConfigureSection", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(methodInfo);

        methodInfo!.Invoke(null, new object[] { section });

        var setup = section.PageSetup;
        Assert.Equal(PageFormat.A4, setup.PageFormat);
        Assert.Equal(Unit.FromCentimeter(2), setup.LeftMargin);
        Assert.Equal(Unit.FromCentimeter(2), setup.RightMargin);
        Assert.Equal(Unit.FromCentimeter(3), setup.TopMargin);
        Assert.Equal(Unit.FromCentimeter(1), setup.BottomMargin);

        Assert.Equal(Unit.FromCentimeter(0.5), setup.HeaderDistance);
        Assert.Equal(Unit.FromCentimeter(0.5), setup.FooterDistance);
    }
}