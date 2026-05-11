namespace WheelchairConfigurator.Data.DTOs;

public class ComponentDto
{
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? CatalogUrl { get; set; }
    public decimal Price { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string ManufacturerCode { get; set; } = string.Empty;
}