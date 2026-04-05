namespace WheelchairConfigurator.ServiceLayer.Models;

/// <summary>
/// UI model for a category.
/// UI never works directly with domain entities from DataLayer.
/// </summary>
public class CategoryModel
{
    /// <summary>Database ID of the category.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the category.</summary>
    public string Name { get; set; } = string.Empty;
}