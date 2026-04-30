namespace ConfigurationLogic.DTOs;

// Component output payload
public class ComponentOutputDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryRoleKey { get; set; } = "unknown";
    public string? CatalogUrl { get; set; }
    public decimal Price { get; set; }
}

