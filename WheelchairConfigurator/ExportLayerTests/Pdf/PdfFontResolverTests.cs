using System;
using WheelchairConfigurator.Export.Pdf;
using Xunit;

namespace WheelchairConfigurator.Export.Tests.Pdf;

public class PdfFontResolverTest
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    [Fact]
    public void Instance_IsSingleton()
    {
        var a = PdfFontResolver.Instance;
        var b = PdfFontResolver.Instance;
        Assert.Same(a, b);
    }

    // -------------------------------------------------------------------------
    // DefaultFontName
    // -------------------------------------------------------------------------

    [Fact]
    public void DefaultFontName_IsRoboto()
    {
        Assert.Equal("Roboto", PdfFontResolver.Instance.DefaultFontName);
    }

    // -------------------------------------------------------------------------
    // RegisterFont / GetFont
    // -------------------------------------------------------------------------

    [Fact]
    public void GetFont_AfterRegistering_ReturnsSameBytes()
    {
        var resolver = PdfFontResolver.Instance;
        var bytes = new byte[] { 1, 2, 3 };
        resolver.RegisterFont("TestFontA", bytes);

        var result = resolver.GetFont("TestFontA");
        Assert.Equal(bytes, result);
    }

    [Fact]
    public void RegisterFont_IsCaseInsensitive()
    {
        var resolver = PdfFontResolver.Instance;
        var bytes = new byte[] { 10, 20, 30 };
        resolver.RegisterFont("CaseFontB", bytes);

        // Look up with different casing
        var result = resolver.GetFont("casefontb");
        Assert.Equal(bytes, result);
    }

    [Fact]
    public void RegisterFont_OverwritesExistingEntry()
    {
        var resolver = PdfFontResolver.Instance;
        var originalBytes = new byte[] { 1 };
        var updatedBytes = new byte[] { 2 };

        resolver.RegisterFont("OverwriteFont", originalBytes);
        resolver.RegisterFont("OverwriteFont", updatedBytes);

        Assert.Equal(updatedBytes, resolver.GetFont("OverwriteFont"));
    }

    [Fact]
    public void GetFont_UnknownFontName_ThrowsInvalidOperationException()
    {
        var resolver = PdfFontResolver.Instance;
        var ex = Assert.Throws<InvalidOperationException>(
            () => resolver.GetFont("NonExistentFont_XYZ_12345"));

        Assert.Contains("NonExistentFont_XYZ_12345", ex.Message);
    }

    [Fact]
    public void GetFont_ExceptionMessage_ContainsRegisterFontHint()
    {
        var resolver = PdfFontResolver.Instance;
        var ex = Assert.Throws<InvalidOperationException>(
            () => resolver.GetFont("AnotherMissingFont"));

        Assert.Contains("RegisterFont", ex.Message);
    }

    // -------------------------------------------------------------------------
    // ResolveTypeface
    // -------------------------------------------------------------------------

    [Fact]
    public void ResolveTypeface_AnyFamilyName_ReturnsFontResolverInfo()
    {
        var result = PdfFontResolver.Instance.ResolveTypeface("Arial", isBold: false, isItalic: false);
        Assert.NotNull(result);
    }

    [Fact]
    public void ResolveTypeface_AnyFamilyName_AlwaysResolvesToRoboto()
    {
        var result = PdfFontResolver.Instance.ResolveTypeface("SomeFont", isBold: true, isItalic: true);
        Assert.Equal("Roboto", result!.FaceName);
    }

    [Theory]
    [InlineData("Arial", false, false)]
    [InlineData("Times New Roman", true, false)]
    [InlineData("Helvetica", false, true)]
    [InlineData("Courier", true, true)]
    public void ResolveTypeface_VariousInputs_AlwaysReturnsNonNull(
        string family, bool bold, bool italic)
    {
        var result = PdfFontResolver.Instance.ResolveTypeface(family, bold, italic);
        Assert.NotNull(result);
    }
}